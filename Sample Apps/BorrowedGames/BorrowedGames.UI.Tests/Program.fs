open runner

open canopy

open uimethods

open setupmethods

start "firefox"

compareTimeout <- 10.0

elementTimeout <- 10.0

pageTimeout <- 10.0

test(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1")

test(fun _ ->
    describe "request games"
    reset()
    stageGame "Dark Souls"
    registerUser "user1"
    registerUser "user2"
    loginAs "user1"
    addGame "Dark Souls"
    logOff()
    loginAs "user2"
    follow "user1"
    requestFirstGame()
    logOff()
    loginAs "user1"
    giveFirstRequestedGame()
    logOff())

run()
 
quit()