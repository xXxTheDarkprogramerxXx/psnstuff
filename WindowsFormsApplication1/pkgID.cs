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
using System.Net;
using System.Security.Cryptography;

namespace psnstuff
{
    public partial class pkgID : Form
    {
        public pkgID()
        {
            InitializeComponent();
        }
        string shavalue;
        private void button1_Click_1(object sender, EventArgs e)
        {
            
            if (textBox1.Text.Contains("/cdn/"))
            {
                button2.Enabled = false;
                long sha1pos;
                long sizebyte;
                

                try
                {
                    textBox2.Text = "";
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    pictureBox2.Image = null;

                    string link = textBox1.Text;
                    string URL;
                    if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    {
                        URL = ("http://" + link);
                    }
                    else
                    {
                        URL = link;
                    }
                    
                    if (textBox1.Text.Contains(".pkg"))
                    {
                        HttpWebRequest request;
                        request = WebRequest.Create(URL) as HttpWebRequest;

                        request.AddRange(0, 319); //48, 83


                        using (WebResponse response = request.GetResponse())
                        {
                            string[] contran = response.Headers.GetValues(0);
                            string[] contrans = contran[0].Split('/');
                            double b = Convert.ToInt64(contrans[1]);
                            string sizeb = contrans[1];

                            sizebyte = Convert.ToInt64(contrans[1]);
                            sha1pos = Convert.ToInt64(sizeb) - 32;




                            //rest infos
                            if (b < 1048576)
                            {
                                double kb = Math.Round(b / 1024, 2);
                                textBox4.Text = kb + " KB" + " " + "(" + sizeb + " Byte)";
                            }
                            else
                            {
                                double mb = Math.Round(b / 1024 / 1024, 2);
                                textBox4.Text = mb + " MB" + " " + "(" + sizeb + " Byte)";
                            }
                            try
                            {
                                using (Stream stream = response.GetResponseStream())
                                {
                                    byte[] buffer = new byte[319];
                                    int read = stream.Read(buffer, 0, 319);
                                    //Array.Resize(ref buffer, read);

                                    byte[] contentID = new byte[36];
                                    Array.Copy(buffer, 48, contentID, 0, 36);

                                    //contentID
                                    textBox2.Text = Encoding.ASCII.GetString(contentID);

                                    //id
                                    textBox3.Text = textBox2.Text.Substring(7, 9);

                                    //pkgver
                                    byte[] ver = new byte[2];
                                    Array.Copy(buffer, 254, ver, 0, 2);
                                    string verstr = BitConverter.ToString(ver);
                                    verstr = verstr.Replace("-", ".");
                                    textBox6.Text = verstr;


                                    //fwcalc
                                    byte[] fwcal = new byte[2];
                                    Array.Copy(buffer, 38, fwcal, 0, 2);
                                    string fwcalcstr = BitConverter.ToString(fwcal);
                                    fwcalcstr = fwcalcstr.Replace("-", "");
                                    string fwcalc = (Convert.ToInt32(fwcalcstr) - 60 + 9).ToString();
                                    int fwcalcs = Convert.ToInt32(fwcalc, 16);

                                    //fw
                                    byte[] fw = new byte[2];
                                    Array.Copy(buffer, fwcalcs, fw, 0, 2);
                                    string fwstr = BitConverter.ToString(fw);
                                    fwstr = fwstr.Replace("-", ".");
                                    textBox5.Text = fwstr;

                                }
                            }
                            catch { }
                        }


                        //sha1

                        HttpWebRequest request2;
                        request2 = WebRequest.Create(URL) as HttpWebRequest;

                        request2.AddRange(sha1pos, sizebyte);

                        using (WebResponse response2 = request2.GetResponse())
                        {
                            using (Stream stream2 = response2.GetResponseStream())
                            {
                                byte[] buffer2 = new byte[20];
                                int read = stream2.Read(buffer2, 0, 20);
                                string sha1 = BitConverter.ToString(buffer2).ToLower();
                                sha1 = sha1.Replace("-", "");
                                textBox7.Text = sha1;
                            }
                        }

                    }
                    else { MessageBox.Show("Link missing or wrong"); }
                }
                catch { }
            }
           
            else if (textBox1.Text.Contains(".pkg"))
            {
                
                    button2.Enabled = true;
                    // Local pkg
                    BinaryReader local = new BinaryReader(File.OpenRead(textBox1.Text));
                
                    //contentID
                    local.BaseStream.Position = 0x30;
                    byte[] contID = local.ReadBytes(0x24);
                    textBox2.Text = Encoding.ASCII.GetString(contID);

                    //id
                    textBox3.Text = textBox2.Text.Substring(7, 9);
                    try
                    {
                    //pkgversion
                    local.BaseStream.Position = 0xfe;
                    byte[] pkgversion = local.ReadBytes(0x2);
                    string pkgver = BitConverter.ToString(pkgversion);
                    pkgver = pkgver.Replace("-", ".");
                    textBox6.Text = pkgver;
                    }
                    catch { }
                    //size
                    double si = local.BaseStream.Length;
                    if (si < 1048576)
                    {
                        double kb = Math.Round(si / 1024, 2);
                        textBox4.Text = kb + " KB" + " " + "(" + si + " Byte)";
                    }
                    else
                    {
                        double mb = Math.Round(si / 1024 / 1024, 2);
                        textBox4.Text = mb + " MB" + " " + "(" + si + " Byte)";
                    }
                    try
                    {
                        //FWcalc
                        local.BaseStream.Position = 0x26;
                        byte[] cal = local.ReadBytes(0x2);
                        string calc = BitConverter.ToString(cal);
                        calc = calc.Replace("-", "");
                        string offset = (Convert.ToInt32(calc) - 60 + 9).ToString();
                        int off = Convert.ToInt32(offset, 16);

                        //FW
                        local.BaseStream.Position = off;
                        byte[] fwbyte = local.ReadBytes(0x2);
                        string fwby = BitConverter.ToString(fwbyte);
                        fwby = fwby.Replace("-", ".");
                        textBox5.Text = fwby;
                    }
                    catch { }
                    //SHA1
                    long shaoffset = Convert.ToInt64(si) - 32;
                    local.BaseStream.Position = shaoffset;
                    byte[] sha = local.ReadBytes(20);
                    shavalue = BitConverter.ToString(sha).ToLower();
                    shavalue = shavalue.Replace("-", "");
                    textBox7.Text = shavalue;

                    local.Close();
                
            }
            else { MessageBox.Show("Wrong URL or file!"); }
        }
        //Drag & Drop
        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            textBox1.Text = null;
            string[] filename =(string[]) e.Data.GetData(DataFormats.FileDrop);
            if (filename[0].Contains(".pkg"))
            {
                textBox1.Text = filename[0];
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                pictureBox2.Image = null;
            }
            else
                MessageBox.Show("File not supported");
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(shavalue);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
                string filePath = textBox1.Text;

