﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BVEPackageInstaller
{
    public partial class InstallProgress : Form
    {
        public InstallProgress()
        {
            InitializeComponent();
        }

        public int ProgressValue
        {
            set { progressBar1.Value = value; }
        }
    }
}
