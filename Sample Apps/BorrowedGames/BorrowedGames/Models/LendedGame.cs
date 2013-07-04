using System;

namespace BorrowedGames.Models
{
    public class LendedGame : Rental
    {
        public LendedGame(dynamic game)
            : base(game as object)
        { }

        public int DaysOut
        {
            get { return Days(DateTime.Today - DateGiven); }
        }

        public int DaysLeft
        {
            get { return Days(_.ReturnDate - DateTime.Today); }
        } 

        public int Days(dynamic timeSpan)
        {
            return Convert.ToInt32(timeSpan.TotalDays);
        }

        public DateTime DateGiven
        {
            get { return _.ReturnDate.AddMonths(-1); }
        }
    }
}
