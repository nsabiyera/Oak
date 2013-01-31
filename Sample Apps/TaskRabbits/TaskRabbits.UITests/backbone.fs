module backbone

open canopy
open OpenQA.Selenium
open System

let home = @"http://localhost:3000/Home/Backbone"
let selectARabbit = "#rabbitsDropDown_chzn a div"
let add = ".icon-plus"
let rabbits = "li.rabbit"
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