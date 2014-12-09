#I "../../lib/"

#r "MBrace.Core.dll"
#r "MBrace.Library.dll"
#r "FsPickler.dll"
#r "Vagrant.dll"
#r "MBrace.Azure.Runtime.Common.dll"
#r "MBrace.Azure.Runtime.dll"
#r "MBrace.Azure.Client.dll"
#r "Streams.Core.dll"
#r "MBrace.Streams.dll"

let private selectEnv name =
    (System.Environment.GetEnvironmentVariable(name,System.EnvironmentVariableTarget.User),
        System.Environment.GetEnvironmentVariable(name,System.EnvironmentVariableTarget.Machine))
    |> function | null, s | s, null | s, _ -> s

let azureStorageConn = selectEnv "azurestorageconn"

let serviceBusConn = selectEnv "azureservicebusconn"

(*
#r "MBrace.Azure.Runtime.Standalone"
open Nessos.MBrace.Azure.Runtime.Standalone
Runtime.WorkerExecutable <- __SOURCE_DIRECTORY__ + "/../../lib/MBrace.Azure.Runtime.Standalone.exe"
Runtime.Spawn(config, 4)

runtime.ClientLogger.Attach(Common.ConsoleLogger())
*)