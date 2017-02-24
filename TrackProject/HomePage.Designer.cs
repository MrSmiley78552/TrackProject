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
            this.athletesListView.HotTracking = true;
            this.athletesListView.HoverSelection = true;
            this.athletesListView.Location = new System.Drawing.Point(12, 82);
            this.athletesListView.Name = "athletesListView";
            this.athletesListView.Size = new System.Drawing.Size(121, 318);
            this.athletesListView.TabIndex = 0;
            this.athletesListView.UseCompatibleStateImageBehavior = false;
            this.athletesListView.View = System.Windows.Forms.View.List;
            this.athletesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.athletesListView_MouseClick);
            // 
            // HomePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 412);
            this.Controls.Add(this.athletesListView);
            this.Controls.Add(this.athletesLabel);
            this.Name = "HomePage";
            this.Text = "HomePage";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label athletesLabel;
        private System.Windows.Forms.ListView athletesListView;
    }
}