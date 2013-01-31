open runner
open canopy
open backbone
open System

start firefox

//helper to reset database
let reset _ =   
    let seed = new Oak.Controllers.SeedController()
    seed.PurgeDb() |> ignore
    seed.All() |> ignore
    () 

before(fun _ -> 
    reset ()
    url home
    on home)

test(fun _ ->
    describe "fresh database has no rabbits in drop down"
    click selectARabbit    
    count rabbits 0    
)

test(fun _ ->
    describe "adding one rabbit creates one rabbit in drop down"
    addRabbit "Aldous"
    click selectARabbit        
    count rabbits 1
    rabbits *= "Aldous"
)

test(fun _ ->
    describe "adding two rabbits creates two rabbits in drop down"
    addRabbit "Aldous"
    addRabbit "Holden"
    click selectARabbit
    count rabbits 2
    rabbits *= "Aldous"
    rabbits *= "Holden"
)

test(fun _ ->
    describe "adding task for Holden shows for him but not Aldous"
    addRabbit "Aldous"
    addRabbit "Holden"    
    clickRabbit "Holden"
    click addTask
    description << "Go for a walk"
    date << today
    click saveTask
    clickRabbit "Aldous"
    count tasks 0
    clickRabbit "Holden"
    count tasks 1
)

run()
System.Console.ReadKey() |> ignore
quit()