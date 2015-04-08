using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppetMasterPMNR;
using System.Net;
using System.Net.Sockets;

namespace PuppetMasterPMNR {
    public partial class PuppetMasterForm : Form {

        PuppetMaster puppetMaster;

        public PuppetMasterForm() {
            InitializeComponent();
            puppetMaster = new PuppetMaster(this);
            
            //Fill textbox of PuppetMasterServiceURL
            string host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            string url = "tcp://" + host + ":" + 20001 + "/PM";
            tb_PuppetMasterURL.Text = url; 
        }

        private void bt_script_Click(object sender, EventArgs e) {
            ofd_script.Filter = "TXT|*.txt";
            if (ofd_script.ShowDialog() == DialogResult.OK) {
                tb_scriptFileName.Text = ofd_script.SafeFileName;
                tb_scriptFileAddress.Text = ofd_script.FileName;
            }
        }

        private void bt_submitScript_Click(object sender, EventArgs e) {
            using (StreamReader sr = File.OpenText(tb_scriptFileAddress.Text)) {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null) {
                    ProcessCommand(s);
                }
            }
        }

        private void bt_singleCommand_Click(object sender, EventArgs e) {
            string command = tb_singleCommand.Text;
            ProcessCommand(command);
        }

        private void ProcessCommand(string command) {

            if(command.StartsWith("%"))
                return;

              char[] delimiter = { ' ' };

            string[] words = command.Split(delimiter);

            switch (words[0]) {   
                case "WORKER":
                    if(words.Length == 5)
                        puppetMaster.WORKER(Int32.Parse(words[1]), words[2], words[3], words[4]);
                    else
                        puppetMaster.WORKER(Int32.Parse(words[1]), words[2], words[3], "NOENTRYPOINT");
                    return;
                case "SUBMIT":
                    puppetMaster.SUBMIT(words[1], words[2], words[3], words[4], words[5], words[6]);
                    return;
                case "WAIT":
                    puppetMaster.WAIT(Int32.Parse(words[1]));
                    return;
                case "STATUS":
                    puppetMaster.STATUS();
                    return;
                case "SLOWW":
                    puppetMaster.SLOWW(Int32.Parse(words[1]), Int32.Parse(words[2]));
                    return;
                case "FREEZEW":
                    puppetMaster.FREEZEW(Int32.Parse(words[1]));
                    return;
                case "UNFREEZEW":
                    puppetMaster.UNFREEZEW(Int32.Parse(words[1]));
                    return;
                case "FREEZEC":
                    puppetMaster.FREEZEC(Int32.Parse(words[1]));
                    return;
                case "UNFREEZEC":
                    puppetMaster.UNFREEZEC(Int32.Parse(words[1]));
                    return;
                default:
                    return;
            }
        }

        private void bt_openConfig_Click(object sender, EventArgs e) {
            ofd_config.Filter = "TXT|*.txt";
            if (ofd_config.ShowDialog() == DialogResult.OK) {
                tb_configFileName.Text = ofd_config.SafeFileName;
                tb_configFileAddress.Text = ofd_config.FileName;
            }
        }

        private void bt_submitConfig_Click(object sender, EventArgs e) {
            using (StreamReader sr = File.OpenText(tb_configFileAddress.Text)) {
                string address = String.Empty;
                while ((address = sr.ReadLine()) != null) {
                    Uri baseUri = new Uri(address);
                    if (!baseUri.IsLoopback && !address.Equals(puppetMaster.GetURL()))
                        puppetMaster.AddPuppetMaster(address);
                }
            }
        }


    }
}
