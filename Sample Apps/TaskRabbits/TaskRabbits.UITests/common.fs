module common

let reset _ =   
    let seed = new Oak.Controllers.SeedController()
    seed.PurgeDb() |> ignore
    seed.All() |> ignore
    () 