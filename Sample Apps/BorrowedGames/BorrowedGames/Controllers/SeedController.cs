using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using BorrowedGames.Models;

namespace Oak.Controllers
{
    public class LocalOnly : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(filterContext.RequestContext.HttpContext.Request.IsLocal == false)
            {
                filterContext.Result = new HttpNotFoundResult();
            }
        }
    }

    [LocalOnly]
    public class SeedController : Controller
    {
        public Seed Seed { get; set; }

        public SeedController()
        {
            Seed = new Seed();
        }

        /// <summary>
        /// Change this method to create your tables.  Take a look 
        /// at each method, CreateSampleTable(), CreateAnotherSampleTable(), 
        /// AlterSampleTable() and AdHocChange()...you'll want to replace 
        /// this with your own set of methods.
        /// </summary>
        [HttpPost]
        public ActionResult All()
        {
            CreateUsers();

            CreateGames();

            CreateLibrary();

            CreateFriends();

            CreateNotInterestedGames();

            CreateGameRequests();

            AddConsoleToGames();

            return new EmptyResult();
        }

        private void CreateUsers()
        {
            Seed.CreateTable("Users", new dynamic[] 
            { 
                Id(),
                new { Email = "nvarchar(1000)" },
                new { Password = "nvarchar(100)" },
                new { Handle = "nvarchar(1000)" }
            }).ExecuteNonQuery();
        }

        private void CreateGames()
        {
            Seed.CreateTable("Games", new dynamic[] {
                Id(),
                new { Name = "nvarchar(1000)" }
            }).ExecuteNonQuery();
        }

        private void CreateLibrary()
        {
            Seed.CreateTable("Library", new dynamic[] {
                Id(),
                UserId(),
                GameId(),
            }).ExecuteNonQuery();
        }

        private void CreateFriends()
        {
            Seed.CreateTable("FriendAssociations", new dynamic[] {
                Id(),
                UserId(),
                new { IsFollowing = "int", ForeignKey = "Users(Id)" }
            }).ExecuteNonQuery();
        }

        private void CreateNotInterestedGames()
        {
            Seed.CreateTable("NotInterestedGames", new dynamic[] { 
                Id(),
                UserId(),
                GameId(),
            }).ExecuteNonQuery();
        }

        private void CreateGameRequests()
        {
            Seed.CreateTable("GameRequests", new dynamic[] {
                Id(),
                UserId(),
                GameId(),
                new { FromUserId = "int", ForeignKey = "Users(Id)" },
            }).ExecuteNonQuery();
        }

        private void AddConsoleToGames()
        {
            Seed.AddColumns("Games", new dynamic[] 
            { 
                new { Console = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        private object Id()
        {
            return new { Id = "int", Identity = true, PrimaryKey = true };
        }

        private object UserId()
        {
            return new { UserId = "int", ForeignKey = "Users(Id)" };
        }

        private object GameId()
        {
            return new { GameId = "int", ForeignKey = "Games(Id)" };
        }

        [HttpPost]
        public ActionResult PurgeDb()
        {
            Seed.PurgeDb();

            return new EmptyResult();
        }

        /// <summary>
        /// Create sample entries for your database in this method.
        /// </summary>
        [HttpPost]
        public ActionResult SampleEntries()
        {
            var amir = SeedUser("amirrajan");

            var james = SeedUser("jamesmarsh");

            var greg = SeedUser("gregkniffin");

            SeedGames(james,
                "Battalion Wars 2|WII",
                "Big Brain Academy: Wii Degree|WII",
                "Boom Blox|WII",
                "Cranium Kabookii|WII",
                "Donkey Kong Barrel Blast|WII",
                "Fishing Master|WII",
                "Ghost Squad|WII",
                "Legend of Zelda: Twilight Princess, The|WII",
                "Lego Star Wars: The Complete Saga|WII",
                "Mario & Sonic at the Olympic Games|WII",
                "Mario Kart Wii|WII",
                "Mario Strikers Charged|WII",
                "Mario Super Sluggers|WII",
                "Metroid Prime 3: Corruption|WII",
                "No More Heroes|WII",
                "Paper Mario|WII",
                "Rayman Raving Rabbids|WII",
                "Rayman Raving Rabbids 2|WII",
                "Rayman Raving Rabbids: TV Party|WII",
                "Super Mario Galaxy|WII",
                "Super Mario World|WII",
                "Super Smash Bros. Brawl|WII",
                "Wii Fit|WII",
                "Wii Sports|WII",
                "Fable: The Lost Chapters",
                "Halo",
                "Halo 2",
                "Aegis Wing",
                "After Burner Climax",
                "Alien Hominid HD",
                "Aqua",
                "Armored Core 4",
                "Assassin's Creed II",
                "Batman: Arkham Asylum",
                "Battlefield: Bad Company 2",
                "Bayonetta",
                "Beatles: Rock Band, The",
                "BioShock",
                "BioShock 2",
                "Blitz: The League II",
                "Blue Dragon",
                "Bomberman Live",
                "Bust-A-Move Live!",
                "Call of Duty 4: Modern Warfare",
                "Call of Duty: Modern Warfare 2",
                "Castle Crashers",
                "Castlevania: Harmony of Despair",
                "Catan",
                "Comic Jumper: The Adventures of Captain Smiley",
                "Crackdown",
                "Crackdown 2",
                "Crystal Defenders",
                "Culdcept SAGA",
                "Dance Central",
                "Darwinia+",
                "Dead or Alive 4",
                "Death By Cube",
                "Defense Grid: The Awakening",
                "Devil May Cry 4",
                "Dragon Ball Z: Burst Limit",
                "Dynasty Warriors 6 Empires",
                "Dynasty Warriors: Gundam",
                "Dynasty Warriors: Gundam 2",
                "Fallout 3",
                "Final Fantasy XIII",
                "Gears of War",
                "Gears of War 2",
                "Halo 3",
                "Halo 3: ODST",
                "Halo Reach",
                "Hexic HD",
                "Ikaruga",
                "Invincible Tiger: The Legend of Han Tao",
                "Iron Man 2",
                "Kinect Adventures!",
                "Kingdom Under Fire: Circle of Doom",
                "Kung Fu Panda",
                "Lara Croft and the Guardian of Light",
                "Last Remnant, The",
                "Left 4 Dead",
                "Lego Batman",
                "Lego Indiana Jones: The Original Adventures",
                "Lips",
                "Lost Odyssey",
                "Lost Planet 2",
                "Magic: The Gathering - Duels of the Planeswalkers",
                "Marvel vs. Capcom 2",
                "Marvel: Ultimate Alliance",
                "Marvel: Ultimate Alliance 2",
                "Mass Effect",
                "Mass Effect 2",
                "Monday Night Combat",
                "Monopoly",
                "Naruto: Rise of a Ninja",
                "NBA 2K7",
                "NHL 2K8",
                "Ninja Gaiden II",
                "Operation Darkness",
                "Panzer General: Allied Assault",
                "Peggle Deluxe",
                "Pinball FX 2",
                "Plants vs. Zombies",
                "Poker Smash",
                "Prince of Persia: The Forgotten Sands",
                "Project Sylpheed",
                "Prototype",
                "Puzzle Quest 2",
                "Puzzle Quest: Challenge of the Warlords",
                "Puzzle Quest: Challenge of the Warlords - Revenge of the Plague Lord",
                "Puzzle Quest: Galactrix",
                "Record of Agarest War",
                "Resonance of Fate",
                "RISK: Factions",
                "Rock Band",
                "Rock Band 2",
                "Rock Band 3",
                "Rockstar Games presents Table Tennis",
                "R-Type Dimensions",
                "Rumble Roses XX",
                "Sacred 2: Fallen Angel",
                "Scott Pilgrim vs. the World",
                "Scrap Metal",
                "Sega Superstars Tennis",
                "Shadow Complex",
                "Sid Meier's Civilization Revolution",
                "Singularity",
                "Soul Calibur IV",
                "Spectral Force 3",
                "Spiderman: Shattered Dimensions",
                "Spider-Man: Web of Shadows",
                "Splosion Man",
                "Star Ocean: The Last Hope",
                "Star Trek: Legacy",
                "Stranglehold",
                "Street Fighter IV",
                "Super Meat Boy",
                "Super Puzzle Fighter II Turbo HD Remix",
                "Super Street Fighter IV",
                "Teenage Mutant Ninja Turtles: Turtles in Time Re-Shelled",
                "Tekken 6",
                "Tom Clancy's HAWX",
                "Too Human",
                "Toy Soldiers",
                "Transformers: Revenge of the Fallen",
                "Transformers: The Game",
                "Transformers: War for Cybertron",
                "Trials HD",
                "Triggerheart Exelica",
                "Trivial Pursuit",
                "Vandal Hearts: Flames of Judgment",
                "Viking: Battle for Asgard",
                "Virtua Tennis 3",
                "X-Men Origins: Wolverine",
                "Your Shape: Fitness Evolved"
            );

            SeedGames(amir, 
                "Final Fantasy X|PS2",
                "Gran Turismo 3 A-spec|PS2",
                "Grand Theft Auto: Vice City|PS2",
                "Kingdom Hearts|PS2",
                "Kingdom Hearts II|PS2",
                "Metal Gear Solid 2: Sons of Liberty|PS2",
                "Metal Gear Solid 3: Snake Eater|PS2",
                "Okami|PS2",
                "We Love Katamari|PS2",
                "Xenosaga Episode I: Der Wille zur Machz|PS2",
                "Madden NFL 09|WII",
                "Metroid Prime 3: Corruption|WII",
                "Monster Hunter Tri|WII",
                "No More Heroes|WII",
                "Super Mario Galaxy II|WII",
                "Super Smash Bros. Brawl|WII",
                "Trauma Center: New Blood|WII",
                "Wii Sports|WII",
                "Wii Sports Resort|WII",
                "Braid",
                "Forza Motorsport II",
                "Gears of War",
                "Halo 3",
                "Ikaruga",
                "Left 4 Dead",
                "LIMBO",
                "NBA 2K10",
                "NBA 2K9",
                "Ninja Gaiden II",
                "Pac-Man Championship Edition",
                "Peggle Deluxe",
                "Peggle Night",
                "Prototype",
                "Resident Evil 5",
                "Street Fighter II",
                "Trials HD");

            SeedGames(greg,
                "Assassin's Creed",
                "Battlefield: Bad Company 2",
                "Beatles: Rock Band, The",
                "Call of Duty: Modern Warfare 2",
                "Crackdown",
                "Dance Central",
                "Dragon Age: Origins",
                "Fable II",
                "Fallout 3",
                "Grand Theft Auto IV",
                "Guitar Hero III: Legends of Rock",
                "Halo 3",
                "Homefront",
                "Kinect Adventures!",
                "L.A. Noire",
                "Marvel: Ultimate Alliance",
                "Mass Effect",
                "Mirror's Edge",
                "Need for Speed: Hot Pursuit",
                "Red Dead Redemption",
                "Rock Band",
                "Rock Band 2",
                "Sid Meier's Civilization Revolution",
                "Star Wars: The Force Unleashed",
                "Star Wars: The Force Unleashed II",
                "Superman Returns",
                "Toy Soldiers",
                "Trials HD",
                "Your Shape: Fitness Evolved"
            );

            return new EmptyResult();
        }

        private void SeedGame(string name, object userId)
        {
            Games games = new Games();

            var console = "XBOX360";

            var tokens = name.Split(new string[] { "|" }, StringSplitOptions.None);

            if (tokens.Length == 2)
            {
                name = tokens.First();
                console = tokens.Last();
            }

            var game = games.SingleWhere("Name = @0", name);

            object gameId = null;

            if (game == null) gameId = new { name, console }.InsertInto("Games");

            else gameId = game.Id;

            new { userId, gameId }.InsertInto("Library");
        }

        private void SeedGames(object userId, params string[] games)
        {
            games.ForEach(g => SeedGame(g, userId));
        }

        private object SeedUser(string handle)
        {
            return new { Email = handle + "@example.com", Password = "password", Handle = "@" + handle }.InsertInto("Users");
        }

        //here is a sample of how to create a table
        private void CreateSampleTable()
        {
            Seed.CreateTable("SampleTable", new dynamic[] 
            { 
                new { Id = "uniqueidentifier", PrimaryKey = true },
                new { Foo = "nvarchar(max)", Default = "Hello" },
                new { Bar = "int", Nullable = false }
            }).ExecuteNonQuery();
        }

        //here is another sample of how to create a table
        private void CreateAnotherSampleTable()
        {
            Seed.CreateTable("AnotherSampleTable", new dynamic[] 
            { 
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Foo = "nvarchar(max)", Default = "Hello", Nullable = false },
            }).ExecuteNonQuery();
        }

        //here is a sample of how to alter a table
        private void AlterSampleTable()
        {
            Seed.AddColumns("SampleTable", new dynamic[] 
            {
                new { AnotherColumn = "bigint" },
                new { YetAnotherColumn = "nvarchar(max)" }
            }).ExecuteNonQuery();
        }

        //different ad hoc queries
        private void AdHocChange()
        {
            //hey look, you can just do an ad hoc read
            var reader = "select * from SampleTable".ExecuteReader();
            while(reader.Read())
            {
                //do stuff here
            }

            //hey look, I can do a ad hoc scalar
            var name = "select top 1 name from sysobjects".ExecuteScalar() as string;

            //hey look, I can do an ad hoc non query
            "drop table SampleTable".ExecuteNonQuery();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = Content(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }
    }
}
