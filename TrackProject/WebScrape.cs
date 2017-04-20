using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TrackProject
{
    class WebScrape
    {
        public WebScrape()
        {
            string remoteUrl = "https://sites.google.com/a/fargoschools.org/girlstrack/meet-results";
            string fileName = "fargoDavies.txt";

            WebClient myWebClient = new WebClient();
            string myStringWebResource = fileName;
            myWebClient.DownloadFile(remoteUrl, fileName);
            Console.WriteLine("Success");

            string[] lines = System.IO.File.ReadAllLines(fileName);
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                if (lines[lineNumber].Contains("https:") && lines[lineNumber].Contains(".pdf"))
                    Console.WriteLine("here");
            }
        }
        

    }
}
