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

        public static bool IS_NOT_FINISHED;

        private RemoteWorkerInterface remoteWorker;
        //private RemoteClient remoteClient;

        public static TcpChannel channel;
        private string url;
        // TODO: We need to access this from remoteClient
        public static int totalSplits;
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
            totalSplits = numberSplits;
            inputFile = inputFilePath;
            outputFolder = outputFolderPath+"/";
            // TODO: em vez de ler todos os bytes para memória fazer:
            /*FileInfo file = new FileInfo(inputFilePath);
            int fileSizeBytes = file.Length;*/
            byte[] file = File.ReadAllBytes(inputFilePath);
            int fileSizeBytes = file.Length;
            byte[] dllCode = File.ReadAllBytes(dllFilePath);

            remoteWorker.JobMetaData(numberSplits, fileSizeBytes, dllCode, className);
            return fileSizeBytes;
        }

        public void unregisterChannel() {
            Client.channel.StopListening(null);
            Client.channel = null;
        }

    }

    public class RemoteClient : MarshalByRefObject, RemoteClientInterface {

        private static Object thisLock = new Object();

        public RemoteClient() {}

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
            splitBytes = null; // free memory
            int indexFirstNL, indexExtraNL;
            if (begin != 0) {
                indexFirstNL = split.IndexOf(Environment.NewLine) + Environment.NewLine.Length;
            } else {
                indexFirstNL = 0;
            }
            indexExtraNL = split.IndexOf(Environment.NewLine, mySplitsSize);

            int splitLength = indexExtraNL - indexFirstNL;
            // TODO: Senao existir um newline no meio do split (que ja vimos que acontece) a logica esta mal
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
            Client.totalSplits--;
            File.WriteAllText(Client.outputFolder+splitId+".out", result);
            if (Client.totalSplits == 0) {
                Client.IS_NOT_FINISHED = false;
            }

        }

        // TODO: Usar isto para procurar nemLine no array de bytes
        private int FindNewLine(ref byte[] split, int startIndex = 0) {
            byte[] newLine = Encoding.ASCII.GetBytes(Environment.NewLine);
            for (int i = startIndex; i < split.Length; i++) {
                if (split[i] == newLine[0] && split[i + 1] == newLine[1]) {
                    return i;
                }
            }
            return -1;
        }
    }

}
