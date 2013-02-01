
open runner
open canopy
open System

start firefox

backbone.tests()
knockout.tests()

run()
System.Console.ReadKey() |> ignore
quit()