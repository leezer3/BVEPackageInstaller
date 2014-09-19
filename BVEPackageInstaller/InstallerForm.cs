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
        public int csvfiles = 0;
        public int b3dfiles = 0;
        public int soundfiles = 0;
        public int imagefiles = 0;
        public int trainfiles = 0;
        public bool pathfound;
        InstallProgress progress;
        ReadingPackage progress1;
        ArchiveContents contents;
        public StartWindowForm.PackageInformation currentpackage;
        public static List<string> archivecontents = new List<string>();
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
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

        }
        string openfile;
        string packageimage;
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
            else if (Path.GetExtension(openfile) == ".zip" || Path.GetExtension(openfile) == ".7z" || Path.GetExtension(openfile) == ".rar")
            {
                //We're an archive, so go to the archive tab
                tabControl1.SelectedTab = tabPage3;
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
            case 2:
                //Try to load the contents of an archive
                if (ArchiveInfoWorker.IsBusy != true)
                {
                    progress1 = new ReadingPackage();
                    progress1.Show();
                    ArchiveInfoWorker.RunWorkerAsync();
                }
                break;
            case 3:
                //Package extraction
                if (InstallerWorker.IsBusy != true)
                {
                    progress = new InstallProgress();
                    progress.Show();
                    InstallerWorker.RunWorkerAsync();
                }   
                break;
                
           }   
        }

        //This event handler processes the extraction of the package
        private void InstallerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string extractiondirectory = StartWindowForm.OpenBVELocation + currentpackage.installpath;
            var installpackage = new SevenZip.SevenZipExtractor(openfile);
            installpackage.ExtractArchive(Path.Combine(StartWindowForm.OpenBVELocation + "\\" + currentpackage.installpath));
            StartWindowForm.installedpackages.Add(currentpackage.guid, currentpackage);
            //Move the package image to the database if we created one
            if (File.Exists(Path.Combine(StartWindowForm.OpenBVELocation + "\\package.png")))
            {
                File.Move(Path.Combine(StartWindowForm.OpenBVELocation  + "\\package.png"), Path.Combine(StartWindowForm.imagefolder + "\\" + currentpackage.guid + ".png"));
            }
            //Delete the package info file we extracted
            if (File.Exists(Path.Combine(StartWindowForm.OpenBVELocation + "\\packageinfo.bpi")))
            {
                File.Delete(Path.Combine(StartWindowForm.OpenBVELocation + "\\packageinfo.bpi"));
            }
            //If we have a package image set for an archive, copy it rather than moving it
            if (File.Exists(packageimage))
            {
                File.Copy(packageimage, Path.Combine(StartWindowForm.imagefolder + "\\" + currentpackage.guid + ".png"));
            }
            updatedatabase();
        }

        //Update the progress percentage
        private void InstallerWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.ProgressValue = e.ProgressPercentage;
        }

        //Close the installer progress box when the worker has been completed
        private void InstallerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress.Close();
        }

        //This event handler processes the archive's contents
        private void ArchiveInfoWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            var readarchive = new SevenZip.SevenZipExtractor(openfile);
            //Read the archive's contents into our list
            archivecontents = readarchive.ArchiveFileNames.ToList<string>();
            if (archivecontents.Count > 0)
            {
                {
                    foreach (string item in archivecontents)
                    {
                        //Test potential paths
                        //
                        //Complete, easily identifiable packages
                        if (item.StartsWith("railway\\", StringComparison.OrdinalIgnoreCase))
                        {
                            currentpackage.installpath = "";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "ROUTE";
                            });
                            pathfound = true;
                            break;
                        }
                        else if (item.StartsWith("route\\", StringComparison.OrdinalIgnoreCase) || item.StartsWith("object\\", StringComparison.OrdinalIgnoreCase) || item.StartsWith("sound\\", StringComparison.OrdinalIgnoreCase))
                        {
                            currentpackage.installpath = "\\Railway";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "ROUTE COMPONENT (Full path found)";
                            });
                            pathfound = true;
                            break;
                        }
                        else if (item.StartsWith("train\\", StringComparison.OrdinalIgnoreCase))
                        {
                            currentpackage.installpath = "";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "TRAIN (Full path found)";
                            });
                            pathfound = true;
                            break;
                        }
                        else if (item.EndsWith("train.dat", StringComparison.OrdinalIgnoreCase) || item.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            //We've found a train.dat file or a DLL
                            trainfiles++;
                        }
                        else if (item.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                        {
                            //We've found a CSV file, increment the counter by one
                            csvfiles++;
                        }
                        else if (item.EndsWith(".b3d", StringComparison.OrdinalIgnoreCase))
                        {
                            //We've found a CSV file, increment the counter by one
                            b3dfiles++;
                        }
                        else if (item.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || item.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                        {
                            //We've found a CSV file, increment the counter by one
                            soundfiles++;
                        }
                        else if (item.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || item.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) || item.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        {
                            //We've found an image file, increment the counter by one
                            imagefiles++;
                        }
                    }

                    if (pathfound == false)
                    {
                        //No easily identifiable path was found- Try and figure it out manually from the archive contents.
                        if ((csvfiles < 50 || b3dfiles < 50) && imagefiles < 50)
                        {
                            //We've got more than 50 objects and images
                            //This suggests that this is an *object* archive
                            currentpackage.installpath = "\\Railway\\Object";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "ROUTE OBJECTS (Full path not found)";
                            });
                        }
                        else if (soundfiles < 10 && trainfiles == 0)
                        {
                            //We've got more than 10 sounds and no train components
                            //This suggests that this is a sounds package
                            currentpackage.installpath = "\\Railway\\Sound";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "ROUTE SOUNDS (Full path not found)";
                            });
                        }
                        else if (csvfiles < 5 && imagefiles > 10)
                        {
                            //We've got more than 5 CSV files and less than 10 images
                            //This suggests that this is a route package
                            currentpackage.installpath = "\\Railway\\Route";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "ROUTEFILES (Full path not found)";
                            });
                        }
                        else if (soundfiles < 10 && trainfiles < 0)
                        {
                            //We've got more than 10 sounds and train components
                            //This suggests that this is a train
                            currentpackage.installpath = "\\Train";
                            this.Invoke((MethodInvoker)delegate
                            {
                                label2.Text = "TRAIN (Full path not found)";
                            });
                        }
                        
                    }
                    //Print Path
                    this.Invoke((MethodInvoker)delegate
                    {
                        label5.Text = Path.Combine(StartWindowForm.OpenBVELocation, currentpackage.installpath);
                    });
                    
                }
            }
        }

        //Update the progress percentage
        private void ArchiveInfoWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress1.ProgressValue = e.ProgressPercentage;
        }

        //Close the installer progress box when the worker has been completed
        private void ArchiveInfoWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress1.Close();
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

        private void button6_Click(object sender, EventArgs e)
        {
            contents = new ArchiveContents(archivecontents);
            contents.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage5;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Load the values from the text boxes
            //Set the GUID to be the archive's filename for the moment
            //Change this??
            currentpackage.guid = Path.GetFileNameWithoutExtension(openfile);
            currentpackage.name = textBox6.Text;
            currentpackage.author = textBox7.Text;
            currentpackage.version = (double)numericUpDown1.Value;
            currentpackage.weburl = textBox8.Text;
            //Switch to the installer tab and voila.....
            tabControl1.SelectedTab = tabPage4;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            packageimage = openFileDialog1.FileName;
            Bitmap tempimage;
            using (FileStream myStream = new FileStream(packageimage, FileMode.Open))
            {
                tempimage = (Bitmap)Image.FromStream(myStream);
                this.pictureBox1.Image = tempimage;
                this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }   
    }
}
