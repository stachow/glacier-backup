open System
open System.IO

type splits = {name:string; size:int64; hash: string}

let resultsPath = @"c:\temp\file-results_1MB.txt"

let resLines = File.ReadAllLines resultsPath

let getSplits len (res : string) =
    let revSplits = res.Split(' ')
                    |> Seq.toList
                    |> List.rev

    let (hash, remain) =  List.head revSplits, List.tail revSplits
    let (size, remain) = List.head remain |> int64, List.tail remain
    let backToOriginalOrderArr = List.rev remain |> List.toArray 
    let name = String.Join(" ", backToOriginalOrderArr)

    { name = name; size = size; hash = hash.Substring(0, len) }

resLines
|> Seq.map (getSplits 8)
//|> Seq.iter (fun s -> printfn "%s %i %s" s.name s.size s.hash)
|> Seq.groupBy (fun x -> x.size.ToString() + " " + x.hash)
|> Seq.filter (fun x -> (Seq.length (snd x)) > 1 )
|> Seq.map (fun x -> printfn "%s %i" (fst x) (x |> snd |> Seq.length); x)
|> Seq.length
|> printfn "%i"
//|> Seq.where   (fun x -> snd(x) |> Seq.length > 1) 

//printfn "%i" (Seq.length res)

printfn "Done"
//let x = Console.ReadLine()    