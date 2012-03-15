module uimethods

open runner

open canopy

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

let baseUrl = "http://localhost:3000";

let url = fun address -> canopy.url (baseUrl + address)

let on = fun address -> canopy.on(baseUrl + address)

let logOff = fun _ ->
    url "/account/logoff"
    on "/account/logoff"

let registerUser = fun email ->
    url "/"
    on "/Account/LogOn?ReturnUrl=%2f"
    click "a[href='/Account/Register']"
    on "/Account/Register"
    write "#Email" email
    write "#Password" "Password"
    write "#PasswordConfirmation" "Password"
    click "input[value='register']"
    on "/"
    logOff()

let loginAs = fun email ->
    url "/"
    on "/Account/LogOn?ReturnUrl=%2f"
    write "#Email" email
    write "#Password" "Password"
    click "input[value='login']"
    on "/"

let addGame = fun name ->
    click "#showLibrary"
    write "#gameToAdd" name
    click "table tbody tr"
    click "#closeLibraryTop"