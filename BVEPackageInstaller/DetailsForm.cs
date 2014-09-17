using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace BVEPackageInstaller
{
    public partial class DetailsForm : Form
    {
        public DetailsForm(string selectedpackage)
        {
            InitializeComponent();
            label6.Text = StartWindowForm.installedpackages[selectedpackage].name;
            label7.Text = StartWindowForm.installedpackages[selectedpackage].author;
            label8.Text = Convert.ToString(StartWindowForm.installedpackages[selectedpackage].version);
            linkLabel1.Text = StartWindowForm.installedpackages[selectedpackage].weburl;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url;
            if (e.Link.LinkData != null)
                url = e.Link.LinkData.ToString();
            else
                url = linkLabel1.Text.Substring(e.Link.Start, e.Link.Length);

            if (!url.Contains("://"))
                url = "http://" + url;

            var si = new ProcessStartInfo(url);
            Process.Start(si);
            linkLabel1.LinkVisited = true;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
