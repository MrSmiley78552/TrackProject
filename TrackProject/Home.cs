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
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
        }

        public void setAllPanelsVisibleFalse()
        {
            Panel [] panels = {homePanel, goldenEagleAwardWinnersPanel, stateQualifierListPanel, qualifyingStandardsPanel,
                        teamSuccessPanel, individualSuccessPanel, daviesAllTimeTopPerformancesPanel, edcRecordsPanel,
                        stateRecordsPanel, ndAllTimeTop10Panel, letteringStandardsPanel, nationalsQualifyingStandardsPanel,
                        archivedSeasonsPanel, athletesHomePanel};
            foreach (var panel in panels)
                panel.Visible = false;
        }

        private void homeLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            homePanel.Visible = true;
        }

        private void goldenEagleAwardWinnersLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            goldenEagleAwardWinnersPanel.Visible = true;
        }

        private void stateQualifierListLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            stateQualifierListPanel.Visible = true;
        }

        private void qualifyingStandardsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            qualifyingStandardsPanel.Visible = true;
        }

        private void teamSuccessLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            teamSuccessPanel.Visible = true;
        }

        private void individualSuccessLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            individualSuccessPanel.Visible = true;
        }

        private void daviesAllTimeTopPerformancesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            daviesAllTimeTopPerformancesPanel.Visible = true;
        }

        private void edcRecordsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            edcRecordsPanel.Visible = true;
        }

        private void stateRecordsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            stateRecordsPanel.Visible = true;
        }

        private void ndAllTimeTop10Link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            ndAllTimeTop10Panel.Visible = true;
        }

        private void letteringStandardsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            letteringStandardsPanel.Visible = true;
        }

        private void nationalsQualifyingStandardsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            nationalsQualifyingStandardsPanel.Visible = true;
        }

        private void archivedSeasonsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setAllPanelsVisibleFalse();
            archivedSeasonsPanel.Visible = true;
        }

        private void athletesButton_Click(object sender, EventArgs e)
        {
            SqlDataReader sqlReader;
            setAllPanelsVisibleFalse();
            athletesHomePanel.Visible = true;
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
                while(sqlReader.Read())
                {
                    var listViewItem = new ListViewItem(sqlReader.GetString(0) + " " +sqlReader.GetString(1));
                    athletesListView.Items.Add(listViewItem); 
                }
                sqlReader.Close();
            }
            sqlReader.Close();
            conn.Close();
        }

        private void athletesListView_MouseClick(object sender, MouseEventArgs e)
        {
            string[] splitNames = athletesListView.SelectedItems[0].Text.Split(' ');

            int aIdFromDatabase = getAIDFromDatabase(splitNames[0], splitNames[1]);

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
