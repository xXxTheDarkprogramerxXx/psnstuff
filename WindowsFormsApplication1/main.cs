using psnstuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Net;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Xml;

namespace WindowsFormsApplication1
{
    public partial class psnStuff : Form
    {
        DataTable dt = new DataTable();
        public psnStuff()
        {

            InitializeComponent();
            pictureBox6.AllowDrop = true;

            Thread updat = new Thread(updatet);
            updat.Start();

            //update db
            try
            {
                if (!File.Exists("database"))
                {
                    WebClient update = new WebClient();
                    update.DownloadFile(new Uri("http://sin.spdns.eu/loozers/d/db"), @"database");
                    updateico.ShowBalloonTip(2000, "Download", "Database downloaded successfully!", ToolTipIcon.Info);
                    FileInfo downf = new FileInfo(@"database");
                    downf.Attributes = FileAttributes.Hidden;
                }
                else
                {
                    FileInfo sourceFile = new FileInfo(@"database");
                    sourceFile.Attributes = FileAttributes.Normal;
                    var request = (HttpWebRequest)WebRequest.Create(@"http://sin.spdns.eu/loozers/d/db");
                    request.Method = "HEAD";
                    request.Timeout = 5000;
                    var response = (HttpWebResponse)request.GetResponse();
                    sourceFile.Attributes = FileAttributes.Hidden;



                    if (response.LastModified > sourceFile.LastWriteTime)
                    {

                        WebClient update = new WebClient();
                        sourceFile.Attributes = FileAttributes.Normal;
                        update.DownloadFile(new Uri("http://sin.spdns.eu/loozers/d/db"), @"database");
                        sourceFile.Attributes = FileAttributes.Hidden;

                        updateico.ShowBalloonTip(2000, "Update", "Database updated successfully!", ToolTipIcon.Info);

                    }
                }

            }
            catch { }

            try
            {
                var n = new WebClient().DownloadString("http://sin.spdns.eu/loozers/n/news.txt");
                newstxt.Text = Convert.ToString(n);
            }
            catch { }

            //csv read line by line and add to table
            dt.Columns.Add("ID");
            dt.Columns.Add("Name");
            dt.Columns.Add("Typ");
            dt.Columns.Add("Region");
            dt.Columns.Add("psnlink");
            dt.Columns.Add("pnglink");
            dt.Columns.Add("rapname");
            dt.Columns.Add("rapdata");
            dt.Columns.Add("info");
            dt.Columns.Add("postedby");

            try
            {

                FileInfo sourceFile = new FileInfo(@"database");
                sourceFile.Attributes = FileAttributes.Normal;
                string line;
                string[] linesplit;


                byte[] db = File.ReadAllBytes(@"database");

               // byte[] db = Decompress(dbpack);

                string getstr = Encoding.GetEncoding(1252).GetString(db);
                StringReader file = new StringReader(getstr);

                while ((line = file.ReadLine()) != null)
                {
                    linesplit = line.Split(';');
                    dt.Rows.Add(linesplit);
                    count.Text = dt.Rows.Count.ToString();

                }

                dataGridView1.DataSource = dt;
                dataGridView1.Columns[0].Width = 100;
                dataGridView1.Columns[1].Width = 352;
                dataGridView1.Columns[2].Width = 72;
                dataGridView1.Columns[3].Width = 60;
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[8].Visible = false;
                dataGridView1.Columns[9].Visible = false;

                sourceFile.Attributes = FileAttributes.Hidden;
                file.Close();
            }
            catch
            {
                MessageBox.Show("Could not load Database", "Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FileInfo sourceFile = new FileInfo(@"database");
                sourceFile.Attributes = FileAttributes.Hidden;
            }

            //set "All" for combobox
            comboBox1.SelectedIndex = comboBox1.FindStringExact("All");

        }


        private string DownloadDir()
        {
            string thedir = string.Empty;
            if (psnstuff.Properties.Settings.Default.DownloadDirectory != string.Empty)
            {
                thedir = psnstuff.Properties.Settings.Default.DownloadDirectory.ToString();
            }
            else
            {
                thedir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PSNStuff\\Downloads";
                psnstuff.Properties.Settings.Default.DownloadDirectory = textBox1.Text;
                psnstuff.Properties.Settings.Default.Save();
            }

            if (!Directory.Exists(thedir))
            {
                Directory.CreateDirectory(thedir);
            }
            return thedir;

        }

        //icon & info & rapbutton & change
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount == 0) return;
            richTextBox1.Text = dataGridView1.CurrentRow.Cells["info"].Value.ToString();
            size.Text = "Size";
            pictureBox1.ImageLocation = dataGridView1.CurrentRow.Cells["pnglink"].Value.ToString();
            pictureBox1.Image = pictureBox1.InitialImage;
            richTextBox2.Text = dataGridView1.CurrentRow.Cells["postedby"].Value.ToString();
            textBox2.Text = dataGridView1.CurrentRow.Cells["ID"].Value.ToString();
            if (richTextBox2.Text == "")
            { richTextBox2.Text = "anonymous"; }

