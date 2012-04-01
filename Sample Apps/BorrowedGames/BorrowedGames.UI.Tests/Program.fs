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

wip(fun _ ->
    describe "request games"
    reset()
    stageGame "Dark Souls"
    stageGame "Mirror's Edge"
    registerUser "user1"
    registerUser "user2"
    loginAs "user1"
    addGame "Mirror's Edge"
    addGame "Dark Souls"
    logOff()
    loginAs "user2"
    follow "user1"
    request "Dark Souls")

run()


 
quit()