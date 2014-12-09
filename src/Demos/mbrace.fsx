
#load "helpers.fsx"

open Nessos.MBrace
open Nessos.MBrace.Azure
open Nessos.MBrace.Azure.Client
open Nessos.MBrace.Azure.Runtime


let config = 
    { Configuration.Default with
        StorageConnectionString = Helpers.azureStorageConn
        ServiceBusConnectionString =  Helpers.serviceBusConn }


let runtime = Runtime.GetHandle(config)


runtime.ShowWorkers()
runtime.ShowLogs()

runtime.Run(cloud { return "Hello world"})



open Nessos.MBrace.Streams

let xs = 
    [|1..10|]
    |> CloudStream.ofArray
    |> CloudStream.map (fun x -> x * x)
    |> CloudStream.toArray


runtime.Run xs
