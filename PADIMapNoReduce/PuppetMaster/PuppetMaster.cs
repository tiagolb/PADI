using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading.Tasks;
using PADIMapNoReduce;
using InterfacePMNR;
using UserPMNR;
using System.Windows.Forms;
using System.Threading;
using WorkerPMNR;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace PuppetMasterPMNR {
    public class PuppetMaster {

        private PuppetMasterForm puppetMasterForm;
        private int lastWorkerPort; 
        private IList<KeyValuePair<int, string>> workplace;
        private IList<string> puppetMasters;
        private int port;  //Port where PM is running
        private RemotePuppetMaster remotePuppetMaster;

        public PuppetMaster(PuppetMasterForm form) {
            this.puppetMasterForm = form;
            this.puppetMasters = new List<string>();
            this.workplace = new List<KeyValuePair<int, string>>();
            this.lastWorkerPort = 30001; //Hardcoded port
            this.port = 20001; //Hardcoded port

            TcpChannel channel = new TcpChannel(port);   //Hardcoded port
            ChannelServices.RegisterChannel(channel, false);
            remotePuppetMaster = new RemotePuppetMaster(this);
            RemotingServices.Marshal(remotePuppetMaster, "PM", typeof(RemotePuppetMasterInterface));
        }


        public int GetLastPort() {
            return lastWorkerPort;
        }

        public string GetURL() {
            string host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            string url = "tcp://" + host + ":" + 20001 + "/PM";
            return url;
        }

        public void SetLastPort(int lastPort) {
            this.lastWorkerPort = lastPort;
        }

        public void AddPuppetMaster(string newPuppetMasterURL) {
            puppetMasters.Add(newPuppetMasterURL);
        }


        public void WORKER(int id, string puppetMasterURL, string serviceURL, string entryURL) {
            if (puppetMasterURL.Equals(GetURL())) {

                string[] args = { id.ToString(), serviceURL, entryURL };

                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.RedirectStandardInput = false;
                p.StartInfo.FileName = "Z:\\Documents\\GitHub\\PADI\\PADIMapNoReduce\\WorkerPMNR\\bin\\Debug\\WorkerPMNR.exe";
                p.StartInfo.Arguments = String.Join(" ", args);
                p.Start();

                ReceiveNewWorker(id, serviceURL);  //Adds workerID , URL pair
                BroadcastNewWorker(id, serviceURL); //Advertises workerID , URL pair

            }
            else {
                RemotePuppetMasterInterface pm = (RemotePuppetMasterInterface)Activator.GetObject(typeof(RemotePuppetMasterInterface), puppetMasterURL);
                pm.CreateWorker(id, serviceURL, entryURL);
            }
        }


        public void SUBMIT(string entryURL, string filePath, string outputFolderPath, string nSplits, string dllFilePath, string mapClassName) { 
            //Creates user application in local node. The application submits the designated job
            puppetMasterForm.BeginInvoke((Action)delegate {
                UserGUIForm userGUI = new UserGUIForm(entryURL, filePath, outputFolderPath, nSplits, dllFilePath, mapClassName);
                userGUI.Show();
              //  userGUI.SubmitJob();
            });

        }

        

        public void WAIT(int seconds) { 
            //PuppetMaster stops its script execution for x seconds
            System.Threading.Thread.Sleep(1000*seconds);
        }

        public void STATUS() { 
            //Makes all workers print their status        
        }
        

        public void SLOWW(int id, int secondsDelay) {
            string serviceURL = null;
            foreach (KeyValuePair<int, string> k in workplace)
                if (k.Key == id) {
                    serviceURL = k.Value;
                    break;
                }
            if (serviceURL == null) {
                MessageBox.Show("There is no such worker");
                return;
            }

            RemoteWorkerInterface w = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), serviceURL);
            w.Slow(secondsDelay);
        }

        public void FREEZEW(int id){
            //Disables communication of worker id
        }

        public void UNFREEZEW(int id) { 
            //Enables communication of worker id
        }

        public void FREEZEC(int id) { 
            //Disables communication of jobtracker in worker id
        }

        public void UNFREEZEC(int id) { 
            //Enables communication of jobtracker in worker id
        }

        public void ReceiveNewWorker(int id, string serviceURL){
            workplace.Add(new KeyValuePair<int,string>(id,serviceURL));
        }

        public void BroadcastNewWorker(int workerID, string serviceURL) {

            foreach (string rpm in puppetMasters) {
                RemotePuppetMasterInterface pm = (RemotePuppetMasterInterface)Activator.GetObject(typeof(RemotePuppetMasterInterface), rpm);
                pm.ReceiveWorker(workerID, serviceURL);
            }
        }
    }


    public class RemotePuppetMaster : MarshalByRefObject, RemotePuppetMasterInterface {

        private PuppetMaster puppetMaster;

        public RemotePuppetMaster(PuppetMaster pm) {
            this.puppetMaster = pm;
        }

        public void CreateWorker(int id, string serviceURL, string entryURL) {
            string[] args = { id.ToString(), serviceURL, entryURL };

            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.FileName = "Z:\\Documents\\GitHub\\PADI\\PADIMapNoReduce\\WorkerPMNR\\bin\\Debug\\WorkerPMNR.exe";
            p.StartInfo.Arguments = String.Join(" ", args);
            p.Start();
            puppetMaster.ReceiveNewWorker(id, serviceURL); //Adds workerID , URL pair
            puppetMaster.BroadcastNewWorker(id, serviceURL); //Advertises workerID , URL pair
        }

        public int Connect(string newPuppetMasterURL) {
            puppetMaster.AddPuppetMaster(newPuppetMasterURL);
            return puppetMaster.GetLastPort();
        }

        public void ReceiveWorker(int workerID, string serviceURL) {
            puppetMaster.ReceiveNewWorker(workerID, serviceURL);
        }

    }
}
