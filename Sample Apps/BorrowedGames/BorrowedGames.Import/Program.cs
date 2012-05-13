using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BorrowedGames.Models;
using System.Net;
using HtmlAgilityPack;

namespace BorrowedGames.Import
{
    class Program
    {
        static Games games;

        static void Main(string[] args)
        {
            games = new Games();
            var urls = new List<string>();
            urls.Add("http://www.gamefaqs.com/XBOX360/list-a");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-b");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-c");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-d");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-e");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-f");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-g");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-h");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-i");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-j");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-k");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-l");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-m");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-n");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-o");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-p");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-q");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-r");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-s");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-t");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-u");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-v");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-w");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-x");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-y");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-z");
            urls.Add("http://www.gamefaqs.com/XBOX360/list-0");

            foreach (string url in urls)
            {
                try
                {
                    ProcessGameLetter(url, 0);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }

                Console.WriteLine("Done: " + url.Replace("http://www.gamefaqs.com", ""));
            }

            Console.WriteLine("Done.");
            Console.Read();
        }

        private static void ProcessGameLetter(string url, int page)
        {
            var console = "XBOX360";

            var webClient = new WebClient();
            var response = webClient.DownloadData(url + "?page=" + page.ToString());
            var utf8Encoding = new UTF8Encoding();
            var html = utf8Encoding.GetString(response);
            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            var gameNames = doc.DocumentNode.SelectNodes("//div[@class='body']/table/tr/td");

            if (gameNames != null)
            {
                foreach (var item in gameNames)
                {
                    var gameName = item.FirstChild.InnerHtml;

                    gameName = System.Web.HttpUtility.HtmlDecode(gameName);

                    gameName = gameName.Trim();

                    var exclude = new string[] { "---", "MyG", "Q&A", "Pics", "Vids", "Board", "FAQs", "Codes", "Reviews" };

                    if (exclude.Contains(gameName)) continue;

                    if (gameName.Contains("Ass"))
                    {
                        Console.WriteLine(gameName);
                    }

                    if (games.All(where: "Name = @0", args: new object[] { gameName }).Count() == 0) games.Insert(new { Name = gameName, console });
                }

                ProcessGameLetter(url, ++page);
            }
        }
    }
}
