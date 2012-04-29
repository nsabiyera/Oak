open runner

open canopy

open uimethods

open setupmethods

start "firefox"

compareTimeout <- 10.0

elementTimeout <- 10.0

pageTimeout <- 10.0

xtest(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1")

test(fun _ ->
    describe "request games"
    requestGame()
    logOff())

xtest(fun _ ->
    describe "lender marks game as returned"
    requestGame()
    System.Console.ReadLine() |> ignore
    click gameReturnedLink)

run()
 
quit()