
#load "helpers.fsx"

open Nessos.MBrace
open Nessos.MBrace.Azure
open Nessos.MBrace.Azure.Client
open Nessos.MBrace.Azure.Runtime


let config = 
    { Configuration.Default with
        StorageConnectionString = Helpers.azureStorageConn
        ServiceBusConnectionString = Helpers.serviceBusConn }


let runtime = Runtime.GetHandle(config)


runtime.ShowWorkers()
runtime.ShowLogs()

runtime.Run(cloud { return "Hello world"})



open Nessos.Streams
open Nessos.MBrace.Streams

let xs = 
    [|1..10|]
    |> CloudStream.ofArray
    |> CloudStream.map (fun x -> x * x)
    |> CloudStream.toArray


runtime.Run xs

let ps1 = runtime.CreateProcess(CloudFile.Enumerate "wikipedia", name = "enumeratefiles")
let files = ps1.AwaitResult()

let getTop files count =
    files
    |> CloudStream.ofCloudFiles CloudFile.ReadAllText
    |> CloudStream.collect (fun text -> Helpers.splitWords text |> Stream.ofArray |> Stream.map Helpers.wordTransform)
    |> CloudStream.filter Helpers.wordFilter
    |> CloudStream.countBy id
    |> CloudStream.sortBy (fun (_,c) -> -c) count
    |> CloudStream.toArray

let ps = runtime.CreateProcess(getTop files 20, name = "getTop with cloudfiles")
let r = ps.AwaitResult()


let mkCloudArray files =
    cloud {
        let! cloudarray =
            files
            |> CloudStream.ofCloudFiles CloudFile.ReadAllText
            |> CloudStream.collect (fun text -> Helpers.splitWords text |> Stream.ofArray |> Stream.map Helpers.wordTransform)
            |> CloudStream.filter Helpers.wordFilter
            |> CloudStream.toCloudArray
        return! CloudStream.cache cloudarray
    }

let getTop' cloudarray count =
    cloudarray
    |> CloudStream.ofCloudArray
    |> CloudStream.countBy id
    |> CloudStream.sortBy (fun (_,c) -> -c) count
    |> CloudStream.toArray

let caProcess = runtime.CreateProcess(mkCloudArray files, name = "create cloudarray")
let ca = caProcess.AwaitResult()

let ps' = runtime.CreateProcess(getTop' ca 20, name = "getTop with cloudarray")
let r' = ps'.AwaitResult()


// PREPROCESS

let splitByChunkSize chunkSize (ts : 'T []) =
    if chunkSize <= 0 then invalidArg "chunkSize" "must be positive."
    elif chunkSize > ts.Length then invalidArg "chunkSize" "chunk size greater than array size."
    let q, r = ts.Length / chunkSize , ts.Length % chunkSize
    [|
        for i in 0 .. q-1 do
            yield ts.[ i * chunkSize .. (i + 1) * chunkSize - 1]

        if r > 0 then yield ts.[q * chunkSize .. ]
    |]

let preprocess =
    cloud {
        do! Cloud.Log "Reading files"
        let! files = CloudFile.Enumerate "wikipedia"
        do! Cloud.Log(sprintf "Read %d files" files.Length)
        let split = files |> splitByChunkSize 15000
        do! Cloud.Log(sprintf "Split in  %d " split.Length)
        return! 
            split 
            |> Array.map (fun files -> 
                cloud {
                    let lines = seq {
                        for f in files do
                            let text = Async.RunSynchronously(f.Read(CloudFile.ReadAllText))
                            yield text
                    }
                    return! CloudFile.WriteLines(lines, path = "wiki/" + System.Guid.NewGuid().ToString("N")) })
            |> Cloud.Parallel
    }

let p = runtime.CreateProcess preprocess

p.ShowLogs()

p.AwaitResult()



