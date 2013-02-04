module backbone

open canopy
open common

let selectors = 
    {
        home = @"http://localhost:3000/Home/Backbone"        
        selectARabbit = "#rabbitsDropDown_chzn a div"
        add = ".icon-plus"
        rabbits = "li.rabbit"
        newRabbitName = "#newRabbitName"
        save = "#createRabbit"
        addTask = "#addTask"
        description = ".description"
        date = ".date"
        today = System.DateTime.Today.ToString()
        saveTask = "input[value='save']"
        tasks = "#tasks tr"
        error = ".alert-error"
        suiteName = "Backbone"
    }