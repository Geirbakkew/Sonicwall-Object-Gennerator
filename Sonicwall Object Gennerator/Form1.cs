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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Security.Policy;
using System.Xml.Linq;
using Newtonsoft.Json;




namespace Sonicwall_Object_Gennerator
{
  

    public partial class Form1 : Form
    {
       

        public Form1()
        {
            InitializeComponent();
            string result = Path.GetTempPath();
            Console.WriteLine(result);

        }
     
        private async void btn_Download_Click(object sender, EventArgs e)
        {
            btn_Download.Enabled = false;
            lblWarning.Text = "Jobber!";
           
            using (WebClient wc = new WebClient())
    {
                
                await wc.DownloadFileTaskAsync(
                    // Param1 = Link of file
                     new System.Uri("https://download.microsoft.com/download/B/2/A/B2AB28E1-DAE1-44E8-A867-4987FE089EBE/msft-public-ips.csv"),
                    // Param2 = Path to save
                    Path.GetTempPath()+"msft-public-ips.csv"
                );
            }
            String line;
            try
            {
                

                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(Path.GetTempPath() + "msft-public-ips.csv");
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {

                    string[] TMP = line.Split(',').Select(str => str.Trim()).ToArray();
                    string[] IP_Subnett = TMP.First().Split('/');

                    IPAddress address;
                    if (IPAddress.TryParse(IP_Subnett.First(), out address))
                    {
                        switch (address.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:
                                // This is IPv4 address
                                richTextBox1.AppendText(Environment.NewLine + "address-object ipv4 " + "\"MS " + TMP.First() + "\"");
                                richTextBox1.AppendText(Environment.NewLine + "network " + IP_Subnett.First() + " " + getSubnetAddressFromIPNetMask(IP_Subnett.Last()));
                                richTextBox1.AppendText(Environment.NewLine + "zone WAN");
                                richTextBox1.AppendText(Environment.NewLine + "exit" + Environment.NewLine);

                                richTextBox2.AppendText(Environment.NewLine + "address-group ipv4 Microsoft_WAN");
                                richTextBox2.AppendText(Environment.NewLine + "address-object ipv4 " + "\"MS " + TMP.First() + "\"");
                                richTextBox2.AppendText(Environment.NewLine + "exit");
                                richTextBox2.AppendText(Environment.NewLine + "commit");

                                

                                //richTextBox1.AppendText(Environment.NewLine + "This is IPv4 address");
                                //richTextBox1.AppendText(Environment.NewLine + IP_Subnett.First());
                                //richTextBox1.AppendText(Environment.NewLine + getSubnetAddressFromIPNetMask(IP_Subnett.Last()));
                                break;
                            case System.Net.Sockets.AddressFamily.InterNetworkV6:
                                //richTextBox1.AppendText(Environment.NewLine + "This is IPv6 address");
                                //richTextBox1.AppendText(Environment.NewLine + IP_Subnett.First());
                                //richTextBox1.AppendText(Environment.NewLine + getSubnetAddressFromIPNetMask(IP_Subnett.Last()));

                                break;
                            default:
                                break;
                        }
                    }

                    
                    //Read the next line
                    line = sr.ReadLine();

                }
                //close the file
                sr.Close();
                Console.ReadLine();

            }
            catch (Exception f)
            {
                Console.WriteLine("Exception: " + f.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            richTextBox1.AppendText(Environment.NewLine + "commit");
            btn_Download.Enabled = true;
            lblWarning.Text = "Ferdig!";

        }
    

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            // Display a MsgBox asking the user to save changes or abort.
            if (MessageBox.Show("Do you want to delete downloaded file?", "Cleanup",
               MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // Call method to delete file...
                File.Delete(Path.GetTempPath() + "msft-public-ips.csv");

            }
            
        }

        private static bool IsIPAddress(string ipAddress)
        {
            bool retVal = false;

            try
            {
                IPAddress address;
                retVal = IPAddress.TryParse(ipAddress, out address);
            }
            catch (Exception ex)
            {
                
            }
            return retVal;
        }

      
        private void button1_Click(object sender, EventArgs e)
        {

            


            IPAddress address;
            if (IPAddress.TryParse("192.168.168.0", out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        // This is IPv4 address
                        richTextBox1.AppendText(Environment.NewLine + "This is IPv4 address");
                        break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        // This is IPv6 address
                        break;
                    default:
                        break;
                }
            }
        }



        //This Function is Used to get Subnet basede on NetMask(i.e 0-32)
        public string getSubnetAddressFromIPNetMask(string netMask)
        {
            string subNetMask = string.Empty;
            int calSubNet = 0;
            double result = 0;
            if (!string.IsNullOrEmpty(netMask))
            {
                calSubNet = 32 - Convert.ToInt32(netMask);
                if (calSubNet >= 0 && calSubNet <= 8)
                {
                    for (int ipower = 0; ipower < calSubNet; ipower++)
                    {
                        result += Math.Pow(2, ipower);
                    }
                    double finalSubnet = 255 - result;
                    subNetMask = "255.255.255." + Convert.ToString(finalSubnet);
                }
                else if (calSubNet > 8 && calSubNet <= 16)
                {
                    int secOctet = 16 - calSubNet;

                    secOctet = 8 - secOctet;

                    for (int ipower = 0; ipower < secOctet; ipower++)
                    {
                        result += Math.Pow(2, ipower);
                    }
                    double finalSubnet = 255 - result;
                    subNetMask = "255.255." + Convert.ToString(finalSubnet) + ".0";
                }
                else if (calSubNet > 16 && calSubNet <= 24)
                {
                    int thirdOctet = 24 - calSubNet;

                    thirdOctet = 8 - thirdOctet;

                    for (int ipower = 0; ipower < thirdOctet; ipower++)
                    {
                        result += Math.Pow(2, ipower);
                    }
                    double finalSubnet = 255 - result;
                    subNetMask = "255." + Convert.ToString(finalSubnet) + ".0.0";
                }
                else if (calSubNet > 24 && calSubNet <= 32)
                {
                    int fourthOctet = 32 - calSubNet;

                    fourthOctet = 8 - fourthOctet;

                    for (int ipower = 0; ipower < fourthOctet; ipower++)
                    {
                        result += Math.Pow(2, ipower);
                    }
                    double finalSubnet = 255 - result;
                    subNetMask = Convert.ToString(finalSubnet) + ".0.0.0";
                }
            }

            return subNetMask;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btn_Json_Click(object sender, EventArgs e)
        {

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://endpoints.office.com/endpoints/Worldwide?ClientRequestId=b10c5ed1-bad1-445f-b386-b919946339a7");
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json; charset=utf-8";
            string file;
            var response = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                file = sr.ReadToEnd();
            }


            //var table = JsonConvert.DeserializeAnonymousType(file, new { Makes = default(DataTable) }).Makes;
           
            //var table = JsonConvert.DeserializeAnonymousType(file, new { Makes = default(DataTable) }).Makes;
            

            /*
            if (table.Rows.Count > 0)
            {
                //do something 

                MessageBox.Show(Convert.ToString(table.Rows[0][0]));
            }
            */
        }
    }
    
}

