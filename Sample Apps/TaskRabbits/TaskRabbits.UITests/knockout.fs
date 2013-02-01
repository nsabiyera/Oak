module knockout

open canopy
open OpenQA.Selenium
open System
open common
open runner


let home = @"http://localhost:3000/Home/Knockout"
let selectARabbit = "#rabbitsDropDown_chzn a div"
let add = ".icon-plus"
let rabbits = ".chzn-results li"
let newRabbitName = "#newRabbitName"
let save = "#createRabbit"
let addTask = "#addTask"
let description = ".description"
let date = ".date"
let today = DateTime.Today.ToString()
let saveTask = "input[value='save']"
let tasks = "#tasks tr"

//helpers
let addRabbit name = 
    click add
    newRabbitName << name
    click save
    displayed add

let clickRabbit name = 
    click selectARabbit
    elements rabbits
    |> List.filter (fun rabbit -> rabbit.Text = name)
    |> List.head
    |> click

//tests
let tests _ =
    context "Knockout Suite"
    
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