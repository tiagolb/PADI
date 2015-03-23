﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerPMNR;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.ChannelServices;


namespace ClientPMNR {
    public class Client {


        private Worker worker;
        private TcpChannel channel;

        public void INIT(string entryURL) {

            worker = (Worker)Activator.GetObject(typeof(Worker), entryURL);

            channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteClient), "Client", WellKnownObjectMode.SingleCall);

        }

        public bool SUBMIT(string inputFilePath, int numberSplits, string outputFolderPath, string dllFilePath) {


            return true;
        }
 
    }

    public class RemoteClient : MarshalByRefObject{

        IList<IList<KeyValuePair<string, string>>> processedSplits;

        public RemoteClient() {
            this.processedSplits = new List<IList<KeyValuePair<string, string>>>();
        }

        public IList<KeyValuePair<string, string>> getSplit(int begin, int end) {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            return result;
        }

        public void sendProcessedSplit(IList<KeyValuePair<string, string>> result) {
            processedSplits.Add(result);
        }
    }

}