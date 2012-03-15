// Learn more about F# at http://fsharp.net

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

start "firefox"

let registerUser = fun email ->
    url "http://localhost:3000"
    on "http://localhost:3000/Account/LogOn?ReturnUrl=%2f"
    click "a[href='/Account/Register']"
    on "http://localhost:3000/Account/Register"
    write "#Email" email
    write "#Password" "Password"
    write "#PasswordConfirmation" "Password"
    click "input[value='register']"
    on "http://localhost:3000/"
    url "http://localhost:3000/account/logoff"
    on "http://localhost:3000/account/logoff"

let loginAs = fun email ->
    url "http://localhost:3000"
    on "http://localhost:3000/Account/LogOn?ReturnUrl=%2f"
    write "#Email" email
    write "#Password" "Password"
    click "input[value='login']"
    on "http://localhost:3000/"

let addGame = fun name ->
    click "#showLibrary"
    write "#gameToAdd" name
    click "table tbody tr"
    click "#closeLibraryTop"

//test(fun _ -> 
//    describe "registering a user"
//    reset()
//    registerUser "user1@example.com")

test(fun _ ->
    describe "request games"
    reset()
    stageGame "Dark Souls"
    registerUser "user1@example.com"
    registerUser "user2@example.com"
    loginAs "user1@example.com"
    addGame "Dark Souls")

run()
 
//quit()

//describe "login"
//reset()
//given_user "user@example.com" "Password"
//url "http://localhost:3000"
//write "#Email" "user@example.com"
//write "#Password" "Password"
//click "input[value='login']"
//url "http://localhost:3000/account/logoff"