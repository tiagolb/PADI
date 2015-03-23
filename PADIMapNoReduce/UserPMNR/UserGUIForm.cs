using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserPMNR
{
    public partial class UserGUIForm : Form
    {
        public UserGUIForm()
        {
            InitializeComponent();
        }

        private void bt_inputFile_Click(object sender, EventArgs e)
        {
            if (ofd_inputFile.ShowDialog() == DialogResult.OK)
            {
                tb_safeInputFile.Text = ofd_inputFile.SafeFileName;
                tb_absInputFile.Text = ofd_inputFile.FileName;
            }
        }

        private void bt_entryURL_Click(object sender, EventArgs e)
        {
            bt_entryURL.Enabled = false;
            tb_entryURL.ReadOnly = true;
            bt_submitJob.Enabled = true;
        }

        private void bt_dllFile_Click(object sender, EventArgs e)
        {
            ofd_inputDll.Filter = "DLL|*.dll";
            if (ofd_inputDll.ShowDialog() == DialogResult.OK)
            {
                tb_safeDllFile.Text = ofd_inputDll.SafeFileName;
                tb_absDllFile.Text = ofd_inputDll.FileName;
            }
        }

        private void bt_outputFolder_Click(object sender, EventArgs e)
        {
            if (fbd_outputFolder.ShowDialog() == DialogResult.OK)
            {
                tb_absOutputFolder.Text = fbd_outputFolder.SelectedPath;
            }
        }
    }
}
