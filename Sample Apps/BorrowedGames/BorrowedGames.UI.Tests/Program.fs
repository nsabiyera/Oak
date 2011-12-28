// Learn more about F# at http://fsharp.net

open firefox

type args = { Email : string; Password : string; PasswordConfirmation : string }

let given_user email password = 
    let controller = new BorrowedGames.Controllers.AccountController()
    controller.Authenticate <- new System.Action<string>(fun _ -> ())
    controller.Register({ Email = email; Password = password; PasswordConfirmation = password }) |> ignore
    ()

//describe "register user"
//url "http://localhost:3000"
//find "register" |> click
//write "#Email" "user@example.com"
//write "#Password" "Password"
//write "#PasswordConfirmation" "Password"
//click "input[value='register']"

describe "login"
given_user "user@example.com" "Password"
url "http://localhost:3000"
write "#Email" "user@example.com"
write "#Password" "Password"
click "input[value='login']"