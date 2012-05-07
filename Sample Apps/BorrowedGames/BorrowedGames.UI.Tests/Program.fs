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
    sleep() //elements with text bug needs to be fixed before this sleep can be removed
    is (numberOfBorrowedGames()) 1
    click deleteBorrowedGameLink
    sleep() //elements with text bug needs to be fixed before this sleep can be removed
    is (numberOfBorrowedGames()) 0)

test(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1")

test(fun _ ->
    describe "request games"
    requestGame())

wip(fun _ ->
    describe "lender marks game as returned"
    requestGame()
    is (numberOfRequestedGames()) 1
    click gameReturnedLink
    is (numberOfRequestedGames()) 0)

run()
 
quit()