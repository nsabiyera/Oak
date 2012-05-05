open runner

open canopy

open uimethods

open setupmethods

start "firefox"

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