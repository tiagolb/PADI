using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using InterfacePMNR;
using System.Net;
using System.Net.Sockets;


namespace ClientPMNR {
    public class Client {

        private RemoteWorkerInterface remoteWorker;
        private TcpChannel channel;
        private string url;
        // TODO: We need to access this from remoteClient
        public static string inputFile;
        public static string outputFolder;

        public void INIT(string entryURL) {
            remoteWorker = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), entryURL);
            channel = new TcpChannel(10001);
            // It should work but says we already registered a channel named 'tcp'
            //ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteClient), "C", WellKnownObjectMode.SingleCall);
            url = "tcp://" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            url += ":10001/C";
            remoteWorker.SetClientURL(url);

        }

        public int SUBMIT(string inputFilePath, int numberSplits, string outputFolderPath, string className, string dllFilePath) {
            inputFile = inputFilePath;
            outputFolder = outputFolderPath+"/";
            byte[] file = File.ReadAllBytes(inputFilePath);
            int fileSizeBytes = file.Length;
            byte[] dllCode = File.ReadAllBytes(dllFilePath);

            remoteWorker.JobMetaData(numberSplits, fileSizeBytes, dllCode, className);
            return fileSizeBytes;
        }

    }

    public class RemoteClient : MarshalByRefObject, RemoteClientInterface {

        private static Object thisLock = new Object();

        IList<IList<KeyValuePair<string, string>>> processedSplits;


        public RemoteClient() {
            this.processedSplits = new List<IList<KeyValuePair<string, string>>>();
        }

        /*
         * getSplit receives begin and end position of the file byte array
         * return byte array from begin to end
         */
        public byte[] getSplits(int begin, int end, int extraSplitSize) {
            int mySplitsSize = end - begin;
            int bytesToRead = mySplitsSize + extraSplitSize;

            byte[] splitBytes = new byte[bytesToRead];

            lock (thisLock) {
                using (BinaryReader reader = new BinaryReader(new FileStream(Client.inputFile, FileMode.Open))) {
                    reader.BaseStream.Seek(begin, SeekOrigin.Begin);
                    reader.Read(splitBytes, 0, bytesToRead);
                    reader.Close();
                }
            }

            string split = System.Text.Encoding.ASCII.GetString(splitBytes);
            int indexFirstNL, indexExtraNL;
            if (begin != 0) {
                indexFirstNL = split.IndexOf(Environment.NewLine) + Environment.NewLine.Length;
            } else {
                indexFirstNL = 0;
            }
            indexExtraNL = split.IndexOf(Environment.NewLine, mySplitsSize);

            int splitLength = indexExtraNL - indexFirstNL;

            if (splitLength > 0) {
                split = split.Substring(indexFirstNL, splitLength);
            } else {
                split = split.Substring(indexFirstNL);
            }
            return System.Text.Encoding.ASCII.GetBytes(split);
        }

        /*
         * sendProcessedSplit receives a processed split from worker
         */
        public void sendProcessedSplit(string result, int splitId) {

            File.WriteAllText(Client.outputFolder+splitId+".out", result);
        }
    }

}
