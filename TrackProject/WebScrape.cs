using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System;
using Google.Apis.Drive.v2;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Drive.v2.Data;
using System.Collections.Generic;

namespace TrackProject
{
    class WebScrape
    {
        private static string[] urls = { "https://sites.google.com/a/fargoschools.org/girlstrack/meet-results",
        };


        public WebScrape()
        {
            List<string> allPDF_Urls = new List<string>();
            allPDF_Urls = getAllWebsites(allPDF_Urls);
            string[] fargoDaviesPDF_Urls = getPDF_UrlsFromFargoDavies("https://sites.google.com/a/fargoschools.org/girlstrack/meet-results");
            //not working
            //string[] test = getPDF_UrlsFromLegacy("http://bpssabers.wixsite.com/lh-track-and-field/blank");
            string[] bismarckHighPDF_Urls = getPDF_UrlsFromBismarckHigh("http://demonstf.webs.com/results");
            string[] combined = fargoDaviesPDF_Urls.Concat(bismarckHighPDF_Urls).ToArray();
            //puts all the pdf's into the Results folder
            int i = 1;
            WebClient myWC = new WebClient();
            foreach (var pdfUrl in combined)
            {
                myWC.DownloadFile(pdfUrl, @"C:\Users\Mitchell\Desktop\TrackProject\Results\" + "testResults" + i + ".pdf");
                i++;
            }
            myWC.Dispose();
        }

        //does what it says
        private List<string> getPDFsFromWebSiteTXT(string txtFilePath)
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

        private List<string> getAllWebsites(List<string> allPDF_Urls)
        {
            int i = 1;
            foreach (var url in urls)
            {
                //gets the website and stores it in a .txt file
                string fileName = "website" + i + ".txt";
                string txtFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\HS_WebSites\" + fileName;
                WebClient myWC = new WebClient();
                myWC.DownloadFile(url, txtFilePath);
                myWC.Dispose();
                i++;

                //searches through the .txt file for .pdf documents and puts them all into the list
                allPDF_Urls.AddRange(getPDFsFromWebSiteTXT(txtFilePath));
            }

            return allPDF_Urls;
        }

        private string[] getPDF_UrlsFromBismarckHigh(string bismarckHighPageURL)
        {
            string fileName = "website_BismarckHigh.txt";
            string txtFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\HS_WebSites\" + fileName;
            WebClient myWC = new WebClient();
            myWC.DownloadFile(bismarckHighPageURL, txtFilePath);
            myWC.Dispose();
            string[] possiblePDF_Links = getLinesFromTxtContainingPDF(txtFilePath);
            List<string> pdfs = new List<string>();
            foreach (var possiblePDF in possiblePDF_Links)
            {
                pdfs.AddRange(getPDF_Urls(possiblePDF));
            }
            return pdfs.ToArray();
        }

        //not working
        private string[] getPDF_UrlsFromLegacy(string legacyPageURL)
        {
            string fileName = "website_Legacy.txt";
            string txtFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\HS_WebSites\" + fileName;
            WebClient myWC = new WebClient();
            string text = myWC.DownloadString(legacyPageURL);
            myWC.Dispose();

            string[] possiblePDF_Links = getLinesFromTxtContainingPDF(txtFilePath);
            List<string> pdfs = new List<string>();
            foreach(var possiblePDF in possiblePDF_Links)
            {
                pdfs.Concat(getPDF_Urls(possiblePDF));
            }
            return pdfs.ToArray();
        }

        //WORKING
        private string[] getPDF_UrlsFromFargoDavies(string fargoDaviesPageURL)
        {
            string fileName = "website_FargoDavies.txt";
            string txtFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\HS_WebSites\" + fileName;
            WebClient myWC = new WebClient();
            myWC.DownloadFile(fargoDaviesPageURL, txtFilePath);
            myWC.Dispose();

            //gets rid of the view links and leaves us with the part of the line containing the download link.
            string[] possiblePDF_Links = getLinesFromTxtContainingPDF(txtFilePath);
            for(int i = 0; i < possiblePDF_Links.Length; i++)
            {
                if(possiblePDF_Links[i].Contains("View</a>"))
                    possiblePDF_Links[i] = possiblePDF_Links[i].Substring(possiblePDF_Links[i].IndexOf("View</a>"));
            }

            List<string> pdfUrls = new List<string>();
            for(int i = 0; i < possiblePDF_Links.Length; i++)
            {
                string line = possiblePDF_Links[i];
                if(line.IndexOf("/a/fargoschools.org/") != -1)
                {
                    int indexOfStart = line.IndexOf("/a/fargoschools.org/");
                    line = line.Substring(indexOfStart);
                    indexOfStart = line.IndexOf("/a/fargoschools.org/");
                    int indexOfQUOTES = line.IndexOf("\"");
                    line = stripOfAMP(line.Substring(indexOfStart, indexOfQUOTES - indexOfStart));
                    pdfUrls.Add("https://sites.google.com" + line);
                }
            }

            return pdfUrls.ToArray();
        }
        private static void addWebsite(string newWebsite)
        {

        }
        
        private static void removeWebsite(string removableWebsite)
        {

        }

    }
}
