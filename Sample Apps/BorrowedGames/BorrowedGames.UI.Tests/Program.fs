// Learn more about F# at http://fsharp.net

open firefox

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

describe "register user"
reset()
url "http://localhost:3000"
find "register" |> click
write "#Email" "user@example.com"
write "#Password" "Password"
write "#PasswordConfirmation" "Password"
click "input[value='register']"
url "http://localhost:3000/account/logoff"

describe "login"
reset()
given_user "user@example.com" "Password"
url "http://localhost:3000"
write "#Email" "user@example.com"
write "#Password" "Password"
click "input[value='login']"
url "http://localhost:3000/account/logoff"