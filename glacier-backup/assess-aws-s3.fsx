open System.IO
#r @"bin\Debug\AWSSDK.dll"
open Amazon
open Amazon.S3.Model

let secrets = File.ReadAllLines (__SOURCE_DIRECTORY__ + "\config.secret.txt")
let awsAccessKey = secrets.[0]
let awsSecretKey = secrets.[1]        

let manifestBucketName = "glacier-backup-manifest"
let manifestObjectName = "glacier-backup-manifest.zip"

let getClient() = 
    AWSClientFactory.CreateAmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.EUWest1)

let getFileStream filePath =
    File.OpenRead filePath 
    |> (fun stream -> new BufferedStream(stream))
    :> Stream

let ensureBucketExists() = 
    use client = getClient()
    let req = new PutBucketRequest()
    req.BucketName <- manifestBucketName
    try
        client.PutBucket(req) |> ignore
    with
        | _ -> ()

let listBuckets() = 
    use client = getClient()
    let buckets = client.ListBuckets().Buckets
    buckets |> Seq.iter (fun b -> printfn "%s" b.BucketName)

let storeFile objectName fileStream =
    use client = getClient()
    let req = new PutObjectRequest()
    req.BucketName <- manifestBucketName
    req.Key <- manifestObjectName
    req.InputStream <- fileStream
    try
        client.PutObject(req) |> ignore
    with
        | _ -> ()

ensureBucketExists()
listBuckets()
storeFile manifestObjectName (getFileStream @"C:\temp\file-results.zip")

printfn "%s %s" awsAccessKey awsSecretKey