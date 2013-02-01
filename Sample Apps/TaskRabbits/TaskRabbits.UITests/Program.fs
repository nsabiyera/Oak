open backbone
open runner
open canopy
open System

start firefox

backbone.tests()

run()
System.Console.ReadKey() |> ignore
quit()