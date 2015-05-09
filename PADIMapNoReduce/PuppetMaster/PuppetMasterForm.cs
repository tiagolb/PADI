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
using System.Threading;

namespace PuppetMasterPMNR {
    public partial class PuppetMasterForm : Form {

        PuppetMaster puppetMaster;

        private string host;
        private StreamReader sr;
        private string s = String.Empty;


        public PuppetMasterForm() {
            InitializeComponent();

            //Fill textbox of PuppetMasterHost
            host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            tb_puppetHost.Text = host;
        }

        private void bt_script_Click(object sender, EventArgs e) {
            ofd_script.Filter = "TXT|*.txt";
            if (ofd_script.ShowDialog() == DialogResult.OK) {
                tb_scriptFileName.Text = ofd_script.SafeFileName;
                tb_scriptFileAddress.Text = ofd_script.FileName;
            }
        }

        private void bt_submitScript_Click(object sender, EventArgs e) {
            if (tb_scriptFileAddress.Text != "")
                sr = File.OpenText(tb_scriptFileAddress.Text); 
            else MessageBox.Show("Insert a file path for the script");
        }

        private void bt_singleCommand_Click(object sender, EventArgs e) {
            if (tb_singleCommand.Text != "") {
                string command = tb_singleCommand.Text;
                ProcessCommand(command);
            }
            else MessageBox.Show("Insert a command");
        }

        private void ProcessCommand(string command) {

            if (command.StartsWith("%"))
                return;

            char[] delimiter = { ' ' };

            string[] words = command.Split(delimiter);

            switch (words[0]) {
                case "WORKER":
                    if (words.Length == 5) {
                        puppetMaster.WORKER(Int32.Parse(words[1]), words[2], words[3], words[4]);
                    }
                    else {
                        puppetMaster.WORKER(Int32.Parse(words[1]), words[2], words[3], "NOENTRYPOINT");
                    }
                    return;
                case "SUBMIT":
                    Thread submitThread = new Thread(() => puppetMaster.SUBMIT(words[1], words[2], words[3], words[4], words[5], words[6]));
                    submitThread.Start();
                    submitThread = null;
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
            if (tb_configFileAddress.Text != "") {
                using (StreamReader sr = File.OpenText(tb_configFileAddress.Text)) {
                    string address = String.Empty;
                    while ((address = sr.ReadLine()) != null) {
                        Uri baseUri = new Uri(address);
                        if (baseUri.IsLoopback || baseUri.Host.Equals(host)) {
                            puppetMaster = new PuppetMaster(this, baseUri.Port);
                            tb_PuppetMasterURL.Text = "tcp://" + host + ":" + baseUri.Port + "/PM";
                            bt_openConfig.Enabled = false;
                            bt_submitConfig.Enabled = false;
                        }
                    }
                }


                using (StreamReader sr = File.OpenText(tb_configFileAddress.Text)) {
                    string address = String.Empty;
                    while ((address = sr.ReadLine()) != null) {
                        Uri baseUri = new Uri(address);
                        if (!baseUri.IsLoopback && !address.Equals(puppetMaster.GetURI())) {
                            puppetMaster.AddPuppetMaster(address);
                        }
                    }
                }

                bt_script.Enabled = true;
                bt_submitScript.Enabled = true;
                bt_singleCommand.Enabled = true;
                tb_scriptFileName.Enabled = true;
                tb_scriptFileAddress.Enabled = true;
                tb_singleCommand.Enabled = true;

            }
            else MessageBox.Show("Insert a file path for the script");
        }

        public void SetWorkers(IList<KeyValuePair<int, string>> workers, KeyValuePair<int, string> jobTracker) {
            lb_workers.Items.Clear();
            foreach (KeyValuePair<int, string> k in workers)
                lb_workers.Items.Add("Worker ID: " + k.Key + " | URI: " + k.Value);

            lb_workers.SelectedIndex = lb_workers.FindString("Worker ID: " + jobTracker.Key + " | URI: " + jobTracker.Value);
        }

        private void bt_status_Click(object sender, EventArgs e) {
            if(puppetMaster != null)
                puppetMaster.STATUS();
        }

        private void bt_runScript_Click(object sender, EventArgs e) {
            while ((s = sr.ReadLine()) != null) {
                ProcessCommand(s);
            }
        }

        private void bt_step_Click(object sender, EventArgs e) {
            if((s = sr.ReadLine()) != null) {
                ProcessCommand(s);
            }
        }
    }
}
