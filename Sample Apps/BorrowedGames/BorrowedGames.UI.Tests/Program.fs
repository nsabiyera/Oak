open canopy
open canopy.core
open runner
open uimethods
open setupmethods
open configuration

start firefox

context "borrowedgames.com"

before(fun _ -> logOff())

test(fun _ ->
    describe "borrower returns game" 
    requestGame()
    logOff()
    loginAs "user2"
    wantedGamesList *= "currently borrowing"
    click deleteBorrowedGameLink
    wantedGamesList *!= "currently borrowing"
)

test(fun _ ->
    describe "registering a user" 
    reset()
    registerUser "user1"
)

test(fun _ ->
    describe "request games" 
    requestGame()
)

test(fun _ ->
    describe "lender marks game as returned"
    requestGame()
    loginAs "user1"
    click gameReturnedLink
    count "#requestedGames i" 0
)

run()

quit()

