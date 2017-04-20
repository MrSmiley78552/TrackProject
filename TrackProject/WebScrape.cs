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
        List<string> pdfFilePathList = new List<string>();

        private string[] websites = {"http://demonstf.webs.com/results"};

        public WebScrape()
        {
            string[] websites = getAllWebsites();
            //get files of all the websites
            int i = 1;
            foreach(var website in websites)
            {
                //gets the website and stores it in a .txt file
                string url = website;
                string fileName = "website" + i + ".txt";
                string txtFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\HS_WebSites\" + fileName;
                WebClient myWC = new WebClient();
                myWC.DownloadFile(url, txtFilePath);
                myWC.Dispose();
                i++;

                //searches through the .txt file for .pdf documents
                getPDFsFromWebSite(txtFilePath);
            }
            //get pdf
            int counter = pdfFilePathList.Count();

        }

        private void getPDFsFromWebSite(string txtFilePath)
        {
            int i = 1;
            string[] lines = System.IO.File.ReadAllLines(txtFilePath);
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                if (lines[lineNumber].Contains(".pdf"))
                {
                    string pdfFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\PDF_Results\pdfTestFile" + i + ".txt";
                    pdfFilePathList.Add(pdfFilePath);
                    i++;
                }
            }
        }
        private string[] getAllWebsites()
        {
            return websites;
        }

        private static void addWebsite(string newWebsite)
        {

        }
        
        private void removeWebsite(string removableWebsite)
        {

        }

    }
}
