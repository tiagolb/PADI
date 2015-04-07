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

namespace PuppetMasterPMNR {
    public class PuppetMaster {

        private PuppetMasterForm puppetMasterForm;
        private int lastPort;
        private IList<KeyValuePair<int, string>> workplace;
        private IList<string> puppetMasters;

        public PuppetMaster(PuppetMasterForm form) {
            this.puppetMasterForm = form;
            this.puppetMasters = new List<string>();
            this.workplace = new List<KeyValuePair<int, string>>();
            this.lastPort = 8090; //Hardcoded port

            TcpChannel channel = new TcpChannel(9000);   //Hardcoded port
            ChannelServices.RegisterChannel(channel, false);
            RemotePuppetMaster remotePuppetMaster = new RemotePuppetMaster(this);
            RemotingServices.Marshal(remotePuppetMaster, "PM", typeof(RemotePuppetMasterInterface));
        }


        public int GetLastPort() {
            return lastPort;
        }

        public void SetLastPort(int lastPort) {
            this.lastPort = lastPort;
        }

        public void AddPuppetMaster(string newPuppetMasterURL) {
            puppetMasters.Add(newPuppetMasterURL);
        }

        public void AddWorkplace(int workerID, string puppetMasterURL) {
            workplace.Add(new KeyValuePair<int,string>(workerID, puppetMasterURL));
        }

        public void WORKER(int id, string puppetMasterURL, string serviceURL, string entryURL) {
            RemotePuppetMasterInterface remotePuppetMasterConnector =
                (RemotePuppetMasterInterface)Activator.GetObject(typeof(RemotePuppetMasterInterface), puppetMasterURL);

            remotePuppetMasterConnector.CreateWorker(id, serviceURL, entryURL);
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
            //Injects the specified delay on worker with ID = id
        }

        public void FREEZEW(int id){
            //Disables communication of worker id
        }

        public void UNFREEZEW(int id) { 
            //Enables communication of worker id
        }

        public void FREEZEC(int id) { 
            //Disables communication of jobtacker in worker id
        }

        public void UNFREEZEC(int id) { 
            //Enables communication of jobtracker in worker id
        }
    }


    public class RemotePuppetMaster : MarshalByRefObject, RemotePuppetMasterInterface {

        private PuppetMaster puppetMaster;

        public RemotePuppetMaster(PuppetMaster pm) {
            this.puppetMaster = pm;
        }


        public void CreateWorker(int id, string serviceURL, string entryURL) {
            TcpChannel channel = new TcpChannel(puppetMaster.GetLastPort()+1);
            ChannelServices.RegisterChannel(channel, false);
            RemoteWorker remoteWorker = new RemoteWorker(puppetMaster.GetLastPort()+1);
            RemotingServices.Marshal(remoteWorker, "W", typeof(RemoteWorkerInterface));


        }


        public int Connect(string newPuppetMasterURL) {
            puppetMaster.AddPuppetMaster(newPuppetMasterURL);
            return puppetMaster.GetLastPort();
        }

        public void BroadcastNewWorker(int lastPort, int workerID, string puppetMasterURL) {
            puppetMaster.SetLastPort(lastPort);
            puppetMaster.AddWorkplace(workerID, puppetMasterURL);
        }

    }
}
