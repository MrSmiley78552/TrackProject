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
        private static string[] urls = { "http://demonstf.webs.com/results" };

        public WebScrape()
        {
            List<string> allPDF_Urls = new List<string>();
            string[] urls = getAllWebsites();
            //get files of all the websites
            int i = 1;
            foreach(var url in urls)
            {
                //gets the website and stores it in a .txt file
                string fileName = "website" + i + ".txt";
                string txtFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\HS_WebSites\" + fileName;
                WebClient myWC = new WebClient();
                myWC.DownloadFile(url, txtFilePath);
                myWC.Dispose();
                i++;

                //searches through the .txt file for .pdf documents and puts them all into the list
                allPDF_Urls.AddRange(getPDFsFromWebSite(txtFilePath));
            }
        }

        //does what it says
        private List<string> getPDFsFromWebSite(string txtFilePath)
        {
            //now we have a list of lines that contain ".pdf"
            string[] pdfLineList = getLinesFromTxtContainingPDF(txtFilePath);

            List<string> pdfUrls = new List<string>();
            //cycles through each line from the website.txt file to pick out all the pdf links.
            foreach(var line in pdfLineList)
            {
                pdfUrls.AddRange(getPDF_Urls(line));
            }
            return pdfUrls;
        }

        //given a line with an instance/s of ".pdf" returns the url to access those pdf files.
        private List<string> getPDF_Urls(string line)
        {
            List<string> pdfUrls = new List<string>();
            while(line.IndexOf("http") != -1)
            {
                int indexOfHTTP = line.IndexOf("http");
                line = line.Substring(indexOfHTTP);
                indexOfHTTP = line.IndexOf("http");
                int indexOfQUOTES = line.IndexOf("\"");
                pdfUrls.Add(stripOfAMP(line.Substring(indexOfHTTP, indexOfQUOTES - indexOfHTTP)));
                line = line.Substring(indexOfQUOTES);
            }
            return pdfUrls;
        }

        private string stripOfAMP(string pdfUrl)
        {
            return pdfUrl.Replace("amp;", "");
        }
        //searches through the txtFile for any lines containing ".pdf" then returns a List of those lines.
        private string[] getLinesFromTxtContainingPDF(string txtFilePath)
        {
            List<string> pdfLineList = new List<string>();
            string[] lines = System.IO.File.ReadAllLines(txtFilePath);
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                if (lines[lineNumber].Contains(".pdf"))
                {
                    pdfLineList.Add(lines[lineNumber]);
                }
            }
            return pdfLineList.ToArray();
        }
        private static string[] getAllWebsites()
        {
            return urls;
        }

        private static void addWebsite(string newWebsite)
        {

        }
        
        private static void removeWebsite(string removableWebsite)
        {

        }

    }
}
