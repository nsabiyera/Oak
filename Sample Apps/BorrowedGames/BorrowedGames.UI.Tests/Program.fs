open canopy
open runner
open uimethods
open setupmethods
open configuration
open reporters

start firefox

let (====) a b = a &&& b

context "borrowedgames.com"

before(fun _ -> logOff())

"borrower returns game" 
==== fun _ ->
    requestGame()
    logOff()
    loginAs "user2"
    wantedGamesList *= "currently borrowing"
    click deleteBorrowedGameLink
    wantedGamesList *!= "currently borrowing"

"registering a user" 
==== fun _ -> 
    reset()
    registerUser "user1"

"request games" 
==== fun _ -> 
    requestGame()

"lender marks game as returned" 
==== fun _ ->
    requestGame()
    loginAs "user1"
    click gameReturnedLink
    count "#requestedGames i" 0

run()

quit()

