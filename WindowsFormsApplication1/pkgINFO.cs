using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace psnstuff
{
    public partial class pkgINFO : Form
    {
        public pkgINFO(string url, string png)
        {
            InitializeComponent();
            textBox1.Text = url;
            pictureBox1.ImageLocation = png;
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
                            string sha1 = BitConverter.ToString(buffer2);
                            sha1 = sha1.Replace("-", "");
                            textBox7.Text = sha1;
                        }
                    }
                }
                else { MessageBox.Show("Link missing or wrong"); }
            }
            catch { }
 

        }

           
    }
}
