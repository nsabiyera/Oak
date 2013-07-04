module common

open canopy
open runner

let reset _ =   
    let seed = new Oak.Controllers.SeedController()
    seed.PurgeDb() |> ignore
    seed.All() |> ignore
    () 

type selectors =
    {
        home : string;
        selectARabbit : string;
        add : string;
        rabbits : string;
        newRabbitName : string;
        save : string;
        addTask : string;
        description : string;
        date : string;
        today : string;
        saveTask : string;
        tasks : string;
        error : string;
        suiteName : string
    }
    
//tests
let tests (s : selectors) =
    //helpers
    let addRabbit name = 
        click s.add
        s.newRabbitName << name
        click s.save
        displayed s.add

    let clickRabbit name = 
        click s.selectARabbit
        elements s.rabbits
        |> List.filter (fun rabbit -> rabbit.Text = name)
        |> List.head
        |> click

    context (System.String.Format("{0} Suite", s.suiteName))

    before(fun _ -> 
    reset ()
    url s.home
    on s.home)

    test(fun _ ->
        describe "fresh database has no rabbits in drop down"
        click s.selectARabbit    
        count s.rabbits 0    
    )

    test(fun _ ->
        describe "adding one rabbit creates one rabbit in drop down"
        addRabbit "Aldous"
        click s.selectARabbit        
        count s.rabbits 1
        s.rabbits *= "Aldous"
    )

    test(fun _ ->
        describe "adding two rabbits creates two rabbits in drop down"
        addRabbit "Aldous"
        addRabbit "Holden"
        click s.selectARabbit
        count s.rabbits 2
        s.rabbits *= "Aldous"
        s.rabbits *= "Holden"
    )

    test(fun _ ->
        describe "adding task for Holden shows for him but not Aldous"
        addRabbit "Aldous"
        addRabbit "Holden"    
        clickRabbit "Holden"
        click s.addTask
        s.description << "Go for a walk"
        s.date << s.today
        click s.saveTask
        clickRabbit "Aldous"
        count s.tasks 0
        clickRabbit "Holden"
        count s.tasks 1
    )

    test(fun _ ->
        describe "adding one rabbit creates one rabbit, adding same rabbit shows error"
        addRabbit "Aldous"
        click s.add
        s.newRabbitName << "Aldous"
        click s.save
        s.error == "Name is taken."
    )

    test(fun _ ->
        describe "trying to add a rabbit without a name gives an error"        
        click s.add
        s.newRabbitName << ""
        click s.save
        s.error == "Name is required."
    )