                byte[] buffer;
                int bytesRead;
                long size;
                long totalBytesRead = 0;

                using (Stream file = File.OpenRead(filePath))
                {
                    size = file.Length -32 ;
                   

                    using (HashAlgorithm hasher = SHA1.Create())
                    {
                        do
                        {
                            buffer = new byte[4096];
                            if (totalBytesRead + 4096 > size)
                            {
                                int last = (int)(size - totalBytesRead);
                                buffer = new byte[last];
                            }
                            bytesRead = file.Read(buffer, 0, buffer.Length);

                            totalBytesRead += bytesRead;

                            hasher.TransformBlock(buffer, 0, bytesRead, null, 0);

                            backgroundWorker1.ReportProgress((int)((double)totalBytesRead / size * 100));
                        }
                        while (totalBytesRead < size);

                        hasher.TransformFinalBlock(buffer, 0, 0);

                        e.Result = MakeHashString(hasher.Hash);
                    }
                }
        }
        private static string MakeHashString(byte[] hashBytes)
        {
            StringBuilder hash = new StringBuilder(32);

            foreach (byte b in hashBytes)
                hash.Append(b.ToString("X2").ToLower());

            return hash.ToString();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
             progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button2.Enabled = false;
            progressBar1.Value = 0;
            try
            {
                if (textBox7.Text == e.Result.ToString())
                {
                    MessageBox.Show("Package is valid!", "Validating");
                    pictureBox2.Image = psnstuff.Properties.Resources.gruener_haken;
                }
                else
                {
                    MessageBox.Show("Package is corrupt!", "Validating");
                    pictureBox2.Image = psnstuff.Properties.Resources.rotesX;
                }
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                pictureBox2.Image = null;
            }
        }    
    }
}
