﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using WorkerPMNR;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using InterfacePMNR;


namespace ClientPMNR {
    public class Client {


        private RemoteWorker remoteWorker;
        private TcpChannel channel;
        private string url = "tcp://localhost:8086/Client";

        public void INIT(string entryURL) {

            remoteWorker = (RemoteWorker)Activator.GetObject(typeof(RemoteWorker), entryURL);
            remoteWorker.SetClientURL(url);
            //channel = new TcpChannel(8086);
            //ChannelServices.RegisterChannel(channel, true);
            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteClient), "Client", WellKnownObjectMode.SingleCall);

        }

        public int SUBMIT(string inputFilePath, int numberSplits, string outputFolderPath, string dllFilePath, string className) {
            byte[] file = File.ReadAllBytes(inputFilePath);
            int fileSizeBytes = file.Length;
            byte[] dllCode = File.ReadAllBytes(dllFilePath);

            remoteWorker.JobMetaData(numberSplits, fileSizeBytes, dllCode, className);
            return fileSizeBytes;
        }
 
    }

    public class RemoteClient : MarshalByRefObject, RemoteClientInterface{

        IList<IList<KeyValuePair<string, string>>> processedSplits;

        public RemoteClient() {
            this.processedSplits = new List<IList<KeyValuePair<string, string>>>();
        }


        /*public IList<string> getSplit(int begin, int end) {
            IList<string> result = new List<string>();

            return result;
        }*/

        /*
         * getSplit receives begin and end position of the file byte array
         * return byte array from begin to end
         */
        public byte[] getSplit(int begin, int end) {
            return new byte[]{};
        }

        public void sendProcessedSplit(IList<KeyValuePair<string, string>> result) {
            processedSplits.Add(result);
        }
    }

}
