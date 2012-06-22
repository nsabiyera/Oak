open runner
open System.Linq

open canopy

start firefox

url "http://www.google.com"

start firefox

url "http://www.bing.com"

browser.SwitchTo().Window(browser.WindowHandles.Last())

url "http://www.duckduckgo.com"

browser.WindowHandles |> Seq.toList
|> List.map (fun a -> System.Console.WriteLine(a.ToString())) 

run()

System.Console.ReadKey()