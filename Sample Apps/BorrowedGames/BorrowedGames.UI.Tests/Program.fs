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
    is (elementsWithText "#wantedGames .brand" "Borrrrrrowed").Length 1)

xtest(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1")

xtest(fun _ ->
    describe "request games"
    requestGame())

xtest(fun _ ->
    describe "lender marks game as returned"
    requestGame()
    is (numberOfRequestedGames()) 1
    click gameReturnedLink
    is (numberOfRequestedGames()) 0)

run()
 
quit()