module knockout

open canopy
open OpenQA.Selenium
open common

let selectors =
    {
        home = @"http://localhost:3000/Home/Knockout"
        selectARabbit = "#rabbitsDropDown_chzn a div"
        add = ".icon-plus"
        rabbits = ".chzn-results li"
        newRabbitName = "#newRabbitName"
        save = "#createRabbit"
        addTask = "#addTask"
        description = ".description"
        date = ".date"
        today = System.DateTime.Today.ToString()
        saveTask = "input[value='save']"
        tasks = "#tasks tr"
        error = ".alert-error"
        suiteName = "Knockout"
    }