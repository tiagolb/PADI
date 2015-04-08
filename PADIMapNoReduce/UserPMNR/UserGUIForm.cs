using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClientPMNR;

namespace UserPMNR {
    public partial class UserGUIForm : Form {

        private Client _client;
        private string _entryURL;
        private string _inputFilePath;
        private string _dllFilePath;
        private string _outputFolderPath;
        private string _mapClassName;
        private int _numberSplits;

        public UserGUIForm() {
            InitializeComponent();
        }


        public UserGUIForm(string entryURL, string filePath, string outputFolderPath, string nSplits, string mapClassName, string dllFilePath) {
            InitializeComponent();
            this._entryURL = entryURL;
            this._inputFilePath = filePath;
            this._outputFolderPath = outputFolderPath;
            this._dllFilePath = dllFilePath;
            this._mapClassName = mapClassName;
            this._numberSplits = Int32.Parse(nSplits);
        }
 

        private void bt_entryURL_Click(object sender, EventArgs e) {
            string entryURL = tb_entryURL.Text;
            if (CheckURLValid(entryURL.Trim())) {
                bt_entryURL.Enabled = false;
                tb_entryURL.ReadOnly = true;
                bt_submitJob.Enabled = true;
                _client = new Client();
                _client.INIT(entryURL);
            } else {
                MessageBox.Show("Insert a valid Entry URL!");
            }
        }

        private void bt_inputFile_Click(object sender, EventArgs e) {
            if (ofd_inputFile.ShowDialog() == DialogResult.OK) {
                tb_safeInputFile.Text = ofd_inputFile.SafeFileName;
                tb_absInputFile.Text = _inputFilePath = ofd_inputFile.FileName;
            }
        }

        private void bt_dllFile_Click(object sender, EventArgs e) {
            ofd_inputDll.Filter = "DLL|*.dll";
            if (ofd_inputDll.ShowDialog() == DialogResult.OK) {
                tb_safeDllFile.Text = ofd_inputDll.SafeFileName;
                tb_absDllFile.Text = _dllFilePath = ofd_inputDll.FileName;
            }
        }

        private void bt_outputFolder_Click(object sender, EventArgs e) {
            if (fbd_outputFolder.ShowDialog() == DialogResult.OK) {
                tb_absOutputFolder.Text = _outputFolderPath = fbd_outputFolder.SelectedPath;
            }
        }

        // Useful methods
        public static bool CheckURLValid(string source) {
            //return Uri.IsWellFormedUriString(source, UriKind.RelativeOrAbsolute);
            string regular = @"^(tcp)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$";
            return Regex.IsMatch(source, regular);
        }

        private void bt_submitJob_Click(object sender, EventArgs e) {
            _numberSplits = Int32.Parse(tb_splits.Text);
            _mapClassName = tb_mapClass.Text;
            lb_bytes.Text += _client.SUBMIT(_inputFilePath, _numberSplits, _outputFolderPath, _dllFilePath, _mapClassName);
        }

        public void SubmitJob() {
            this._client = new Client();
            this._client.INIT(_entryURL);
            lb_bytes.Text += _client.SUBMIT(_inputFilePath, _numberSplits, _outputFolderPath, _mapClassName, _dllFilePath);
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }
    }
}