            //rapbutton
            string rapbutton = dataGridView1.CurrentRow.Cells["rapdata"].Value.ToString();
            if (rapbutton == " " || rapbutton == "")
            {
                button2.Enabled = false;
            }
        }

        //Download
        WebClient webClient;
        Stopwatch sw = new Stopwatch();
        string filename;
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;

            /* so lets add some user friendlyniss to this app */



            filename = DownloadDir()+"\\" + dataGridView1.CurrentRow.Cells["ID"].Value.ToString() + " " + dataGridView1.CurrentRow.Cells["name"].Value.ToString() + ".pkg";
            if (button1.Text == "Download Package")
            {
                if (File.Exists(filename))
                {
                    DialogResult result = MessageBox.Show("Download again?", "Already downloaded!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.OK)
                    {
                        goto down;
                    }
                    else
                    {
                        return;
                    }
                }

            down:
                if (!Directory.Exists("pkg"))
                {
                    Directory.CreateDirectory("pkg");
                }
                button1.Text = "Cancel";
                webClient = new WebClient();

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // start stopwatch to calc downloadspeed
                sw.Start();

                string link = dataGridView1.CurrentRow.Cells["psnlink"].Value.ToString();
                string URL;
                // Is "http://"
                if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    URL = ("http://" + link);
                }
                else
                {
                    URL = link;
                }
                // start download
                webClient.DownloadFileAsync(new Uri(URL), filename);
                downl.Text = dataGridView1.CurrentRow.Cells["name"].Value.ToString();
                label16.Text = "Downloading:";
            }
            else
            {
                webClient.CancelAsync();
                button1.Text = "Download Package";
            }

        }


        // Progressbar
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                double actkb = (e.TotalBytesToReceive - e.BytesReceived) / 1024;
                double speed = e.BytesReceived / 1024 / sw.Elapsed.TotalSeconds;
                double time = actkb / speed;
                // Download Speed
                if (prozent.Text != (Convert.ToDouble(e.BytesReceived) / 1024 / sw.Elapsed.TotalSeconds).ToString("0"))
                    speed1.Text = (Convert.ToDouble(e.BytesReceived) / 1024 / sw.Elapsed.TotalSeconds).ToString("0.00") + " kb/s";

                // procentual output
                if (progressBar1.Value != e.ProgressPercentage)
                    progressBar1.Value = e.ProgressPercentage;

                // procentual output
                if (prozent.Text != e.ProgressPercentage.ToString() + "%")
                    prozent.Text = e.ProgressPercentage.ToString() + "%";

                // downloaded / total size
                downl1.Text = (Convert.ToDouble(e.BytesReceived) / 1024 / 1024).ToString("0.00") + " MB" + " / " + (Convert.ToDouble(e.TotalBytesToReceive) / 1024 / 1024).ToString("0.00") + " MB";

            }
            catch { }
        }



        // if webclient finish...msgbox
        string etag;
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            string dname = dataGridView1.CurrentRow.Cells["name"].Value.ToString();

            sw.Reset();
            if (e.Cancelled == true)
            {
                File.Delete("pkg/" + dname + ".pkg");       // remove unfinished download
                button1.Text = "Download Package";
                label16.Text = "Canceled:";
                progressBar1.Value = 0;
                prozent.Text = "";
                downl1.Text = "";
                speed1.Text = "";

            }
            else
            {   //complete
                MessageBox.Show("Download completed!");

                button1.Text = "Download Package";
                label16.Text = "Downloaded:";
                progressBar1.Value = 0;
                prozent.Text = "";
                downl1.Text = "";
                speed1.Text = "";

                //validate
                try
                {
                    string kukik = webClient.ResponseHeaders["ETag"];
                    kukik = kukik.Replace("\"", "");
                    string[] kakik = kukik.Split(':');
                    etag = kakik[0];
                }
                catch
                {
                    MessageBox.Show("Validation not possible!");
                    return;
                }
                DialogResult result = MessageBox.Show("Validate Package?", "Validation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.OK)
                {
                    label16.Text = "Validating:";
                    backgroundWorker1.RunWorkerAsync(filename);
                }
            }
        }

        //md5 validation background
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string filePath = e.Argument.ToString();

            byte[] buffer;
            int bytesRead;
            long size;
            long totalBytesRead = 0;

            using (System.IO.Stream file = File.OpenRead(filePath))
            {
                size = file.Length;

                using (HashAlgorithm hasher = MD5.Create())
                {
                    do
                    {
                        buffer = new byte[4096];

                        bytesRead = file.Read(buffer, 0, buffer.Length);

                        totalBytesRead += bytesRead;

                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);

                        backgroundWorker1.ReportProgress((int)((double)totalBytesRead / size * 100));
                    }
                    while (bytesRead != 0);

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


            progressBar1.Value = 0;
            label16.Text = "Validated:";
            if (etag == e.Result.ToString())
            {
                MessageBox.Show("Package is valid!", "Validating");
            }
            else
            {
                MessageBox.Show("Package is corrupt!", "Validating");
            }


        }

        //rap string to byte
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        //create rap
        private void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("exdata"))
            {
                Directory.CreateDirectory("exdata");
            }
            try
            {
                string rap = dataGridView1.CurrentRow.Cells["rapname"].Value.ToString();
                string rapdata = dataGridView1.CurrentRow.Cells["rapdata"].Value.ToString();

                File.WriteAllBytes("exdata/" + rap, StringToByteArray(rapdata));
                MessageBox.Show("Rap has been saved to exdata!", "rap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { MessageBox.Show("No rap available!"); }

        }
        //submit
        private void submit_Click(object sender, EventArgs e)
        {
            try
            {
                if (addpsn.Text.Contains(".pkg") && addpsn.Text.Contains("zeus") || addpsn.Text.Contains("ares"))
                {
                    goto Send;
                }

                MessageBox.Show("Link is missing or wrong");
                goto End;
            Send:

                string url = "http://sin.spdns.eu/loozers/s/submit.php";


                WebClient wclient = new WebClient();
                {
                    NameValueCollection postData = new NameValueCollection() 
                 {
             
                 { "titleid", addtitle.Text },  //order: {"parameter name", "parameter value"}
                 { "name", addname.Text },
                 { "typ", combotyp.Text },
                 { "reg", comboreg.Text },
                 { "psn", addpsn.Text },
                 { "png", addpng.Text },
                 { "rap", addrap.Text },
                 { "rapd", addrapdata.Text },
                 { "info", addinfo.Text },
                 { "postedby", postedby.Text }
                 };

                    wclient.Encoding = Encoding.UTF8;
                    byte[] responseArray = wclient.UploadValues(url, "POST", postData);
                    MessageBox.Show(Encoding.ASCII.GetString(responseArray), "Upload finished!");
                }
            End: ;
            }
            catch { MessageBox.Show("Connection failed. Please try again later."); }
        }


        //drag effect
        private void dragdrop_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        //drag n drop
        private void dragdrop_DragDrop(object sender, DragEventArgs e)
        {

            string[] test = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in test)
            {

                FileInfo filen = new FileInfo(file);
                if (filen.Extension == ".rap")
                {
                    addrap.Text = filen.Name;

                    BinaryReader br = new BinaryReader(File.OpenRead(file));
                    byte[] hex = File.ReadAllBytes(file);
                    string rephex = BitConverter.ToString(hex);
                    string stringhex = rephex.Replace("-", "");
                    addrapdata.Text = stringhex;
                }
                else
                {
                    MessageBox.Show("This is no rap!");
                }


            }

        }
        //Pkg get size
        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            string sizet;
            string newurllink;
            string urllink = dataGridView1.CurrentRow.Cells["psnlink"].Value.ToString();
            if (!urllink.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                newurllink = ("http://" + urllink);
            }
            else
            {
                newurllink = urllink;
            }

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(newurllink);
                request.Method = "HEAD";
                request.Timeout = 3000;
                var response = (HttpWebResponse)request.GetResponse();
                double length = response.ContentLength;

                if (length > 1073741287)
                {
                    sizet = Math.Round(length / 1024 / 1024 / 1024, 2).ToString() + " GB";
                    size.Text = sizet;
                }
                if (length <= 1073741287 & length >= 1048576)
                {
                    sizet = Math.Round(length / 1024 / 1024, 2).ToString() + " MB";
                    size.Text = sizet;
                }
                if (length < 1048576)
                {
                    sizet = Math.Round(length / 1024, 2).ToString() + " KB";
                    size.Text = sizet;
                }
            }
            catch (WebException)
            {
                size.Text = "unknown";
            }


        }
        //copy to clipboard
        //private void listView1_MouseClick(object sender, MouseEventArgs e)
        //{
        //if (Control.ModifierKeys == Keys.Alt && e.Button == MouseButtons.Right)
        //{
        //    Clipboard.SetText(listView1.SelectedItems[0].SubItems[4].Text);

        //}

        //}

        //Toolupdate
        static void updatet()
        {
            Thread.Sleep(6000);
            try
            {
                string change = new WebClient().DownloadString("http://sin.spdns.eu/loozers/u/upd.txt");
                string strMeldung = "Do you want to download the new PSN Stuff v" + change + "?";

                decimal act = 2.01m;
                decimal chv = Convert.ToDecimal(change, CultureInfo.InvariantCulture);
                if (chv > act)
                {
                    DialogResult result = MessageBox.Show(strMeldung, "Update Available!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.OK)
                    {
                        WebClient download = new WebClient();
                        download.DownloadFile("http://sin.spdns.eu/loozers/u/update.php", "psnstuff_ver_" + change + ".rar");
                        MessageBox.Show("Download succesfull", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

            }

            catch { }
        }
        //Filter
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "All")
            {
                dt.DefaultView.RowFilter = "Name LIKE " + "'*" + textBox1.Text + "*'" + "OR ID LIKE " + "'*" + textBox1.Text + "*'";
                dataGridView1.DataSource = dt.DefaultView;
            }
            else
            {
                dt.DefaultView.RowFilter = "Typ=" + "'" + comboBox1.Text + "'" + "AND (Name LIKE " + "'%" + textBox1.Text + "%'" + "OR ID LIKE " + "'*" + textBox1.Text + "*')";
                dataGridView1.DataSource = dt.DefaultView;
            }
            count.Text = dataGridView1.RowCount.ToString();

        }
        //Search
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "All")
            {
                dt.DefaultView.RowFilter = "Name LIKE " + "'*" + textBox1.Text + "*'" + "OR ID LIKE " + "'*" + textBox1.Text + "*'";
                dataGridView1.DataSource = dt.DefaultView;
            }
            else
            {
                dt.DefaultView.RowFilter = "Typ=" + "'" + comboBox1.Text + "'" + "AND (Name LIKE " + "'%" + textBox1.Text + "%'" + "OR ID LIKE " + "'*" + textBox1.Text + "*')";
                dataGridView1.DataSource = dt.DefaultView;
            }
            count.Text = dataGridView1.RowCount.ToString();
        }

        //link clickable newstextbox
        private void newstxt_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void richTextBox2_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        // File split
        private void pictureBox6_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        string foldername;

        private void pictureBox6_DragDrop(object sender, DragEventArgs e)
        {


            string[] test = (string[])e.Data.GetData(DataFormats.FileDrop);

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK)
            {
                foldername = fbd.SelectedPath;
                backgroundWorker2.RunWorkerAsync(test);
            }
            else return;


        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] test = (string[])e.Argument;

            foreach (string file in test)
            {
                FileInfo fileinf = new FileInfo(file);
                long size = fileinf.Length;

                if (size > 4294967295)
                {
                    label18.Invoke(new Action(() =>
                    {
                        label18.Text = fileinf.Name;
                    }
                    ));
                    long chunkSize = 4294967294;
                    const uint BUFFER_SIZE = 20 * 1024;
                    byte[] buffer = new byte[BUFFER_SIZE];

                    using (System.IO.Stream input = File.OpenRead(file))
                    {
                        uint index = 0;
                        while (input.Position < input.Length)
                        {
                            using (System.IO.Stream output = File.Create(foldername + "\\" + fileinf.Name + ".6660" + index))
                            {
                                long totalbytesread = 0;
                                long remaining = chunkSize, bytesRead;
                                while (remaining > 0 && (bytesRead = input.Read(buffer, 0, Convert.ToInt32(Math.Min(remaining, BUFFER_SIZE)))) > 0)
                                {
                                    output.Write(buffer, 0, Convert.ToInt32(bytesRead));
                                    remaining -= bytesRead;
                                    totalbytesread += bytesRead;
                                    backgroundWorker2.ReportProgress((int)((double)input.Position / size * 100));
                                }

                                index++;

                            }

                        }
                    }
                }
                else { MessageBox.Show(fileinf.Name + "\r\n" + "Splitting is not needed!", "Split", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }

        }


        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar2.Value = 0;
            label18.Text = "";
            MessageBox.Show("Done", "Split", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //pkgID open
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            pkgID open = new pkgID();
            open.Show();
        }

        // Context menu
        private void copiePSNLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dataGridView1.CurrentRow.Cells["psnlink"].Value.ToString());
        }

        private void copieTitleIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dataGridView1.CurrentRow.Cells["ID"].Value.ToString());
        }

        private void copieNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dataGridView1.CurrentRow.Cells["Name"].Value.ToString());
        }

        private void showInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pkgINFO open = new pkgINFO(dataGridView1.CurrentRow.Cells["psnlink"].Value.ToString(), dataGridView1.CurrentRow.Cells["pnglink"].Value.ToString());
            open.Show();
        }


        //Game Updater

        private void button5_Click(object sender, EventArgs e)
        {
            listView2.Items.Clear();
            label23.Text = "";
            string xml;

            try
            {
                string id = textBox2.Text.ToUpper();
                string url = "https://a0.ww.np.dl.playstation.net/tpl/np/" + id + "/" + id + "-ver.xml";
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                WebClient updcon = new WebClient();
                updcon.Encoding = Encoding.UTF8;
                xml = updcon.DownloadString(url);

                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {

                    reader.ReadToFollowing("package");
                    while (reader.HasAttributes)
                    {
                        reader.MoveToFirstAttribute();
                        string version = reader.Value;

                        reader.MoveToNextAttribute();
                        string sizeb = reader.Value;
                        string size = null;

                        if (Convert.ToDouble(sizeb) >= 1048576)
                        {
                            size = (Convert.ToDouble(sizeb) / 1024 / 1024).ToString("0.00") + " MB";
                        }

                        else if (Convert.ToDouble(sizeb) >= 1024)
                        {
                            size = (Convert.ToDouble(sizeb) / 1024).ToString("0.00") + "KB";
                        }


                        reader.MoveToNextAttribute();
                        string sha1sum = reader.Value;

                        reader.MoveToNextAttribute();
                        string pkgurl = reader.Value;

                        reader.MoveToNextAttribute();
                        string fw = reader.Value;

                        ListViewItem item = new ListViewItem(version);
                        item.SubItems.Add(fw);
                        item.SubItems.Add(size);
                        item.SubItems.Add(sha1sum);
                        item.SubItems.Add(pkgurl);

                        item.Checked = true;
                        listView2.Items.Add(item);

                        listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        reader.ReadToFollowing("package");

                    }

                }
                using (XmlReader reader2 = XmlReader.Create(new StringReader(xml)))
                {
                    reader2.ReadToFollowing("TITLE");
                    label23.Text = reader2.ReadElementContentAsString();


                }
            }
            catch { MessageBox.Show("No Game/update found!", "Game Updater", MessageBoxButtons.OK, MessageBoxIcon.Information); }



        }

        WebClient webClient2;
        Stopwatch sw2 = new Stopwatch();
        string filename2;
        private void button4_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("update"))
            {
                Directory.CreateDirectory("update");
            }

            try
            {
                string url = listView2.SelectedItems[0].SubItems[4].Text;

                if (button4.Text == "Download")
                {


                    filename2 = url;
                    filename2 = filename2.Substring(84);

                    button4.Text = "Cancel";
                    webClient2 = new WebClient();

                    webClient2.DownloadFileCompleted += new AsyncCompletedEventHandler(Complete);
                    webClient2.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChan);
                    sw2.Start();
                    webClient2.DownloadFileAsync(new Uri(url), @"update\" + filename2);
                    label25.Text = "Downloading: " + filename2;
                }
                else
                {
                    webClient2.CancelAsync();
                    button4.Text = "Download";
                }
            }
            catch { }
        }

        private void ProgressChan(object sender, DownloadProgressChangedEventArgs e)
        {

            try
            {
                double actkb = (e.TotalBytesToReceive - e.BytesReceived) / 1024;
                double speed = e.BytesReceived / 1024 / sw2.Elapsed.TotalSeconds;
                double time = actkb / speed;
                // Download Speed
                if (prozent1.Text != (Convert.ToDouble(e.BytesReceived) / 1024 / sw2.Elapsed.TotalSeconds).ToString("0"))
                    speed2.Text = (Convert.ToDouble(e.BytesReceived) / 1024 / sw2.Elapsed.TotalSeconds).ToString("0.00") + " kb/s";

                // procentual output
                if (progressBar3.Value != e.ProgressPercentage)
                    progressBar3.Value = e.ProgressPercentage;

                // procentual output
                if (prozent1.Text != e.ProgressPercentage.ToString() + "%")
                    prozent1.Text = e.ProgressPercentage.ToString() + "%";

                // downloaded / total size
                downl2.Text = (Convert.ToDouble(e.BytesReceived) / 1024 / 1024).ToString("0.00") + " MB" + " / " + (Convert.ToDouble(e.TotalBytesToReceive) / 1024 / 1024).ToString("0.00") + " MB";

            }
            catch { }
        }

        private void Complete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                button4.Text = "Download";
                MessageBox.Show("Download cancelled!", "Game Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                sw2.Reset();
                progressBar3.Value = 0;
                prozent1.Text = "";
                speed2.Text = "";
                downl2.Text = "";
                label25.Text = "Cancelled: " + filename2;
            }
            else
            {
                button4.Text = "Download";
                MessageBox.Show("Download finished!", "Game Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                sw2.Reset();
                progressBar3.Value = 0;
                prozent1.Text = "";
                speed2.Text = "";
                downl2.Text = "";
                label25.Text = "Downloaded: " + filename2;
            }
        }

        private void copyPSNLinkToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView2.SelectedItems[0].SubItems[4].Text);
        }

        private void copySHA1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView2.SelectedItems[0].SubItems[3].Text);
        }

        private void copySizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView2.SelectedItems[0].SubItems[2].Text);
        }

        private void copyFWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView2.SelectedItems[0].SubItems[1].Text);
        }

        private void copyVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView2.SelectedItems[0].Text);
        }


        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabControl.TabPages["tabMetacritic"])
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            settingswin settings = new settingswin();
            settings.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //here we just copy everything to the usb sellected in either settings or in the combobopx


        }

        private void psnStuff_Load(object sender, EventArgs e)
        {
            //load the usb drives
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    comboBox2.Items.Add(drive.Name +"-"+ drive.VolumeLabel);
                }
            }
            if (comboBox2.Items.Count == 0)
            {
                comboBox2.Text = "No Removable Drives detected";
            }
        }

    }
}