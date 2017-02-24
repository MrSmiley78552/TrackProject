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
            conn.Close();
        }

        private void athletesListView_MouseClick(object sender, MouseEventArgs e)
        {
            //0 = rId, 1 = time, 2 = distance, 3 = mId, 4 = place, 5 = trackEvent, 6 = finals
            string[,] results = new string[100, 7];

            string[] splitNames = athletesListView.SelectedItems[0].Text.Split(' ');

            int aIdFromDatabase = getAIDFromDatabase(splitNames[0], splitNames[1]);

            //--------------------------------------------------------
            SqlDataReader sqlReader;
            string ssConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Mitchell\Desktop\TrackProject\TrackProject\TrackProject\TrackAthleteRecords.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(ssConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM Record WHERE aId = @aId";
            command.Parameters.AddWithValue("@aId", aIdFromDatabase);
            command.CommandType = CommandType.Text;
            command.Connection = conn;
            sqlReader = command.ExecuteReader();
            if (sqlReader.HasRows)
            {
                int i = 0;
                while (sqlReader.Read())
                {
                    //0 = rId, 1 = time, 2 = distance, 3 = mId, 4 = place, 5 = trackEvent, 6 = finals
                    var test = double.Parse(sqlReader.GetString(1));
                    results[i, 0] = "" + sqlReader.GetInt32(0);
                    results[i, 1] = sqlReader.GetString(1);
                    results[i, 2] = sqlReader.GetString(2);
                    results[i, 3] = "" + sqlReader.GetInt32(4);
                    results[i, 4] = "" + sqlReader.GetInt32(5);
                    results[i, 5] = sqlReader.GetString(6);
                    results[i, 6] = "" + sqlReader.GetInt32(7);
                    i++;
                }
            }
            sqlReader.Close();
            conn.Close();
            //--------------------------------------------------------

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

        
    }

}
