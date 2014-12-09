
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



open Nessos.MBrace.Streams
open Nessos.Streams

let cloudFiles = CloudFile.Enumerate "shakespeare" |> runtime.Run



#time

let xs = 
    cloudFiles
    |> CloudStream.ofCloudFiles CloudFile.ReadAllLines
    |> CloudStream.collect Stream.ofArray
    |> CloudStream.filter (fun x -> x.ToUpper().Contains("KING") )
    |> CloudStream.toCloudArray


let x = runtime.Run xs

let cached = CloudStream.cache x |> runtime.Run

let result = 
    cached
    |> CloudStream.ofCloudArray
    |> CloudStream.filter (fun x -> x.ToUpper().Contains("KING") )
    |> CloudStream.toCloudArray

result |> runtime.Run