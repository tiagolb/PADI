using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster {
    public partial class PuppetMasterForm : Form {
        public PuppetMasterForm() {
            InitializeComponent();
        }

        private void bt_script_Click(object sender, EventArgs e) {
            ofd_script.Filter = "TXT|*.txt" ;
            if (ofd_script.ShowDialog() == DialogResult.OK) {
                tb_scriptFileName.Text = ofd_script.SafeFileName;
                tb_scriptFileAddress.Text = ofd_script.FileName;
            }
        }

        private void bt_PuppetMasterURLSubmit_Click(object sender, EventArgs e) {

        }

        private void bt_submitScript_Click(object sender, EventArgs e) {

        }
    }
}
