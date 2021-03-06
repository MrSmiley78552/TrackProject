﻿using System.Windows.Forms;

namespace TrackProject
{
    partial class HomePage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.athletesLabel = new System.Windows.Forms.Label();
            this.athletesListView = new System.Windows.Forms.ListView();
            this.col1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.meetsPanel = new System.Windows.Forms.Panel();
            this.meetsListView = new System.Windows.Forms.ListView();
            this.columnHeaderMeetName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.athletePanel = new System.Windows.Forms.Panel();
            this.meetsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // athletesLabel
            // 
            this.athletesLabel.AutoSize = true;
            this.athletesLabel.BackColor = System.Drawing.SystemColors.Control;
            this.athletesLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.athletesLabel.Location = new System.Drawing.Point(33, 60);
            this.athletesLabel.Name = "athletesLabel";
            this.athletesLabel.Size = new System.Drawing.Size(71, 19);
            this.athletesLabel.TabIndex = 1;
            this.athletesLabel.Text = "Athletes";
            // 
            // athletesListView
            // 
            this.athletesListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.athletesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col1});
            this.athletesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.athletesListView.HotTracking = true;
            this.athletesListView.HoverSelection = true;
            this.athletesListView.Location = new System.Drawing.Point(12, 82);
            this.athletesListView.Name = "athletesListView";
            this.athletesListView.Size = new System.Drawing.Size(121, 318);
            this.athletesListView.TabIndex = 0;
            this.athletesListView.UseCompatibleStateImageBehavior = false;
            this.athletesListView.View = System.Windows.Forms.View.Details;
            this.athletesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.athletesListView_MouseClick);
            // 
            // col1
            // 
            this.col1.Name = "col1";
            this.col1.Text = "";
            this.col1.Width = 150;
            // 
            // meetsPanel
            // 
            this.meetsPanel.Controls.Add(this.meetsListView);
            this.meetsPanel.Location = new System.Drawing.Point(162, 82);
            this.meetsPanel.Name = "meetsPanel";
            this.meetsPanel.Size = new System.Drawing.Size(396, 318);
            this.meetsPanel.TabIndex = 2;
            // 
            // meetsListView
            // 
            this.meetsListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.meetsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMeetName,
            this.columnHeaderDate});
            this.meetsListView.HoverSelection = true;
            this.meetsListView.Location = new System.Drawing.Point(0, 0);
            this.meetsListView.Name = "meetsListView";
            this.meetsListView.Size = new System.Drawing.Size(396, 318);
            this.meetsListView.TabIndex = 0;
            this.meetsListView.UseCompatibleStateImageBehavior = false;
            this.meetsListView.View = System.Windows.Forms.View.Details;
            this.meetsListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.meetsListView_MouseClick);
            // 
            // columnHeaderMeetName
            // 
            this.columnHeaderMeetName.Text = "Meet";
            this.columnHeaderMeetName.Width = 300;
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.Text = "Date";
            // 
            // athletePanel
            // 
            this.athletePanel.Location = new System.Drawing.Point(162, 79);
            this.athletePanel.Name = "athletePanel";
            this.athletePanel.Size = new System.Drawing.Size(396, 318);
            this.athletePanel.TabIndex = 3;
            // 
            // HomePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 412);
            this.Controls.Add(this.athletePanel);
            this.Controls.Add(this.meetsPanel);
            this.Controls.Add(this.athletesListView);
            this.Controls.Add(this.athletesLabel);
            this.Name = "HomePage";
            this.Text = "HomePage";
            this.meetsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label athletesLabel;
        private System.Windows.Forms.ListView athletesListView;
        private ColumnHeader col1;
        private Panel meetsPanel;
        private Panel athletePanel;
        private ListView meetsListView;
        private ColumnHeader columnHeaderMeetName;
        private ColumnHeader columnHeaderDate;
    }
}