open System
open System.IO
open System.Security.Cryptography

type fileAtrrs = { name : string; length : int64; hash : string; relativePath: string; }

let fullHashFileLimit =   1 * 1024 * 1024

let generateHashFile() = 

    //let directoryPath = @"C:\personal\photos\photos\disc-06"
    //let directoryPath = @"C:\personal\photos\photos\2013-12-04"
    let directoryPath = @"C:\personal\photos\photos\"

    let resultsPath = @"c:\temp\file-results.txt"

    let getFilesFromPath path =
        Directory.EnumerateFiles (path, "*.*", SearchOption.AllDirectories)

    let getFileInfo filePath = 
        new FileInfo(filePath)

    let getHash (fileInfo : FileInfo) hashLength = 

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
            |> (fun x -> x.Substring(0, hashLength))

        let getAppropriateStreamFn fileSize =
            if fileSize > int64(fullHashFileLimit) then getFirstNBytestream fullHashFileLimit
            else getFileStream
             
        fileInfo.FullName |>
        (getAppropriateStreamFn fileInfo.Length)
        |> getMd5 

    let getFileAttrs (fileInfo : FileInfo) =
        { name = fileInfo.Name; length = fileInfo.Length; hash = getHash fileInfo 8; relativePath = fileInfo.FullName }
        
    let clearResults = 
        File.WriteAllText (resultsPath, "")
    
    let writeResult fileInfo = 
        let msg = fileInfo.hash.Substring(0, 8) + " " + fileInfo.length.ToString()
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
    ()


open zip_test
let zipTest() =
    let directoryPath = @"C:\personal\photos\photos\2013-12-04"
    let filePaths =
        Directory.EnumerateFiles (directoryPath, "*.*", SearchOption.AllDirectories)

    zipFiles filePaths
    printfn "Done zipFiles"
    ()

[<EntryPoint>]
let main argv = 
    printfn "h to generate hash files"
    printfn "z to test zipping"
    
    let choice = Console.ReadLine()
    
    match choice with
    | "h" -> generateHashFile()
    | "z" -> zipTest()
    | _ -> failwith "Unsupported option"

    let x = Console.ReadLine()
    0 // return an integer exit code
