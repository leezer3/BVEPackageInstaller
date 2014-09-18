using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BVEPackageInstaller
{
    public partial class ArchiveContents : Form
    {
        public ArchiveContents(List<string> archivecontents)
        {
            InitializeComponent();
            ArchiveFiles.DataSource = archivecontents;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
