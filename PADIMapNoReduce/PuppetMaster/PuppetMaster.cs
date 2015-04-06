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

namespace PuppetMasterPMNR {
    class PuppetMaster {

        public PuppetMaster() {

        }

        public void WORKER(int id, string puppetMasterURL, string serviceURL, string entryURL) { 
            
        }

        public void SUBMIT(string entryURL, string filePath, string outputFolderPath, string nSplits, string dllFilePath, string mapClassName) { 
            //Creates user application in local node. The application submits the designated job
            UserGUIForm userGUI = new UserGUIForm(entryURL, filePath, outputFolderPath, nSplits, dllFilePath, mapClassName);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(userGUI);
           // userGUI.SubmitJob();
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

        public void CreateWorker(int id, string serviceURL, string entryURL) {
        }

    }
}
