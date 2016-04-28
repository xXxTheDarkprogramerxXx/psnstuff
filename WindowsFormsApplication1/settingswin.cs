using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace psnstuff
{
    public partial class settingswin : Form
    {
        public settingswin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog savepath = new FolderBrowserDialog();
            savepath.RootFolder = Environment.SpecialFolder.MyDocuments;
            savepath.ShowNewFolderButton = true;
            savepath.Description = "Please select a location to save your psn titles";
            if (savepath.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = savepath.SelectedPath + "\\PSNStuff\\Downloads";
                if(!Directory.Exists(savepath.SelectedPath + "\\PSNStuff\\Downloads"))
                {
                    Directory.CreateDirectory(savepath.SelectedPath + "\\PSNStuff\\Downloads");
                }
                Properties.Settings.Default.DownloadDirectory = textBox1.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void settingswin_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.DownloadDirectory != string.Empty)
            {
                textBox1.Text = Properties.Settings.Default.DownloadDirectory.ToString();
            }
            else
            {
                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PSNStuff\\Downloads";
                Properties.Settings.Default.DownloadDirectory = textBox1.Text;
                Properties.Settings.Default.Save();
            }

            //load the usb drives
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    comboBox1.Items.Add(drive.Name);
                }
            }
            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Text = "No Removable Drives detected";
            }
            //check for download helper

            if (File.Exists(Application.StartupPath + "\\Download Helper.exe"))
            {
                pictureBox1.Image = Properties.Resources.gruener_haken;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.rotesX;
                if (MessageBox.Show("Download Helper is not installed would you like to download it now ?", "Download Helper", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    //download the file to the startup path
                }
            }

            //Load Versions From Servers
            lblCurV.Text = Application.ProductVersion;

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {

                lblServerV.Text = "Network not avaialable";
            }
            else
            {
                //load from server here
                lblServerV.Text = "3.0.0.0";
            }
        }
    }
}
