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
                    string tempextract_info = Path.Combine(temppath + "\\packageinfo.bpi");
                    try
                    {
                        using (FileStream myStream = new FileStream(tempextract_info, FileMode.Create))
                        {

                            var packageinfofile = new SevenZip.SevenZipExtractor(openfile);
                            packageinfofile.ExtractFile("packageinfo.bpi", myStream);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("The selected package appears to be corrupt.");
                    }
                    try
                    {
                        readpackageinfo(tempextract_info);
                    }
                    catch
                    {
                        MessageBox.Show("Error reading package information.");
                    }
                    //Now extract the image
                    try
                    {
                        string tempextract_image = Path.Combine(temppath + "\\package.png");
                        using (FileStream myStream = new FileStream(tempextract_image, FileMode.Create))
                        {

                            var packageimage = new SevenZip.SevenZipExtractor(openfile);
                            packageimage.ExtractFile("package.png", myStream);
                        }
                        //Read the package information
                        readpackageinfo(tempextract_info);
                        //Load the image
                        Bitmap tempimage;
                        using (FileStream myStream = new FileStream(tempextract_image, FileMode.Open))
                        {
                            tempimage = (Bitmap)Image.FromStream(myStream);
                            tempimage.MakeTransparent(Color.FromArgb(0, 0, 255));
                            this.pictureBox1.Image = tempimage;
                            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    catch
                    {
                        //No error prompt, just don't load the image
                    }
                }
                break;
            case 3:
                //Package extraction
                string extractiondirectory = StartWindowForm.OpenBVELocation + currentpackage.installpath;
                var installpackage = new SevenZip.SevenZipExtractor(openfile);
                installpackage.ExtractArchive(Path.Combine(StartWindowForm.OpenBVELocation + "\\" + currentpackage.installpath));
                StartWindowForm.installedpackages.Add(currentpackage.guid, currentpackage);
                updatedatabase();
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
                currentpackage.version = 0;
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
                                    currentpackage.version = Double.Parse(value);
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
                label8.Text = Convert.ToString(currentpackage.version);
                linkLabel1.Text = currentpackage.weburl;
            }
            catch
            {
                MessageBox.Show("An unexpected error occured whilst reading the package information.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!StartWindowForm.installedpackages.ContainsKey(currentpackage.guid))
            {
                tabControl1.SelectedTab = tabPage4;
            }
            else if (StartWindowForm.installedpackages[currentpackage.guid].version == currentpackage.version)
            {
                MessageBox.Show("This package is already installed.");
            }
            else if (StartWindowForm.installedpackages[currentpackage.guid].version < currentpackage.version)
            {
                if (MessageBox.Show("An older version of this package is currently installed. \n \n Do you wish to replace it?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    tabControl1.SelectedTab = tabPage4;
                }
            }
        }

        private void updatedatabase()
        {
            File.Delete(StartWindowForm.database);
            //Recreate and write out
            using (StreamWriter sw = File.CreateText(StartWindowForm.database))
            {
                foreach (KeyValuePair<string, StartWindowForm.PackageInformation> package in StartWindowForm.installedpackages)
                {
                    StartWindowForm.PackageInformation currentpackage = StartWindowForm.installedpackages[package.Key];
                    sw.WriteLine("[" + currentpackage.guid + "]");
                    sw.WriteLine("name=" + currentpackage.name);
                    sw.WriteLine("version=" + currentpackage.version);
                    sw.WriteLine("requiredpackages=" + currentpackage.requiredpackages);
                    sw.WriteLine("author=" + currentpackage.author);
                    sw.WriteLine("weburl=" + currentpackage.weburl);
                }
                sw.WriteLine("#EOF");
            }
        }
    }
}
