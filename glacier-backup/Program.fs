open System
open System.IO
open System.Security.Cryptography

type fileAtrrs = { name : string; length : int64; hash : string; relativePath: string; }

let fullHashFileLimit =   512 * 1024

[<EntryPoint>]
let main argv = 
    
    //let directoryPath = @"C:\personal\photos\photos\disc-06"
    //let directoryPath = @"C:\personal\photos\photos\2013-12-04"
    let directoryPath = @"C:\personal\photos\photos\"

    let resultsPath = @"c:\temp\file-results_0.5MB.txt"

    let getFilesFromPath path =
        Directory.EnumerateFiles (path, "*.*", SearchOption.AllDirectories)

    let getFileInfo filePath = 
        new FileInfo(filePath)

    let getHash (fileInfo : FileInfo) = 

        let getFileStream filePath =
            File.OpenRead filePath 
            |> (fun stream -> new BufferedStream(stream))
            :> Stream

        let getFirstNBytestream N filePath=
            let fs = getFileStream filePath
            let emptyBuf = Array.zeroCreate N

            let rec readStream buf (fs:Stream) start take =
                if take <= 0 then buf
                else
                    let readBytes = fs.Read (buf, start, take)
                    if readBytes = 0 then buf
                    else readStream buf fs (start + readBytes) (take - readBytes)
        
            let buf = readStream emptyBuf fs 0 fullHashFileLimit
            
            new MemoryStream(buf)
            :> Stream

        let getMd5 stream =
            use md5 = new MD5CryptoServiceProvider()
            md5.ComputeHash (stream : Stream) 
            |> BitConverter.ToString
            |> (fun x -> x.Replace("-", ""))

        let getAppropriateStreamFn fileSize =
            if fileSize > int64(fullHashFileLimit) then getFirstNBytestream fullHashFileLimit
            else getFileStream
             
        fileInfo.FullName |>
        (getAppropriateStreamFn fileInfo.Length)
        |> getMd5

    let getFileAttrs (fileInfo : FileInfo) =
        { name = fileInfo.Name; length = fileInfo.Length; hash = getHash fileInfo; relativePath = fileInfo.FullName }
        
    let clearResults = 
        File.WriteAllText (resultsPath, "")
    
    let writeResult fileInfo = 
        let msg = fileInfo.name + " " + fileInfo.length.ToString() + " " +  fileInfo.hash 
        File.AppendAllText (resultsPath, msg)
        File.AppendAllText (resultsPath, "\n")
        printfn "%s" msg

    let report fileInfo =
        printfn "%s %i %s %s" fileInfo.name fileInfo.length fileInfo.hash fileInfo.relativePath
        
    clearResults
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    directoryPath
        |> getFilesFromPath
        |> Seq.map getFileInfo   
        |> Seq.map getFileAttrs               
        |> Seq.iter writeResult
    stopWatch.Stop()
    
    printfn "Done %i" (int stopWatch.Elapsed.TotalSeconds)


    let x = Console.ReadLine()
    0 // return an integer exit code
