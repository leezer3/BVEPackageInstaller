namespace BVEPackageInstaller
{
    partial class StartWindowForm
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
            this.detailsbutton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.packagedisplay = new System.Windows.Forms.ListView();
            this.InstallButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // detailsbutton
            // 
            this.detailsbutton.Location = new System.Drawing.Point(15, 133);
            this.detailsbutton.Name = "detailsbutton";
            this.detailsbutton.Size = new System.Drawing.Size(141, 23);
            this.detailsbutton.TabIndex = 0;
            this.detailsbutton.Text = "Show Package Details";
            this.detailsbutton.UseVisualStyleBackColor = true;
            this.detailsbutton.Click += new System.EventHandler(this.detailsbutton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Currently Installed Packages:";
            // 
            // packagedisplay
            // 
            this.packagedisplay.Location = new System.Drawing.Point(12, 30);
            this.packagedisplay.Name = "packagedisplay";
            this.packagedisplay.Size = new System.Drawing.Size(480, 97);
            this.packagedisplay.TabIndex = 2;
            this.packagedisplay.UseCompatibleStateImageBehavior = false;
            this.packagedisplay.View = System.Windows.Forms.View.Details;
            this.packagedisplay.SelectedIndexChanged += new System.EventHandler(this.packagedisplay_SelectedIndexChanged);
            // 
            // InstallButton
            // 
            this.InstallButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InstallButton.Location = new System.Drawing.Point(12, 226);
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(181, 51);
            this.InstallButton.TabIndex = 3;
            this.InstallButton.Text = "Install New Package...";
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(173, 133);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(144, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Set OpenBVE Location";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::BVEPackageInstaller.Properties.Resources.OpenBVELogo;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(335, 139);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(157, 157);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // StartWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 308);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.InstallButton);
            this.Controls.Add(this.packagedisplay);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.detailsbutton);
            this.Name = "StartWindowForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button detailsbutton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button InstallButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListView packagedisplay;
    }
}

