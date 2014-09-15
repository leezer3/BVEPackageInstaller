using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BVEPackageInstaller
{
    public partial class InstallerForm : Form
    {
        
        public StartWindowForm.PackageInformation currentpackage;
        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        public InstallerForm()
        {
            InitializeComponent();
            tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);

        }
        string openfile;
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            openfile = openFileDialog1.FileName;
            selectedfile.Text = openfile;
        }

        //Click on initial button- Detect if it's an archive or a package, and load appropriate tab
        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(openfile))
            {
                MessageBox.Show("The selected file does not exist.");
            }
            else if (Path.GetExtension(openfile) == ".bpk")
            {
                //We're a package, so go to the package tab
                tabControl1.SelectedTab = tabPage2;
            }
            else
            {
                tabControl1.SelectedTab = tabPage4;
            }
        }


        //Event handler for selected tabs
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            string temppath = GetTemporaryDirectory();
            SevenZip.SevenZipExtractor.SetLibraryPath(@"D:\Program Files (x86)\7-Zip\7z.dll");
        switch((sender as TabControl).SelectedIndex) {
            case 0:
                //Starting tab, twiddle
                break;
            case 1:
                //We've selected the second tab and need to load the package information from the archive
                if (File.Exists(openfile))
                {
                    string tempextract = Path.Combine(temppath + "\\packageinfo.bpi");
                    using (FileStream myStream = new FileStream(tempextract, FileMode.Create))
                    {
                        
                        var packageinfofile = new SevenZip.SevenZipExtractor(openfile);
                        packageinfofile.ExtractFile("packageinfo.bpi", myStream);
                    }
                    readpackageinfo(tempextract);
                }
                break;
            case 3:
                //Package extraction
                string extractiondirectory = StartWindowForm.OpenBVELocation + currentpackage.installpath;
                var installpackage = new SevenZip.SevenZipExtractor(openfile);
                installpackage.ExtractArchive(Path.Combine(StartWindowForm.OpenBVELocation + "\\" + currentpackage.installpath));

                //So, we've extracted the archive. Now add it to package manager list of installed packages
                List<string> installedpackages = new List<string>(File.ReadAllLines(StartWindowForm.database));
                //Remove EOF
                installedpackages.RemoveAt(installedpackages.Count-1);
                //Add package to list
                installedpackages.Add("["+currentpackage.guid+"]");
                installedpackages.Add("name="+currentpackage.name);
                installedpackages.Add("version="+currentpackage.version);
                installedpackages.Add("requiredpackages="+currentpackage.requiredpackages);
                installedpackages.Add("author="+currentpackage.author);
                installedpackages.Add("weburl="+currentpackage.weburl);
                installedpackages.Add("#EOF");
                File.Delete(StartWindowForm.database);
                //Recreate and write out
                using (StreamWriter sw = File.CreateText(StartWindowForm.database))
                {
                    foreach (string item in installedpackages)
                            {
                                sw.WriteLine(item);
                            }
                }
                break;
                
           }   
        }

        private void readpackageinfo(string tempextract)
        {
            //The archive information has been extracted, now try reading the package info file
            try
            {
                string[] datafile = File.ReadAllLines(tempextract);
                
                currentpackage.guid = "0";
                currentpackage.name = "";
                currentpackage.version = "0";
                currentpackage.requiredpackages = "";
                currentpackage.author = "";
                currentpackage.weburl = "";
                foreach (string line in datafile)
                {

                    //Test to see if the current line is a package GUID
                    if (line[0] == '[' & line[line.Length - 1] == ']')
                    {
                        string guid = line.Substring(1, line.Length - 2);
                        currentpackage.guid = guid;
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
                                    break;
                                case "version":
                                    currentpackage.version = value;
                                    break;
                                case "requiredpackages":
                                    currentpackage.requiredpackages = value;
                                    break;
                                case "author":
                                    currentpackage.author = value;
                                    break;
                                case "weburl":
                                    currentpackage.weburl = value;
                                    break;
                                case "installpath":
                                    currentpackage.installpath = value;
                                    break;
                            }

                        }

                    }

                }
                //We are only worried about displaying the useful bits here
                label6.Text = currentpackage.name;
                label7.Text = currentpackage.author;
                label8.Text = currentpackage.version;
                linkLabel1.Text = currentpackage.weburl;
            }
            catch
            {
                MessageBox.Show("An unexpected error occured whilst reading the package information.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage4;
        }
    }
}
