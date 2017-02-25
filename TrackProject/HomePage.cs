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
            MessageBox.Show("HI: " + athletesListView.SelectedItems[0].Text + "   AID: " + aIdFromDatabase);
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
            string[] events = getEventsForAthlete(aId);
            foreach(var tabEvent in events)
            {
                TabPage tempTabPage = new TabPage();
                tabBox.Controls.Add(tempTabPage);
                tempTabPage.Text = tabEvent;
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
                //Want to double check this chunk to ensure that it works as expected.
                //suspect problems with where it breaks to. 
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
    }
}
