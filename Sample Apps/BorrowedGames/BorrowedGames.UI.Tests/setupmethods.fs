module setupmethods

type args = { Email : string; Password : string; PasswordConfirmation : string }

type game = { Name : string; Console : string }

let given_user email password = 
    let registration = 
        new BorrowedGames.Models.Registration(
            { 
                Email = email; 
                Password = password; 
                PasswordConfirmation = password 
            })
    registration.Register() |> ignore
    ()

let reset _ =
    let seed = new Oak.Controllers.SeedController()
    seed.PurgeDb() |> ignore
    seed.All() |> ignore
    ()

let stageGame = fun gameName ->
    let repo = new BorrowedGames.Models.Games()
    repo.Insert({ Name = gameName; Console = "XBOX360" }) |> ignore
    ()

