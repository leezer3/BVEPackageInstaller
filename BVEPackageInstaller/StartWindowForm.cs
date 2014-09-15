using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BVEPackageInstaller
{
    public partial class StartWindowForm : Form
    {
        //Holds the currently installed packages
        public static Dictionary<string, PackageInformation> installedpackages = new Dictionary<string, PackageInformation>();
        public static string OpenBVELocation;
        public static string database;
        public StartWindowForm()
        {
            
            //Load OpenBVE path from registry
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\BVEPackageInstaller", true))
            {
                if (key != null)
                {
                    OpenBVELocation = Convert.ToString(key.GetValue("OpenBVELocation"));
                }
                else
                {
                    using (var key2 = Registry.CurrentUser.CreateSubKey(@"Software\BVEPackageInstaller"))
                    {
                        MessageBox.Show("You have not currently set your OpenBVE directory's location. \n \n Please do this before continuing.");
                    }
                }
            }
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string programfolder = Path.Combine(appdata + "\\BVEPackageInstaller");
            //Load database from file
            if (!Directory.Exists(programfolder))
            {
                Directory.CreateDirectory(programfolder);
            }
            database = Path.Combine(programfolder + "\\packages.dat");
            if (!File.Exists(database))
            {
                File.Create(database);
            }
            else
            {
                try
                {
                    string[] datafile = File.ReadAllLines(database);
                    PackageInformation currentpackage;
                    currentpackage.guid = "0";
                    currentpackage.name = "";
                    currentpackage.version = "0";
                    currentpackage.requiredpackages = "";
                    currentpackage.author = "";
                    currentpackage.weburl = "";
                    currentpackage.installpath = "";
                    int i = 0;
                    foreach (string line in datafile)
                    {
                        
                        //Test to see if the current line is a package GUID
                        if (line[0] == '[' & line[line.Length - 1] == ']')
                        {
                            if (i != 0)
                            {
                                installedpackages.Add(currentpackage.guid, currentpackage);
                            }
                            string guid = line.Substring(1, line.Length - 2);
                            currentpackage.guid = guid;
                            currentpackage.name = "";
                            currentpackage.version = "0";
                            currentpackage.requiredpackages = "";
                            currentpackage.author = "";
                            currentpackage.weburl = "";
                            
                            
                        }
                        //If not assume that it's a property and add it to the information
                        else
                        {
                            int equals = line.IndexOf('=');
                            if (equals >= 0)
                            {
                                string key = line.Substring(0, equals).Trim().ToLowerInvariant();
                                string value = line.Substring(equals + 1).Trim();
                                switch (key)
                                {
                                    case "name":
                                        currentpackage.name = value;
                                        i++;
                                        break;
                                    case "version":
                                        currentpackage.version = value;
                                        i++;
                                        break;
                                    case "requiredpackages":
                                        currentpackage.requiredpackages = value;
                                        i++;
                                        break;
                                    case "author":
                                        currentpackage.author = value;
                                        i++;
                                        break;
                                    case "weburl":
                                        currentpackage.weburl = value;
                                        i++;
                                        break;
                                    case "installpath":
                                        currentpackage.installpath = value;
                                        i++;
                                        break;
                                }

                            }
                            //Handle EOF
                            else if (line == "#EOF")
                            {
                                installedpackages.Add(currentpackage.guid, currentpackage);
                            }

                        }
                        
                    }
                    
                }
                catch
                {
                    MessageBox.Show("An unexpected error occured whilst reading the database.");
                }
            }

            

            
            InitializeComponent();
            //Populate package list display
            packagedisplay.Columns.Add("Package ID");
            packagedisplay.Columns.Add("Package Name");
            packagedisplay.Columns.Add("Package Author");
            packagedisplay.Columns.Add("Package Version");
            foreach (KeyValuePair<string, PackageInformation> kvp in installedpackages)
            {
                ListViewItem item = new ListViewItem(kvp.Value.guid);
                item.SubItems.Add(kvp.Value.name);
                item.SubItems.Add(kvp.Value.author);
                item.SubItems.Add(kvp.Value.version);
                packagedisplay.Items.Add(item);
            }
            packagedisplay.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            packagedisplay.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            
        }

        private void detailsbutton_Click(object sender, EventArgs e)
        {
            using (DetailsForm childform = new DetailsForm(packagedisplay.SelectedItems[0].Text))
            {
                childform.ShowDialog(this);
            }
        }

        //Package Information Structure
        public struct PackageInformation
        {
            //The package GUID
            public string guid;
            //The package name
            public string name;
            //The package version number
            public string version;
            //Other required packages
            public string requiredpackages;
            //The author of the package
            public string author;
            //The website associated with the package
            public string weburl;
            //The install path of the package
            public string installpath;
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            using (InstallerForm childform = new InstallerForm())
            {
                childform.ShowDialog(this);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenBVELocation = folderBrowserDialog1.SelectedPath;
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\BVEPackageInstaller", true))
                {
                    key.SetValue("OpenBVELocation",OpenBVELocation);
                }
            }
        }

    }
}
