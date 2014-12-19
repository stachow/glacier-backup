module zip_test

    open System.IO
    open System.IO.Compression

    let zipFiles (filePaths: string seq) = 
        
        let addFilesToMemoryStream memoryStream = 
            use archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)

            let addFileToArchive path = 
                archive.CreateEntryFromFile( path, Path.GetFileName(path)) |> ignore
                printfn "Done %s" path

            filePaths
            |> Seq.iter addFileToArchive
            |> ignore

        use memoryStream = new MemoryStream()
        addFilesToMemoryStream memoryStream


        use outputStream = new FileStream(@"c:\temp\testZip.zip", FileMode.Create)
        memoryStream.Seek(0L, SeekOrigin.Begin) |> ignore
        memoryStream.WriteTo(outputStream)
