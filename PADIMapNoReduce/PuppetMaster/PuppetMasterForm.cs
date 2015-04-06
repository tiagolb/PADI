using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppetMasterPMNR;

namespace PuppetMasterPMNR {
    public partial class PuppetMasterForm : Form {

        PuppetMaster puppetMaster = new PuppetMaster();

        public PuppetMasterForm() {
            InitializeComponent();
        }

        private void bt_script_Click(object sender, EventArgs e) {
            ofd_script.Filter = "TXT|*.txt";
            if (ofd_script.ShowDialog() == DialogResult.OK) {
                tb_scriptFileName.Text = ofd_script.SafeFileName;
                tb_scriptFileAddress.Text = ofd_script.FileName;
            }
        }

        private void bt_PuppetMasterURLSubmit_Click(object sender, EventArgs e) {

        }

        private void bt_submitScript_Click(object sender, EventArgs e) {

        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void bt_singleCommand_Click(object sender, EventArgs e) {
            char[] delimiter = { ' ' };

            string command = tb_singleCommand.Text;

            string[] words = command.Split(delimiter);

            switch (words[0]) {   
                case "WORKER":
                    puppetMaster.WORKER(Int32.Parse(words[1]), words[2], words[3], words[4]);
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


    }
}
