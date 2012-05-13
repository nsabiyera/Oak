open runner

open canopy

open uimethods

open setupmethods

start "firefox"

before <- fun _ -> logOff()

test(fun _ ->
    describe "borrower returns game"
    requestGame()
    logOff()
    loginAs "user2"
    wantedGamesList *= "Borrowed"
    click deleteBorrowedGameLink
    wantedGamesList *!= "Borrowed")

test(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1")

test(fun _ ->
    describe "request games"
    requestGame())

test(fun _ ->
    describe "lender marks game as returned"
    requestGame()
    loginAs "user1"
    "#requestedGames a" *= "The game has been returned"
    click gameReturnedLink
    count "#requestedGames a" 0)

run()
 
quit()
