open runner

open canopy

open uimethods

open setupmethods

start "firefox"

before(fun _ -> logOff())

xtest(fun _ ->
    describe "borrower returns game"
    requestGame()
    logOff()
    loginAs "user2"
    wantedGamesList *= "currently borrowing"
    click deleteBorrowedGameLink
    wantedGamesList *!= "currently borrowing")

wip(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1")

xtest(fun _ ->
    describe "request games"
    requestGame())

xtest(fun _ ->
    describe "lender marks game as returned"
    requestGame()
    loginAs "user1"
    click gameReturnedLink
    count "#requestedGames i" 0)

run()
 
quit()
