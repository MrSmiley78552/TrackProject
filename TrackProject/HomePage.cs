using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrackProject
{
    public partial class HomePage : Form
    {
        public HomePage()
        {
            InitializeComponent();

            //set initial states of panels-----------------------
            meetsPanel.Visible = true;
            athletePanel.Visible = false;
            //---------------------------------------------------
            //Load the Athletes side bar-----------------------------
            SqlDataReader sqlReader;
            string ssConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(ssConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT fName, lName FROM Athlete";
            command.CommandType = CommandType.Text;
            command.Connection = conn;
            sqlReader = command.ExecuteReader();
            if (sqlReader.HasRows)
            {
                while (sqlReader.Read())
                {
                    var listViewItem = new ListViewItem(sqlReader.GetString(0) + " " + sqlReader.GetString(1));
                    athletesListView.Items.Add(listViewItem);
                }
                sqlReader.Close();
            }
            sqlReader.Close();
            //---------------------------------------------------
            //Load the meets panel-------------------------------
            SqlDataReader sqlReader2;
            string ssConnectionString2 = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True";
            SqlConnection conn2 = new SqlConnection(ssConnectionString2);
            conn2.Open();
            SqlCommand command2 = conn2.CreateCommand();
            command2.CommandText = "SELECT meetName, date FROM Meet";
            command2.CommandType = CommandType.Text;
            command2.Connection = conn2;
            sqlReader2 = command2.ExecuteReader();
            if (sqlReader2.HasRows)
            {
                while (sqlReader2.Read())
                {
                    var listViewItem = new ListViewItem(new[] { sqlReader2.GetString(0), sqlReader2.GetString(1) });
                    meetsListView.Items.Add(listViewItem);
                }
                sqlReader2.Close();
            }
            sqlReader2.Close();
            conn2.Close();
            //---------------------------------------------------

        }

        private void athletesListView_MouseClick(object sender, MouseEventArgs e)
        {
            //0 = rId, 1 = time, 2 = distance, 3 = mId, 4 = place, 5 = trackEvent, 6 = finals
            string[,] results = new string[100, 7];

            string[] splitNames = athletesListView.SelectedItems[0].Text.Split(' ');

            int aIdFromDatabase = getAIDFromDatabase(splitNames[0], splitNames[1]);
            meetsPanel.Visible = false;
            athletePanel.Visible = true;
            populateTabBox(aIdFromDatabase);
        }

        private int getAIDFromDatabase(string fName, string lName)
        {
            SqlCommand cmd;
            SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True");
            con.Open();
            SqlDataReader sqlReader;
            cmd = new SqlCommand();
            cmd.CommandText = "SELECT aId FROM Athlete WHERE fName = @fName AND lName = @lName";
            cmd.Parameters.AddWithValue("@fName", fName);
            cmd.Parameters.AddWithValue("@lName", lName);
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

        private void populateTabBox(int aId)
        {
            athletePanel.Controls.Clear();
            TabControl dynamicTabControl = new TabControl();
            dynamicTabControl.Name = "DynamicTabControl";
            dynamicTabControl.BackColor = Color.White;
            dynamicTabControl.ForeColor = Color.Black;
            dynamicTabControl.Font = new Font("Arial", 8);
            dynamicTabControl.Width = 396;
            dynamicTabControl.Height = 318;
            athletePanel.Controls.Add(dynamicTabControl);
            //add the overall tab by default
            TabPage overallTabPage = new TabPage();
            dynamicTabControl.Controls.Add(overallTabPage);
            overallTabPage.Text = "Overall";

            string[] events = getEventsForAthlete(aId);
            foreach(var tabEvent in events)
            {
                if (tabEvent == null)
                    break;
                TabPage tempTabPage = new TabPage();
                dynamicTabControl.Controls.Add(tempTabPage);
                tempTabPage.Text = tabEvent;
                //this is how to add things to the tab pages
                tempTabPage.Controls.Add(createAthleteEventChart(aId, tabEvent));
            }
        }

        private string[] getEventsForAthlete(int aId)
        {
            string[] events = new string[20];

            SqlDataReader sqlReader;
            string ssConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(ssConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM Record WHERE aId = @aId";
            command.Parameters.AddWithValue("@aId", aId);
            command.CommandType = CommandType.Text;
            command.Connection = conn;
            sqlReader = command.ExecuteReader();
            if (sqlReader.HasRows)
            {
                int i = 0;
                while (sqlReader.Read())
                {
                    for(int curEvent = 0; curEvent < events.Length; curEvent++)
                    {
                        if (events[curEvent] != null && events[curEvent].Equals(sqlReader.GetString(6)))
                            break;
                    }
                    events[i] = "" + sqlReader.GetString(6);
                    i++;
                }
            }
            sqlReader.Close();
            conn.Close();
            return events;
        }
        public void meetsListView_MouseClick(object sender, MouseEventArgs e)
        {

        }

        //creates chart for individual events for an athlete
        private System.Windows.Forms.DataVisualization.Charting.Chart createAthleteEventChart(int aId, string eventName)
        {
            System.Windows.Forms.DataVisualization.Charting.Chart chart1;

            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();

            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(chart1)).BeginInit();
            //chart
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new System.Drawing.Point(46, 15);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chart1.Series.Add(series1);
            chart1.Size = new System.Drawing.Size(300, 270);
            chart1.TabIndex = 0;
            chart1.Text = "chart1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            //data points for the graph
            string[,] records = getRecordsForIndividualAthleteAndIndividualEvent(aId, eventName);
            for(int record = 0; record < records.GetLength(0); record++)
            {
                //here we will be getting a time
                if(records[record, 0] != null && !records[record, 0].Equals(""))
                {
                    //handling time in the 1:23.45 format
                    if(records[record, 0].Contains(':'))
                    {
                        string[] temp = records[record, 0].Split(':');
                        double convertedTime = Math.Round(Convert.ToDouble(temp[0]) * 60 + Convert.ToDouble(temp[1]), 2);
                        chart1.Series["Series1"].Points.AddXY(records[record, 0], convertedTime);
                    }
                    else
                        chart1.Series["Series1"].Points.AddXY(records[record, 0], records[record, 0]);
                }
                //here we will be getting a distance
                else if(records[record, 0] != null)
                {
                    if (records[record, 1].Contains('-'))
                    {
                        string[] temp = records[record, 1].Split('-');
                        double convertedDistance = Math.Round((Convert.ToDouble(temp[0]) * 12 + Convert.ToDouble(temp[1])) / 12, 2);
                        chart1.Series["Series1"].Points.AddXY(records[record, 1], convertedDistance);
                    }
                }
                else
                    break;
            }


            ((System.ComponentModel.ISupportInitialize)(chart1)).EndInit();

            return chart1;
        }

        private string[,] getRecordsForIndividualAthleteAndIndividualEvent(int aId, string eventName)
        {
            string[,] eventAndTime = new string[20, 4];

            SqlDataReader sqlReader;
            string ssConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(ssConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM Record WHERE aId = @aId AND event = @event";
            command.Parameters.AddWithValue("@aId", aId);
            command.Parameters.AddWithValue("@event", eventName);
            command.CommandType = CommandType.Text;
            command.Connection = conn;
            sqlReader = command.ExecuteReader();
            if (sqlReader.HasRows)
            {
                int recCount = 0;
                while (sqlReader.Read())
                {
                    string time = sqlReader.GetString(1);
                    eventAndTime[recCount, 0] = time;
                    string distance = sqlReader.GetString(2);
                    eventAndTime[recCount, 1] = distance;
                    int mId = sqlReader.GetInt32(4);
                    eventAndTime[recCount, 2] = "" + mId;
                    int finals = sqlReader.GetInt32(7);
                    eventAndTime[recCount, 3] = "" + finals;
                    recCount++;
                }
            }
            sqlReader.Close();
            conn.Close();
            return eventAndTime;
        }
    }
}
