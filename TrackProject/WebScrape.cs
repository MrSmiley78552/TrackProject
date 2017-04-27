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

        public object DaimtoGoogleDriveHelper { get; private set; }

        public WebScrape()
        {
            List<string> allPDF_Urls = new List<string>();
            allPDF_Urls = getAllWebsites(allPDF_Urls);

            //------------------------------------------------------------------
            //string[] scopes = new string[] { DriveService.Scope.Drive };
            // Full access 
            //var keyFilePath = @"C:\Users\Mitchell\Desktop\TrackProject\file.p12"; // Downloaded from https://console.developers.google.com 
            //var serviceAccountEmail = "xx@developer.gserviceaccount.com"; // found https://console.developers.google.com 
            //loading the Key file 
            //var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
            //var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail) { Scopes = scopes }.FromCertificate(certificate));

            //var service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "Drive API Sample", });

            //var services = new DriveService(new BaseClientService.Initializer()
            //{
            //    ApiKey = "[API key]", // from https://console.developers.google.com (Public API access) 
            //    ApplicationName = "Drive API Sample",
            //});

            //FilesResource.ListRequest request = service.Files.List();
            //FileList files = request.Execute();


            //var stream = services.HttpClient.GetStreamAsync("https://docs.google.com/viewer?a=v&pid=sites&srcid=ZmFyZ29zY2hvb2xzLm9yZ3xnaXJsc3RyYWNrfGd4OjNjN2NkYmM3MDc2MDIwMDM");
            //var result = stream.Result;
            //using (var fileStream = System.IO.File.Create(@"C:\Users\Mitchell\Desktop\TrackProject\Results\" + "test.pdf"))
            //{
            //    result.CopyTo(fileStream);
            //}
            //------------------------------------------------------------------
            //puts all the pdf's into the Results folder
            int i = 1;
            foreach(var pdfUrl in allPDF_Urls)
            {
                WebClient myWC = new WebClient();
                myWC.DownloadFile(pdfUrl, @"C:\Users\Mitchell\Desktop\TrackProject\Results\" + "testResults" + i + ".pdf");
                myWC.Dispose();
                i++;
            }
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

        private static void addWebsite(string newWebsite)
        {

        }
        
        private static void removeWebsite(string removableWebsite)
        {

        }

    }
}
