using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using System.Net.Mail;

namespace BorrowedGames.Models
{
    public class Email : Gemini
    {
        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        void Send()
        {
            var client = new SmtpClient();

            client.SendAsync("notifications@borrowedgames.com", To, Subject, Body, null);
        }
    }
}