using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerPMNR;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.ChannelServices;


namespace ClientPMNR
{
    public class Client
    {

        private Worker worker;

        public void INIT(String entryURL) {

            worker = (Worker)Activator.GetObject(typeof(Worker), entryURL);

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);

        }

        public bool SUBMIT(String inputFilePath, int numberSplits, String outputFolderPath, String dllFilePath) {
            

            return true;
        }
    }
}
