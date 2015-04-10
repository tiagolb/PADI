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
using System.IO;

namespace PuppetMasterPMNR {
    public class PuppetMaster {

        private PuppetMasterForm puppetMasterForm;
        private IList<KeyValuePair<int, string>> workplace;
        private IList<string> puppetMasters;
        private string host;
        private KeyValuePair<int,string> jobtracker;
        private int port;
        private RemotePuppetMaster remotePuppetMaster;

        public PuppetMaster(PuppetMasterForm form, int port) {
            this.puppetMasterForm = form;
            this.puppetMasters = new List<string>();
            this.workplace = new List<KeyValuePair<int, string>>();
            this.host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            this.port = port;

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            remotePuppetMaster = new RemotePuppetMaster(this);
            RemotingServices.Marshal(remotePuppetMaster, "PM", typeof(RemotePuppetMasterInterface));
        }


        public string GetURI() {
            string uri = "tcp://" + host + ":" + this.port + "/PM";
            return uri;
        }


        public void AddPuppetMaster(string newPuppetMasterURL) {
            puppetMasters.Add(newPuppetMasterURL);
        }


        public void WORKER(int id, string puppetMasterURL, string serviceURL, string entryURL) {
            Uri baseUri = new Uri(puppetMasterURL);
            Uri sUri = new Uri(serviceURL);
            if (sUri.IsLoopback)
                serviceURL = "tcp://" + this.host + ":" + sUri.Port + "/W";
            if (entryURL != "NOENTRYPOINT") {
                Uri epUri = new Uri(entryURL);
                if (epUri.IsLoopback)
                    entryURL = "tcp://" + this.host + ":" + epUri.Port + "/W";
            }

            if (puppetMasterURL.Equals(GetURI()) || baseUri.IsLoopback) {

                string[] args = { id.ToString(), serviceURL, entryURL };

                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.RedirectStandardInput = false;

                System.IO.DirectoryInfo myDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                p.StartInfo.FileName = System.IO.Path.Combine(myDirectory.ToString(), "..\\..\\..\\WorkerPMNR\\bin\\Debug\\WorkerPMNR.exe");
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


        public void SUBMIT(string entryURL, string filePath, string outputFolderPath, string nSplits, string mapClassName, string dllFilePath) { 
            //Creates user application in local node. The application submits the designated job
            string jbURL = entryURL;
            Uri baseUri = new Uri(jbURL);
            if (baseUri.IsLoopback)
                jbURL = "tcp://" + this.host + ":" + baseUri.Port + "/W";

            foreach(KeyValuePair<int,string> k in workplace)
                if(k.Value == jbURL)
                    this.jobtracker = new KeyValuePair<int,string>(k.Key, k.Value);

            puppetMasterForm.BeginInvoke((Action)delegate {
                UserGUIForm userGUI = new UserGUIForm(entryURL, filePath, outputFolderPath, nSplits, mapClassName, dllFilePath);
                userGUI.SubmitJob();
            });

        }

        

        public void WAIT(int seconds) { 
            //PuppetMaster stops its script execution for x seconds

            System.Threading.Thread.Sleep(1000*seconds);
        }

        public void STATUS() { 
            //Makes each puppet master tell its workers to print status
            foreach (string pmAddress in puppetMasters) {
                RemotePuppetMasterInterface pm = (RemotePuppetMasterInterface)Activator.GetObject(typeof(RemotePuppetMasterInterface), pmAddress);
                pm.PrintWorkerStatus();
            }
            //print worker status from this machine workers
            printWorkerStatus();
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

        public void printWorkerStatus() {
            foreach (KeyValuePair<int, string> k in workplace) {
                Uri baseUri = new Uri(k.Value);
                if (baseUri.Host.Equals(this.host)) {
                    RemoteWorkerInterface rw = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), k.Value);
                    rw.PrintStatus();
                }
            }
            puppetMasterForm.SetWorkers(workplace, jobtracker);
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

            System.IO.DirectoryInfo myDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            p.StartInfo.FileName = System.IO.Path.Combine(myDirectory.ToString(), "..\\..\\..\\WorkerPMNR\\bin\\Debug\\WorkerPMNR.exe");
            p.StartInfo.Arguments = String.Join(" ", args);
            p.Start();
            puppetMaster.ReceiveNewWorker(id, serviceURL); //Adds workerID , URL pair
            puppetMaster.BroadcastNewWorker(id, serviceURL); //Advertises workerID , URL pair
        }

        public void Connect(string newPuppetMasterURL) {
            puppetMaster.AddPuppetMaster(newPuppetMasterURL);
        }

        public void ReceiveWorker(int workerID, string serviceURL) {
            puppetMaster.ReceiveNewWorker(workerID, serviceURL);
        }

        public void PrintWorkerStatus() { 
            puppetMaster.printWorkerStatus();
        }

    }
}
