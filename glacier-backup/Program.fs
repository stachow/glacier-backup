open System
open System.IO
open System.Security.Cryptography

type fileAtrrs = { name : string; length : int64; hash : string; relativePath: string; }

let fullHashFileLimit = 10 * 1024 * 1024

[<EntryPoint>]
let main argv = 
    
    //let directoryPath = @"C:\personal\photos\photos\disc-06"
    //let directoryPath = @"C:\personal\photos\photos\2013-12-04"
    let directoryPath = @"C:\personal\photos\photos\"

    let resultsPath = @"c:\temp\file-results.txt"

    let getFilesFromPath path =
        Directory.EnumerateFiles (path, "*.*", SearchOption.AllDirectories)

    let getFileInfo filePath = 
        new FileInfo(filePath)

    let getHash (fileInfo : FileInfo) = 

        let getFileStream filePath =
            File.OpenRead filePath 
            |> (fun stream -> new BufferedStream(stream))
            :> Stream

        let getFirst10MBStream filePath =
            let fs = getFileStream filePath
            let buf, read, offset  = Array.zeroCreate fullHashFileLimit, ref 0, ref 0
            let leftToRead = ref buf.Length

            let doRead() = 
                read := fs.Read(buf, offset.Value, leftToRead.Value)
                read

            while leftToRead.Value > 0 && doRead().Value > 0 do
                  leftToRead := leftToRead.Value - read.Value
                  offset := offset.Value + read.Value
        
            new MemoryStream(buf)
            :> Stream

        let getMd5 stream =
            use md5 = new MD5CryptoServiceProvider()
            md5.ComputeHash (stream : Stream) 
            |> BitConverter.ToString
            |> (fun x -> x.Replace("-", ""))

        let getAppropriateStreamFn fileSize =
            match fileSize with
                | size when size > int64(fullHashFileLimit) -> getFirst10MBStream
                | _ -> getFileStream

        fileInfo.FullName |>
        (fileInfo.Length |> getAppropriateStreamFn)
        |> getMd5

    let getFileAttrs (fileInfo : FileInfo) =
        { name = fileInfo.Name; length = fileInfo.Length; hash = getHash fileInfo; relativePath = fileInfo.FullName }
        
    let clearResults = 
        File.WriteAllText (resultsPath, "")
    
    let writeResult msg = 
        File.AppendAllText (resultsPath, msg)
        File.AppendAllText (resultsPath, "\n")
        printfn "%s" msg

    let report fileInfo =
        printfn "%s %i %s %s" fileInfo.name fileInfo.length fileInfo.hash fileInfo.relativePath
        
    clearResults
    directoryPath
        |> getFilesFromPath
        |> Seq.map getFileInfo   
        |> Seq.map getFileAttrs               
        |> Seq.iter report

    printfn "Done"
    let x = Console.ReadLine()
    0 // return an integer exit code
