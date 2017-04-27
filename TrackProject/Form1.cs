using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace TrackProject
{
    public partial class Form1 : Form
    {
        SqlCommand cmd;
        SqlConnection con;
        SqlDataAdapter da;

        public Form1()
        {
            InitializeComponent();
        }

        string trackOrFieldEvent = "";
        string fName, lName;
        int year;
        int place = -1;
        string[] individualRunningEvents = {    "60 Meter Hurdles", "60 Meter Dash", "1600 Meter Run", "400 Meter Dash",
                                                "800 Meter Run", "200 Meter Dash", "3200 Meter Run" ,"100 Meter Hurdles",
                                                "110 Meter Hurdles", "300 Meter Hurdles", "100 Meter Dash"};
        string[] relayRunningEvents = { "4x800 Meter Relay", "4x100 Meter Relay", "4x200 Meter Relay", "4x400 Meter Relay" };
        string[] fieldEvents = {"High Jump", "Pole Vault", "Long Jump", "Triple Jump", "Shot Put", "Discus", "Javelin" };
        string[] teamEvents = { "Team Rankings" };
        string dateOfMeet = "";
        string meetName = "";
        string time, distance;
        int schoolNameLengthFlag = 0;
        string[] possibleColumnKeyWords = { "Name", "Yr", "School", "Seed", "Finals", "Team", "Relay", "Points", "H#", "Prelims" };
        string[] currentColumnKeyWords;
        int booleanIfFinals = -1; //0 means false---1 means true
        int numberOfPages;
        string fileForTemporaryPDFs = @"C:\Users\Mitchell\Desktop\TrackProject\tempFileForMultiColumnPDFs.txt";
        Boolean areThereColumns = false;
        string[] memberSchools;
        static string schoolName;
        int relayErrorCount = 0;
        int startOfSchoolPosition;


        private void button1_Click(object sender, EventArgs e)
        {
            //WebScrape myWS = new WebScrape();
            string[] allPDFsInResults = Directory.GetFiles(@"C:\Users\Mitchell\Desktop\TrackProject\Results\");
            //PdfReader[] pdfArray = new PdfReader[8];
            List<PdfReader> pdfList = new List<PdfReader>();
            foreach(var pdfFile in allPDFsInResults)
            {
                PdfReader temp = new PdfReader(@"" + pdfFile);
                pdfList.Add(temp);
            }
            PdfReader[] pdfArray = pdfList.ToArray();

            foreach (var reader in pdfArray)
            {
                //sets up the connection to the database
                con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True");
                con.Open();

                numberOfPages = reader.NumberOfPages;
                string text = PdfTextExtractor.GetTextFromPage(reader, 1, new LocationTextExtractionStrategy());
                string[] linesOnPage;
                string line;
                //splits the page by each new line
                linesOnPage = text.Split('\n');

                dateOfMeet = "";
                //cycles through each line in the page
                for (int lineNumber = 0; lineNumber < 5; lineNumber++)
                {
                    //returns a single line of text
                    line = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(linesOnPage[lineNumber]));

                    autoSetDateOfMeet(lineNumber, line, linesOnPage);
                    autoSetNameOfMeet(lineNumber, line);
                }

                //checks if the pdf contains 2 columns
                if (autoCheckForTwoColumns(reader) == true)
                {
                    //the document has 2 columns

                    //clear the temp file
                    File.WriteAllText(fileForTemporaryPDFs, String.Empty);

                    //orgnaizes the two column pdf into a single column text document
                    organizeTwoColumnsIntoOneColumn(reader);


                    //data is stored in a text file now, so figure out how to use the methods I currently have to parse through that information__________________________________________________________
                    string[] lines = System.IO.File.ReadAllLines(fileForTemporaryPDFs);
                    for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                    {
                        autoSetColumnCount(lineNumber, lines[lineNumber], lines);


                        //sets  trackOrFieldEvent  to the type in the line
                        autoSetEventType(lines[lineNumber]);

                        //will skip the team rankings
                        if (!trackOrFieldEvent.Equals("Team Rankings"))
                        {
                            if (!trackOrFieldEvent.Equals(""))
                                determineIfRelayOrSingleEvent(lines[lineNumber], lineNumber, lines);
                        }
                    }
                }
                else
                {
                    //the document has 1 column

                    //handles all operations on the single column pdf
                    autoHandleSingleColumnPDF(reader);
                }
                con.Close();
            }
        }

        private void autoHandleSingleColumnPDF(PdfReader reader)
        {
            string[] pageLines;
            string line;

            //cycles through each page of the pdf
            for (int page = 1; page <= numberOfPages; page++)
            {
                string text = PdfTextExtractor.GetTextFromPage(reader, page, new LocationTextExtractionStrategy());

                //splits the page by each new line
                pageLines = text.Split('\n');

                //cycles through each line in the page
                for (int lineNumber = 0, len = pageLines.Length; lineNumber < len; lineNumber++)
                {
                    //returns a single line of text
                    line = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(pageLines[lineNumber]));

                    //autoSetDateOfMeet(lineNumber, line, pageLines);
                    //autoSetNameOfMeet(lineNumber, line);

                    //sets  trackOrFieldEvent  to the type in the line
                    if (!trackOrFieldEvent.Equals(autoSetEventType(line)))
                    {
                        currentColumnKeyWords = new string[0];
                    }
                    trackOrFieldEvent = autoSetEventType(line);
                    autoSetColumnCount(lineNumber, line, pageLines);

                    //will skip the team rankings
                    if (!trackOrFieldEvent.Equals("Team Rankings"))
                    {
                        if (!trackOrFieldEvent.Equals(""))
                            determineIfRelayOrSingleEvent(line, lineNumber, pageLines);
                    }
                }
            }
        }

        //creates a rectangle in the center of the page, which is checked to see if it contains text in it
        //if there is text in the rectangle, then the document is all a single column
        //if no text in the rectangle, then the document has two columns
        private Boolean autoCheckForTwoColumns(PdfReader reader)
        {
            var centerRect = new iTextSharp.text.Rectangle(296, 0, 301, 600);
            var renderFilter = new RenderFilter[1];
            renderFilter[0] = new RegionTextRenderFilter(centerRect);
            var textExtractionStrategy = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), renderFilter);
            var centerColumn = PdfTextExtractor.GetTextFromPage(reader, 1, textExtractionStrategy);
            
            if (centerColumn.Equals(""))
            {
                return true;
            }
            else
                return false;
        }
        private void organizeTwoColumnsIntoOneColumn(PdfReader reader)
        {
            StreamWriter fileWriter = new StreamWriter(fileForTemporaryPDFs);

            for (int currentPage = 1; currentPage <= numberOfPages; currentPage++)
            {
                string[] leftColumn = getLeftColumn(reader, currentPage);
                foreach (string row in leftColumn)
                {
                    fileWriter.WriteLine(row);
                }
                string[] rightColumn = getRightColumn(reader, currentPage);
                foreach (string row in rightColumn)
                {
                    fileWriter.WriteLine(row);
                }
            }
            fileWriter.Close();
            areThereColumns = true;
        }
         
        private string[] getLeftColumn(PdfReader reader, int page)
        {
            var renderFilter = new RenderFilter[1];
            var textExtractionStrategy = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), renderFilter);
            var leftRect = new iTextSharp.text.Rectangle(0, 0, 296, 720);
            renderFilter[0] = new RegionTextRenderFilter(leftRect);
            var leftColumn = PdfTextExtractor.GetTextFromPage(reader, page, textExtractionStrategy);
            string[] leftColumnArray = leftColumn.Split('\n');
            return leftColumnArray;
        }
        private string[] getRightColumn(PdfReader reader, int page)
        {
            var renderFilter = new RenderFilter[1];
            var textExtractionStrategy = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), renderFilter);
            var rightRect = new iTextSharp.text.Rectangle(305, 0, 702, 720);
            renderFilter[0] = new RegionTextRenderFilter(rightRect);
            var rightColumn = PdfTextExtractor.GetTextFromPage(reader, page, textExtractionStrategy);
            string[] rightColumnArray = rightColumn.Split('\n');
            return rightColumnArray;
        }

        //checks how many columns for the event
        private void autoSetColumnCount(int j, string line, string [] words)
        {
            if (line.Equals("Finals") || line.Equals("Preliminaries"))
            {
                if (line.Equals("Finals"))
                    booleanIfFinals = 1;
                else
                    booleanIfFinals = 0;

                string columnCountLine = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(words[j - 1]));
                currentColumnKeyWords = countKeyWords(columnCountLine);
            }
        }
        private void autoSetNameOfMeet(int j, string line)
        {
            if(j == 1)
            {
                int  indexOfHyphen = line.IndexOf('-');
                if (indexOfHyphen == -1)
                    indexOfHyphen = line.Length;
                meetName = line.Substring(0, indexOfHyphen);
            }
        }

        //gets the date of the meet... first checks the title lines... if no date in the title lines
        //then checks the document header
        private void autoSetDateOfMeet(int j, string line, string [] words)
        {
            if (dateOfMeet.Equals("") && j >= 1)
            {
                Regex rgx = new Regex(@"\d{2}/\d{2}/\d{4}");
                Match mat = rgx.Match(line);
                dateOfMeet = mat.ToString();
                if (dateOfMeet.Equals(""))
                {
                    Regex rgx2 = new Regex(@"\d{1}/\d{2}/\d{4}");
                    Match mat2 = rgx2.Match(line);
                    dateOfMeet = mat2.ToString();
                }
                if (dateOfMeet.Equals(""))
                {
                    Regex rgx3 = new Regex(@"\d{1}/\d{1}/\d{4}");
                    Match mat3 = rgx3.Match(line);
                    dateOfMeet = mat3.ToString();
                }

                if (j > 3 && dateOfMeet.Equals(""))
                {
                    string headerLine = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(words[0]));

                    rgx = new Regex(@"\d{2}/\d{2}/\d{4}");
                    mat = rgx.Match(headerLine);
                    dateOfMeet = mat.ToString();
                    if (dateOfMeet.Equals(""))
                    {
                        Regex rgx2 = new Regex(@"\d{1}/\d{2}/\d{4}");
                        Match mat2 = rgx2.Match(headerLine);
                        dateOfMeet = mat2.ToString();
                    }
                    if (dateOfMeet.Equals(""))
                    {
                        Regex rgx3 = new Regex(@"\d{1}/\d{1}/\d{4}");
                        Match mat3 = rgx3.Match(headerLine);
                        dateOfMeet = mat3.ToString();
                    }
                }
            }
        }

        //checks if the event
        private void determineIfRelayOrSingleEvent(string line, int j, string[] words)
        {
            //determines what kind of check to do
            if (trackOrFieldEvent.Contains("Relay"))
            {
                if (memberOfWhichSchool(line, memberSchools, j))
                {
                    relayEvents(line, j, words);
                }
            }
            else
            {
                if(memberOfWhichSchool(line, memberSchools, j))
                {
                    singleEvents(line);
                }
            }
        }

        //checks if the line contains a member of Fargo Davies
        //if the line does, then the method returns true
        //if the line does not, then the method returns false
        private Boolean memberOfWhichSchool(string line, string [] memberSchools, int pageLineNumber)
        {
            if (pageLineNumber > 4 && currentColumnKeyWords != null && currentColumnKeyWords.Length != 0)
            {
                foreach (var school in memberSchools)
                {
                    try
                    {
                        string[] tempLine = line.Split(' ');
                        string[] tempSchool = school.Split(' ');
    
                        int counter = 0;

                        for(int i = 0; i < tempLine.Length; i++)
                        {
                            for(int j = 0; j < tempSchool.Length; j++)
                            {
                                if (tempLine[i].Equals(tempSchool[j]))
                                {
                                    counter++;
                                    if(counter == tempSchool.Length)
                                    {
                                        schoolName = school;
                                        schoolNameLengthFlag = tempSchool.Length;
                                        startOfSchoolPosition = i - schoolNameLengthFlag + 1;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            return false;
        }

        //checks if the athlete is in the database and returns their aId
        //will return  -1  if athlete is not in database
        private int checkIfAthleteInDatabase(string fName, string lName, int year, string schoolName)
        {
            SqlDataReader sqlReader;
            cmd = new SqlCommand();
            cmd.CommandText = "SELECT aId FROM Athlete WHERE fName = @fName AND lName = @lName AND year = @year AND schoolName = @schoolName";
            cmd.Parameters.AddWithValue("@fName", fName);
            cmd.Parameters.AddWithValue("@lName", lName);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@schoolName", schoolName);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;

            sqlReader = cmd.ExecuteReader();
            if (sqlReader.HasRows)
            {
                while (sqlReader.Read())
                {
                        int aIdFromDatabase = sqlReader.GetInt32(0);
                        sqlReader.Close();
                        return aIdFromDatabase;
                }
            }
            else
            {
                sqlReader.Close();
                return -1;
            }
            return -1;
        }

        private int checkIfMeetInDatabase(string meetName, string date)
        {
            SqlDataReader sqlReader;
            cmd = new SqlCommand();
            cmd.CommandText = "SELECT mId FROM Meet WHERE meetName = @meetName AND date = @date";
            cmd.Parameters.AddWithValue("@meetName", meetName);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;

            sqlReader = cmd.ExecuteReader();
            if (sqlReader.HasRows)
            {
                while (sqlReader.Read())
                {
                    int mIdFromDatabase;
                    mIdFromDatabase = sqlReader.GetInt32(0);
                    sqlReader.Close();
                    return mIdFromDatabase;
                }
            }
            else
            {
                sqlReader.Close();
                return -1;
            }
            return -1;
        }

        //gets the data from a relay for relay events
        private void relayEvents(string line, int lineNumber, string[] lines)
        {
            place = -1;
            string distance = "";
            string time = "";
            string[] currentLine = line.Split(' ');
            string nextLine = "";
            string nextLine2 = "";

            Regex rgx = new Regex(@"\d{4}");
            Match mat = rgx.Match(line);
            string possibleDateInLine = mat.ToString();
            try
            {
                //checks to make sure there isn't a year in the result
                //this helps for the state meet, where they list previous records
                if (possibleDateInLine.Equals(""))
                {
                    place = Convert.ToInt32(currentLine[0]);
                    time = getTimeOrDistance(currentLine);
                    Boolean runnersOnSecondLine = false;

                    try
                    {
                        nextLine = lines[lineNumber + 1];
                    }
                    catch(Exception d)
                    {
                        nextLine = "";
                    }
                    int startOfRunner1 = nextLine.IndexOf("1)");
                    int startOfRunner2 = nextLine.IndexOf("2)");
                    int startOfRunner3 = nextLine.IndexOf("3)");
                    int startOfRunner4 = nextLine.IndexOf("4)");

                    //if true, we know that runners 3 and 4 are on the second line
                    if (startOfRunner3 == -1 && startOfRunner4 == -1)
                    {
                        startOfRunner3 = nextLine2.IndexOf("3)");
                        startOfRunner4 = nextLine2.IndexOf("4)");
                        nextLine2 = lines[lineNumber + 2];
                        runnersOnSecondLine = true;
                    }

                    //no runners were found
                    if (startOfRunner1 == -1 && startOfRunner2 == -1 && startOfRunner3 == -1 && startOfRunner4 == -1)
                    {
                        handleRecord(time, distance, handleAthlete("", "", 0, schoolName), handleMeet(), booleanIfFinals);
                    }
                    else
                    {
                        string entireTextForRunner1 = "";
                        string entireTextForRunner2 = "";
                        string entireTextForRunner3 = "";
                        string entireTextForRunner4 = "";
                        entireTextForRunner1 = nextLine.Substring(startOfRunner1, startOfRunner2);
                        //all runners on the same line
                        if (runnersOnSecondLine == false)
                        {
                            entireTextForRunner2 = nextLine.Substring(startOfRunner2, startOfRunner3 - startOfRunner2);
                            entireTextForRunner3 = nextLine.Substring(startOfRunner3, startOfRunner4 - startOfRunner3);
                            entireTextForRunner4 = nextLine.Substring(startOfRunner4, nextLine.Length - startOfRunner4);
                        }
                        else //runners on two lines
                        {
                            entireTextForRunner2 = nextLine.Substring(startOfRunner2, nextLine.Length - startOfRunner2);
                            entireTextForRunner3 = nextLine2.Substring(startOfRunner3, startOfRunner4 - startOfRunner3);
                            entireTextForRunner4 = nextLine2.Substring(startOfRunner4, nextLine2.Length - startOfRunner4);
                        }
                        int extraPosition;
                        string[] allTextForAllRunners = { entireTextForRunner1, entireTextForRunner2, entireTextForRunner3, entireTextForRunner4 };
                        foreach (var currentRunnerText in allTextForAllRunners)
                        {
                            extraPosition = 0;
                            string[] currentRunnerTextSplit = currentRunnerText.Split(' ');
                            //check to make sure it has a name in it. Otherwise skip that runner
                            if (currentRunnerTextSplit.Length > 2)
                            {
                                //Resizes the array to get rid of the 1), 2), 3), 4)
                                if (currentRunnerTextSplit[0].Contains(')'))
                                {
                                    string[] temp = new string[currentRunnerTextSplit.Length - 1];
                                    for (int i = 0; i < currentRunnerTextSplit.Length - 1; i++)
                                    {
                                        temp[i] = currentRunnerTextSplit[i + 1];
                                    }
                                    currentRunnerTextSplit = temp;
                                    extraPosition++;
                                }
                                int spaceCounter = 0;
                                foreach(var cell in currentRunnerTextSplit)
                                {
                                    if (cell.Equals("") || cell.Equals(" "))
                                        spaceCounter++;
                                }
                                //Sets the year value and resizes the array if it contained a year
                                if (int.TryParse(currentRunnerTextSplit[currentRunnerTextSplit.Length - spaceCounter - extraPosition], out year))
                                {
                                    string[] temp = new string[currentRunnerTextSplit.Length - spaceCounter - extraPosition];
                                    for (int i = 0; i < currentRunnerTextSplit.Length - spaceCounter - extraPosition; i++)
                                    {
                                        temp[i] = currentRunnerTextSplit[i];
                                    }
                                    currentRunnerTextSplit = temp;
                                }
                                else //if no year in the array, set it to the default year value
                                {
                                    year = 0;
                                }
                                getFirstAndLastNames(currentRunnerTextSplit); //sets the fName and lName variables
                                handleRecord(time, distance, handleAthlete(fName, lName, year, schoolName), handleMeet(), booleanIfFinals);
                            }
                        }
                    }
                    //figure out how to store the captured information, so that a user can enter the athlete's names later
                    //problem originally arose from the 2015 EDC Results
                }
            }
            catch(Exception e)
            {
                handleRecord(time, distance, handleAthlete("", "", 0, schoolName), handleMeet(), booleanIfFinals);
            }
        }

        private string getTimeOrDistance(string[] line)
        {
            List<string> possibleTimes = new List<string>();
            for(int i = 1; i < line.Length; i++)
            {
                //preferably using matching regex here to make sure there are numbers before or after punctuation
                if((line[i].Contains(":") || line[i].Contains(".")))
                {
                    if(line[i].Contains("0") || line[i].Contains("1") || line[i].Contains("2") || 
                        line[i].Contains("3") || line[i].Contains("4") || line[i].Contains("5") || 
                        line[i].Contains("6") || line[i].Contains("7") || line[i].Contains("8") || line[i].Contains("9"))
                    {
                        possibleTimes.Add(line[i]);
                    }
                }
            }
            string[] possibleTimesArray = possibleTimes.ToArray();
            if (possibleTimesArray.Length > 1)
            {
                if (Array.FindIndex(currentColumnKeyWords, isSeed) < Array.FindIndex(currentColumnKeyWords, isFinals))
                {
                    return trimTimeOrDistance(possibleTimesArray[1]);
                }
                else
                    return trimTimeOrDistance(possibleTimesArray[0]);
            }
            else
                return trimTimeOrDistance(possibleTimesArray[0]);
        }

        private static bool isSeed(string s)
        {
            if (s.Equals("Seed"))
                return true;
            return false;
        }
        private static bool isFinals(string s)
        {
            if (s.Equals("Finals"))
                return true;
            return false;
        }

        private void getFirstAndLastNames(string[] allText)
        {
            lName = "";
            fName = "";
            //check for all lName+, fName+
            for(int i = 0; i < allText.Length; i++)
            {
                if(allText[i].Contains(','))
                {
                    for(int j = 0; j <= i; j++)
                    {
                        lName += allText[j] + " ";
                    }
                    for(int k = i+1; k < allText.Length; k++)
                    {
                        fName += allText[k] + " ";
                    }
                    break;
                }
            }
            if(!lName.Equals("") && !fName.Equals(""))
            {
                lName = lName.Substring(0, lName.Length - 2); //gets rid of last space and comma
                fName = fName.Substring(0, fName.Length - 1); //gets rid of extra space
            }
            else if(allText.Length == 2) //assume fName lName
            {
                fName = allText[0];
                lName = allText[1];
            }
            else if(allText.Length == 3) //assume fName fName lName
            {
                fName = allText[0] + " " + allText[1];
                lName = allText[2];
            }
            else //assume fName fName lName ... lName
            {
                fName = allText[0] + " " + allText[1];
                for (int i = 2; i < allText.Length; i++)
                {
                    lName += allText[i] + " ";
                }
                lName = lName.Substring(0, lName.Length); //gets rid of extra space
            }
        }

        private static bool isSchool(string s)
        {
            if (s.Equals(schoolName.Split(' ')[0]))
                return true;
            return false;
        }

        //gets the data from a line for single events
        private void singleEvents(string line)
        {
            distance = "";
            time = "";
            int nameLengthFlag = 0;
            int yearLengthFlag = 0;
            int seedLengthFlag = 0;
            int prelimsLengthFlag = 0;
            int finalsLengthFlag = 0;
            string[] temp = line.Split(' ');
            string[] temp2 = new string[temp.Length - 1];
            int indexOfBlank = 0;
            string alternateFName = "";
            string alternateLName = "";
            int alternateNameLengthFlag = 0;


            //--------------------------------------
            //int positionOfMemberSchool = startOfSchoolPosition;
            ////gets the place of the athlete
            //try
            //{
            //    place = Convert.ToInt32(temp[0]);
            //}
            //catch (Exception e)
            //{
            //    place = -1;
            //}
            //string[] arrayOfTimesAndDistances = new string[temp.Length - startOfSchoolPosition - schoolNameLengthFlag];
            //int counter = 0;
            //for(int i = startOfSchoolPosition + schoolNameLengthFlag; i < temp.Length; i++)
            //{
            //    arrayOfTimesAndDistances[counter++] = temp[i];
            //}
            //if(place != -1)
            //{
            //    //gets the time or distance achieved
            //    if (fieldEvents.Contains(trackOrFieldEvent))
            //        distance = getTimeOrDistance(arrayOfTimesAndDistances);
            //    else
            //        time = getTimeOrDistance(arrayOfTimesAndDistances);
            //}
            //if (time.Length >= 10)
            //{
            //    for (int j = 0; j < time.Length; j++)
            //    {
            //        if (!time.Equals('1') || !time.Equals('2') || !time.Equals('3') ||
            //            !time.Equals('4') || !time.Equals('5') || !time.Equals('6') ||
            //            !time.Equals('7') || !time.Equals('8') || !time.Equals('9') ||
            //            !time.Equals('-') || !time.Equals('.') || !time.Equals(':') || !time.Equals('0'))
            //        {
            //            time = time.Substring(j, time.Length - j);
            //        }
            //    }
            //}
            //else if(distance.Length >= 10)
            //{
            //    for(int j = 0; j < distance.Length; j++)
            //    {
            //        if (!distance.Equals('1') || !distance.Equals('2') || !distance.Equals('3') ||
            //           !distance.Equals('4') || !distance.Equals('5') || !distance.Equals('6') ||
            //           !distance.Equals('7') || !distance.Equals('8') || !distance.Equals('9') ||
            //           !distance.Equals('-') || !distance.Equals('.') || !distance.Equals(':') || !distance.Equals('0'))
            //        {
            //            distance = distance.Substring(j, distance.Length - j);
            //        }
            //    }
            //}

            ////puts together the nameArray, which is used to get the first and last names
            //string[] leadingArray = new string[startOfSchoolPosition - 1];
            //string[] nameArray;
            //for (int i = 1; i < startOfSchoolPosition; i++)
            //{
            //    leadingArray[i - 1] = temp[i];
            //}
            //try
            //{
            //    int year = Convert.ToInt32(leadingArray[leadingArray.Length - 1]);
            //    nameArray = new string[leadingArray.Length - 1];
            //    for (int i = 0; i < leadingArray.Length - 1; i++)
            //    {
            //        nameArray[i] = leadingArray[i];
            //    }
            //}
            //catch (Exception d)
            //{
            //    year = 0;
            //    nameArray = leadingArray;
            //}
            //getFirstAndLastNames(nameArray);
            //handleRecord(time, distance, handleAthlete(fName, lName, year, schoolName), handleMeet(), booleanIfFinals);
            //--------------------------------------


            for (int i = 0; i < temp.Length - 1; i++)
            {
                if (temp[i].Equals("") && i > 0)
                {
                    indexOfBlank = i;
                    break;
                }
            }
            if (indexOfBlank != 0)
            {
                for (int k = indexOfBlank; k < temp.Length - 1; k++)
                {
                    temp[k] = temp[k + 1];
                }
            }

            //sets the place the athlete got
            try
            {
                place = Convert.ToInt32(temp[0]);
            }
            catch (Exception e)
            {
                place = -1;
            }

            //checks for Name and it's length
            if (currentColumnKeyWords.Contains("Name"))
            {
                int nameFormat = -1;
                //checks for a   lName, fName   format
                if (temp[1].Contains(','))
                {
                    lName = temp[1].Substring(0, (temp[1].Length - 1));
                    fName = temp[2];
                    nameLengthFlag = 2;
                    nameFormat = 0;
                }
                //works for a   fName lName   format
                else
                {
                    fName = temp[1];
                    lName = temp[2];
                    nameLengthFlag = 2;
                    nameFormat = 1;
                }
                //this will test if the name is longer by trying to convert the next position to an int.
                //if the name is confirmed to be only 2 long, it will do nothing
                if (temp[2].Contains(','))
                {
                    lName = temp[1] + temp[2];
                    lName = lName.Substring(0, lName.Length - 1);
                    fName = temp[3];
                    nameLengthFlag = 3;
                }
                else if (temp[3].Contains(','))
                {
                    lName = temp[1] + temp[2] + temp[3];
                    lName = lName.Substring(0, lName.Length - 1);
                    fName = temp[4];
                    nameLengthFlag = 4;
                }
                else if (temp[1].Contains(','))
                {
                    alternateLName = temp[1];
                    alternateFName = temp[2] + temp[3];
                    alternateNameLengthFlag = 3;
                }
            }

            //check for Yr and its length
            if (currentColumnKeyWords.Contains("Yr"))
            {
                try
                {
                    year = Convert.ToInt32(temp[1 + nameLengthFlag]);
                }
                catch (Exception e)
                {
                    year = 0;
                }
                yearLengthFlag = 1;
            }


            //already found the length of the school name in method---memberOfFargoDavies


            if (currentColumnKeyWords.Contains("Seed"))
                seedLengthFlag = 1;

            if (currentColumnKeyWords.Contains("Prelims"))
                prelimsLengthFlag = 1;

            if (currentColumnKeyWords.Contains("Finals"))
                finalsLengthFlag = 1;

            string alternateDistance = "";
            string alternateTime = "";
            //checks if a track event or a field event and records the time or distance respectively
            if (fieldEvents.Contains(trackOrFieldEvent))
            {
                distance = temp[nameLengthFlag + yearLengthFlag + schoolNameLengthFlag + seedLengthFlag + finalsLengthFlag + prelimsLengthFlag];
                try
                {
                    alternateDistance = temp[alternateNameLengthFlag + yearLengthFlag + schoolNameLengthFlag + seedLengthFlag + finalsLengthFlag + prelimsLengthFlag];
                }
                catch (Exception e)
                { }
                if (distance.Equals("") || distance.Equals(" ") || distance.Equals("  ") || distance.Equals("10") || distance.Equals("8") || distance.Equals("6") || distance.Equals("4.5") ||
                    distance.Equals("5") || distance.Equals("4") || distance.Equals("3") || distance.Equals("2.5") || distance.Equals("2") || distance.Equals("1.5") || distance.Equals("1"))
                    distance = temp[nameLengthFlag + yearLengthFlag + schoolNameLengthFlag + seedLengthFlag + finalsLengthFlag + prelimsLengthFlag - 1];
                distance = trimTimeOrDistance(distance);
                if (!distance.Contains(".") && !distance.Contains("-"))
                {
                    distance = trimTimeOrDistance(alternateDistance);
                }
                time = "";
            }
            else
            {
                time = temp[nameLengthFlag + yearLengthFlag + schoolNameLengthFlag + seedLengthFlag + finalsLengthFlag + prelimsLengthFlag];
                try
                {
                    alternateTime = temp[alternateNameLengthFlag + yearLengthFlag + schoolNameLengthFlag + seedLengthFlag + finalsLengthFlag + prelimsLengthFlag];
                }
                catch (Exception e)
                { }
                if (time.Equals("") || time.Equals(" ") || time.Equals("  ") || time.Equals("10") || time.Equals("8") || time.Equals("6") || time.Equals("4.5") ||
                    time.Equals("5") || time.Equals("4") || time.Equals("3") || time.Equals("2.5") || time.Equals("2") || time.Equals("1.5") || time.Equals("1"))
                    time = temp[nameLengthFlag + yearLengthFlag + schoolNameLengthFlag + seedLengthFlag + finalsLengthFlag + prelimsLengthFlag - 1];
                time = trimTimeOrDistance(time);
                if (!time.Contains('.'))
                {
                    time = trimTimeOrDistance(alternateTime);
                }
                distance = "";
            }

            int aId = handleAthlete(fName, lName, year, schoolName);
            if (aId == 71098)
            {
                int t = 0;
            }
            if (place == -1)
            {
                time = "";
                distance = "";
            }
            int mId = handleMeet();
            handleRecord(time, distance, aId, mId, booleanIfFinals);
        }

        private string trimTimeOrDistance(string mark)
        {
            char[] markChars = mark.ToCharArray();
            if(!(mark.Contains('.') || mark.Contains(':') || mark.Contains('-')))
            {
                return "";
            }
            for(int i = 0; i < markChars.Length; i++)
            {
                if(!(markChars[i].Equals('0') || markChars[i].Equals('1') || markChars[i].Equals('2') || 
                    markChars[i].Equals('3') || markChars[i].Equals('4') || markChars[i].Equals('5') || 
                    markChars[i].Equals('6') || markChars[i].Equals('7') || markChars[i].Equals('8') || 
                    markChars[i].Equals('9') || markChars[i].Equals('.') || markChars[i].Equals(':') || 
                    markChars[i].Equals('-')))
                {
                    if (i < markChars.Length / 2)
                    {
                        return mark.Substring(1);
                    }
                    else
                    {
                        return mark.Substring(0, i);
                    }
                }
            }
            return mark;
            
            //char[] lettersToCheckFor = { 'Q', 'x', 'J', 'X', 'D', 'q', 'B', '!' };
            //foreach(var letter in lettersToCheckFor)
            //{
            //    if(mark.Contains(letter))
            //    {
            //        int indexOfLetter = mark.IndexOf(letter);
            //        if ((letter.Equals('Q') || letter.Equals('q') || letter.Equals('!') || letter.Equals('B')) && indexOfLetter != -1)
            //            mark = mark.Substring(0, indexOfLetter);
            //        else if (indexOfLetter != -1)
            //            mark = mark.Substring(indexOfLetter + 1, mark.Length - (indexOfLetter + 1));
            //    }
            //}
            //return mark;
        }

        private void handleRecord(string time, string distance, int aId, int mId, int booleanIfFinals)
        {
            decimal newTime = convertStringTimeToIntTime(time);
            if (checkForDuplicateRecord(time, distance, aId, mId, booleanIfFinals) == 0)
            {
                //adds a record which is connected to the Athlete
                cmd = new SqlCommand("INSERT INTO Record (time, distance, aId, mId, place, event, finals) VALUES (@time, @distance, @aId, @mId, @place, @event, @finals)", con);

                cmd.Parameters.AddWithValue("@time", newTime);
                cmd.Parameters.AddWithValue("@distance", distance);

                cmd.Parameters.AddWithValue("@aId", aId);
                cmd.Parameters.AddWithValue("@mId", mId);
                cmd.Parameters.AddWithValue("@place", place);
                cmd.Parameters.AddWithValue("@event", trackOrFieldEvent);
                cmd.Parameters.AddWithValue("@finals", booleanIfFinals);
                cmd.ExecuteNonQuery();
            }
        }

        private decimal convertStringTimeToIntTime(string time)
        {
            decimal newTime = 0;
            char[] delimiters = new char[] { ':', '.' };
            string[] parts = time.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length == 3)
            {
                newTime = Convert.ToDecimal(parts[0]) * 60 + Convert.ToDecimal(parts[1]) + (Convert.ToDecimal(parts[2]) / 100);
            }
            else if(parts.Length == 2)
            {
                newTime = Convert.ToDecimal(parts[0]) + (Convert.ToDecimal(parts[1]) / 100);
            }
            return newTime;         
        }
        private int handleMeet()
        {
            int mId = checkIfMeetInDatabase(meetName, dateOfMeet);
            if (mId == -1)
            {
                createNewMeet(meetName, dateOfMeet);
                mId = checkIfMeetInDatabase(meetName, dateOfMeet);
            }
            return mId;
        }
        private int handleAthlete(string fName, string lName, int year, string schoolName)
        {
            int aId = checkIfAthleteInDatabase(fName, lName, year, schoolName);

            if(aId == -1)
            {
                //adds the data for the athlete into the athlete database and retrieves it's aId
                createNewAthlete(fName, lName, year, schoolName);

                //sets aId to the new Athlete's id number
                aId = checkIfAthleteInDatabase(fName, lName, year, schoolName);
            }
            return aId;
        }

        //returns 1 if there are duplicates
        //returns 0 if there are no duplicates
        private int checkForDuplicateRecord(string time, string distance, int aId, int mId, int booleanIfFinals)
        {
            SqlDataReader sqlReader;
            cmd = new SqlCommand();
            cmd.CommandText = "SELECT * FROM Record WHERE time = @time AND distance = @distance AND aId = @aId AND mId = @mId";
            cmd.Parameters.AddWithValue("@time", time);
            cmd.Parameters.AddWithValue("@distance", distance);
            cmd.Parameters.AddWithValue("@aId", aId);
            cmd.Parameters.AddWithValue("@mId", mId);
            cmd.Parameters.AddWithValue("@finals", booleanIfFinals);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;

            sqlReader = cmd.ExecuteReader();
            if (sqlReader.HasRows)
            {
                sqlReader.Close();
                return 1;
            }
            sqlReader.Close();
            return 0;
        }

        //creates a new athlete in the database
        private void createNewAthlete(string fName, string lName, int year, string schoolName)
        {
            cmd = new SqlCommand("INSERT INTO Athlete (fName, lName, year, schoolName) VALUES (@fName, @lname, @year, @schoolName)", con);
            cmd.Parameters.AddWithValue("@fName", fName);
            cmd.Parameters.AddWithValue("@lName", lName);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@schoolName", schoolName);
            cmd.ExecuteNonQuery();
        }

        //creates a new meet in the database
        private void createNewMeet(string meetName, string date)
        {
            cmd = new SqlCommand("INSERT INTO Meet (meetName, date) VALUES (@meetName, @date)", con);
            cmd.Parameters.AddWithValue("@meetName", meetName);
            cmd.Parameters.AddWithValue("@date", date);         //not sure how the conversion between date to the database works
            cmd.ExecuteNonQuery();
        }

        

        //checks a line to see if it contains an event name
        //if it does, then the method returns the event type
        //if the line does not contain an event name, then the method returns null
        private string autoSetEventType(string line)
        {
            //creates an array of all the events
            string[] allEvents = individualRunningEvents.Concat(relayRunningEvents.Concat(fieldEvents).Concat(teamEvents)).ToArray();
            foreach (string eventName in allEvents)
            {
                if (line.Contains(eventName))
                {
                    trackOrFieldEvent = eventName;
                    return eventName;
                }
            }
            return trackOrFieldEvent;
        }

        //keeps track of the number of keywords in the line
        //these keywords represent the number of columns for the records in the document
        private string[] countKeyWords(string columnCountline)
        {
            List<string> keywordsList = new List<string>();
            string[] temp = columnCountline.Split(' ');
            foreach(string keyword in temp)
            {
                if (possibleColumnKeyWords.Contains(keyword))
                {
                    keywordsList.Add(keyword);
                }
            }
            return keywordsList.ToArray();
        }

        private string[] getMemberSchools()
        {
            string[] fileLines;
            string[] newfileLines;
            int lineNumber = 0;

            fileLines = File.ReadAllLines(@"C:\Users\Mitchell\Desktop\TrackProject\memberSchools.txt");
            newfileLines = new string[fileLines.Length];

            foreach (var line in fileLines)
            {
                newfileLines[lineNumber] = line;
                lineNumber++;
            }
            return newfileLines;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            Home homeForm = new Home();
            homeForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //InitializeComponent();
            //string filePath = @"C:\Users\Mitchell\Desktop\TrackProject\testPerlFile.pl";
            //ProcessStartInfo ps = new ProcessStartInfo(@"C:\Users\Mitchell\Desktop\TrackProject\testPerlFile.pl", "program.pl arg + s");
            //ps.UseShellExecute = false;
            //ps.RedirectStandardOutput = true;
            //Process p = new Process();
            //p.Start(Form1.button3_Click.filePath);
            //string output = p.StandardOutput.ReadToEnd();
            //p.WaitForExit();

            //----------------------------works the best so far--------------------------------
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = @"C:\\Users\\Mitchell\\Desktop\\TrackProject\\testPerlFile.pl";
            myProcess.StartInfo.CreateNoWindow = false;
            myProcess.Start();
            string output = myProcess.StandardOutput.ReadToEnd();
            myProcess.WaitForExit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //WebScrape testScraper = new WebScrape();
            HomePage homePageForm = new HomePage();
            homePageForm.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            memberSchools = getMemberSchools();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SqlConnection con2 = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True");

            using (SqlCommand cmd = new SqlCommand("DELETE FROM Record", con2))
            {
                con2.Open();
                cmd.ExecuteNonQuery();
                con2.Close();
            }
            using (SqlCommand cmd = new SqlCommand("DELETE FROM Athlete", con2))
            {
                con2.Open();
                cmd.ExecuteNonQuery();
                con2.Close();
            }
            using (SqlCommand cmd = new SqlCommand("DELETE FROM Meet", con2))
            {
                con2.Open();
                cmd.ExecuteNonQuery();
                con2.Close();
            }
        }
    }
}
