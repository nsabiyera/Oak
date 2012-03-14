// Learn more about F# at http://fsharp.net

open runner
open canopy

type args = { Email : string; Password : string; PasswordConfirmation : string }

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

test(fun _ -> registerUser "user1@example.com")

test(fun _ ->
    describe "request games"
    reset()
    registerUser "user1@example.com"
    registerUser "user2@example.com")

run()
 
quit()

//describe "login"
//reset()
//given_user "user@example.com" "Password"
//url "http://localhost:3000"
//write "#Email" "user@example.com"
//write "#Password" "Password"
//click "input[value='login']"
//url "http://localhost:3000/account/logoff"