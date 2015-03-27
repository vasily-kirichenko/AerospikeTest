open Aerospike.Client
open System
open System.Threading
open System.Diagnostics

[<EntryPoint>]
let main = function
    | [|threads|] ->
        let threads = int threads
        let count = 10000000
        use countdown = new CountdownEvent(threads)
        let worker from to' =
            printfn "Writing %d..%d..." from to'
            use client = new AerospikeClient("localhost", 3000)
            let put (key: int) =
                client.Put(null, Key("test", "status", key), Bin("scanResult", "{ json }"))
            for i in from..to' do put i
            countdown.Signal() |> ignore
    
        let sw = Stopwatch.StartNew()
        let chunk = count / threads
        for thread in 0..threads-1 do
            Thread(fun() -> worker (thread * chunk) (thread * chunk + chunk)).Start()
        countdown.Wait()
        sw.Stop()
        printfn "Done in %O, %0.1f /s" sw.Elapsed (float count / float sw.ElapsedMilliseconds * 1000.)
        0
    | _ -> failwith "Wrong arguments, expected <threads>"
