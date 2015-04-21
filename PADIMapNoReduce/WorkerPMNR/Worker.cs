using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Reflection;
using System.Net;
using PADIMapNoReduce;
using InterfacePMNR;
using System.Net.Sockets;
using System.Threading;


namespace WorkerPMNR {
    public class WorkerPMNR {


        public static void Main(string[] args) {
            int id = Int32.Parse(args[0]);
            string serviceURL = args[1];
            string entryPointURL = args[2];
            string host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            Uri baseUri = new Uri(serviceURL);
            int port = baseUri.Port;

            Console.WriteLine("ServiceURL: " + serviceURL);
            Console.WriteLine("EntryPointURL: " + entryPointURL);


            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemoteWorker remoteWorker = new RemoteWorker(serviceURL, id);

            RemotingServices.Marshal(remoteWorker, "W", typeof(RemoteWorkerInterface));

            if (entryPointURL != "NOENTRYPOINT")
                remoteWorker.ConnectToChain(entryPointURL, serviceURL);

            Console.WriteLine("<enter> para sair...");
            Console.ReadLine();
        }

    }

    public class Worker {
        private Type type;
        private IMapper classObj;
        private int currentLineProcess;
        private int totalSplitLinesProcess;
        private int delay;

        public Worker(byte[] code, string className) {
            Assembly assembly = Assembly.Load(code);
            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes()) {
                if (type.IsClass == true) {
                    if (type.FullName.EndsWith("." + className)) {
                        // create an instance of the object
                        this.type = type;
                        this.classObj = (IMapper)Activator.CreateInstance(type);
                    }
                }
            }
        }

        public int GetCurrentLineProcess() {
            return this.currentLineProcess;    
        }

        public int GetTotalSplitLinesProcess() {
            return this.totalSplitLinesProcess;
        }

        public IList<KeyValuePair<string, string>> processSplit(IList<string> split) {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            int newLineSize = Environment.NewLine.Length;
            foreach (string line in split) {
                this.currentLineProcess = split.IndexOf(line);
                this.totalSplitLinesProcess = split.Count -1;
                result = result.Concat(processLine(line)).ToList();
            }
            return result;
        }

        private IList<KeyValuePair<string, string>> processLine(string fileLine) {
            object[] args = new object[] { fileLine };
            
            if (this.delay != 0) {
                Console.WriteLine("ENTREI");
                Thread.Sleep(this.delay * 1000);
                this.delay = 0;
            }

            object resultObject = type.InvokeMember("Map",
                   BindingFlags.Default | BindingFlags.InvokeMethod,
                   null,
                   classObj,
                   args);
            IList<KeyValuePair<string, string>> r = (IList<KeyValuePair<string, string>>)resultObject;
            return (IList<KeyValuePair<string, string>>)resultObject;
        }

        public void ApplyDelay(int secondsDelay) {
            this.delay = secondsDelay;
        }
    }


    public class RemoteWorker : MarshalByRefObject, RemoteWorkerInterface {
        private string url;
        private int totalNodes;
        private int id;
        private int topologyID;
        private int currentSplit;
        private int currentSplitSize;
        private int totalDataToProcess;
        private int numberSplitsToProcess;
        private Worker worker;
        private RemoteWorkerInterface nextNode;
        private RemoteClientInterface client;
        private string nextNodeURL;
        private string clientURL;

        // Thread 
        Thread workerThread;

        public RemoteWorker() { }

        public RemoteWorker(string serviceURL, int id) {
            this.url = serviceURL;
            this.totalNodes = 1;
            this.id = id;
            this.nextNodeURL = this.url;
        }

        public void SetClientURL(string clientURL) {
            if (nextNode != null) {
                int stopID = mod((this.topologyID - 1), totalNodes);
                nextNode.BroadcastClient(stopID, clientURL);
            }
            Console.WriteLine("Client: " + clientURL);
            client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), clientURL);
        }

        public void Slow(int secondsDelay) {
            worker.ApplyDelay(secondsDelay);
        }

        public void BroadcastClient(int stopId, string clientURL) {
            this.clientURL = clientURL;
            client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), clientURL);
            Console.WriteLine("Client: " + clientURL);
            if (this.topologyID != stopId && nextNode != null) {
                nextNode.BroadcastClient(stopId, clientURL);
            }
        }


        public void SetNextNodeURL(string workerURL) {
            Console.WriteLine("NextNodeURL: " + workerURL);
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            nextNodeURL = workerURL;
        }

        private int mod(int x, int m) {
            return (x % m + m) % m;
        }

        public void Broadcast(int stopID, int begin, int firstSplit, int bytesPerSplit, int extraBytes, int splitsPerMachine, int extraSplits, byte[] code, string className) {

            // init worker for each job
            worker = new Worker(code, className);

            int numberSplits = splitsPerMachine;
            if (extraSplits > 0) {
                extraSplits--;
                numberSplits++;
            }
            int correctedBytesPerMachine = bytesPerSplit * numberSplits;

            if (extraBytes > 0) {
                if (extraBytes >= splitsPerMachine) {
                    correctedBytesPerMachine += splitsPerMachine;
                    extraBytes -= splitsPerMachine;
                } else {
                    correctedBytesPerMachine += extraBytes;
                    extraBytes = 0;
                }
            }

            int end = begin + correctedBytesPerMachine;
            int extraSplitBytes = bytesPerSplit;
            if (this.topologyID == stopID) {
                extraSplitBytes = 0;
            }
            byte[] splits = this.client.getSplits(begin, end, extraSplitBytes);
            this.totalDataToProcess = splits.Length;

            begin = end + 1;
            if (this.topologyID != stopID) {
                Thread broadcastThread = new Thread(() => this.nextNode.Broadcast(stopID, begin, firstSplit + numberSplits, bytesPerSplit, extraBytes, splitsPerMachine, extraSplits, code, className));
                broadcastThread.Start();
            }

            this.numberSplitsToProcess = numberSplits;
            workerThread = new Thread(() => processSplitsThread(splits, numberSplits, firstSplit));
            workerThread.Start();
        }


        private void processSplitsThread(byte[] splitsBytes, int myNumberSplits, int firstSplit) {
            IList<IList<string>> splits = new List<IList<string>>();

            string splitText = System.Text.Encoding.ASCII.GetString(splitsBytes);

            string[] splitLines = splitText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            IList<string> splitLinesList = new List<string>(splitLines);

            int linesPerSplit = splitLines.Count() / myNumberSplits;
            for (int i = 0; i < myNumberSplits - 1; i++) { // para o ultimo receber fora
                IList<string> split = new List<string>();
                for (int j = 0; j < linesPerSplit; j++) {
                    split.Add(splitLinesList[0]);
                    splitLinesList.RemoveAt(0);
                }
                splits.Add(split);
            }

            //ultimo split
            IList<string> lastSplit = new List<string>();
            foreach (string line in splitLinesList) {
                lastSplit.Add(line);
            }
            splits.Add(lastSplit);

            //PROCESS
            IList<KeyValuePair<string, string>> processedSplit;
            int splitId = firstSplit;
           

            foreach (IList<string> split in splits) {
                this.currentSplit = splitId;
                this.currentSplitSize = 0;
                //for status monitoring purpose
                foreach (string s in split) {
                this.currentSplitSize += System.Text.ASCIIEncoding.ASCII.GetByteCount(s);
                }
                
                string result = "";
                Thread.Sleep(30 * 1000);
                processedSplit = worker.processSplit(split);
                foreach (KeyValuePair<string, string> pair in processedSplit) {
                    result += pair.Key + " : " + pair.Value + Environment.NewLine;
                }
                #region debugComments
                //Console.WriteLine("Result: " + result);
                //Console.WriteLine("SentSplit " + splitId);
                #endregion
                client.sendProcessedSplit(result, splitId);
                splitId++;
            }
            worker = null;
        }

        public void JobMetaData(int numberSplits, int nBytes, byte[] code, string className) {
            #region debugComments
            //Console.WriteLine("JobMetaData -> " + numberSplits);
            #endregion
            nBytes--;
            int bytesPerSplit = nBytes / numberSplits;
            int extraBytes = mod(nBytes, numberSplits);

            int splitsPerMachine = numberSplits / this.totalNodes;
            int extraSplits = mod(numberSplits, this.totalNodes);

            #region debugComments
            //Console.WriteLine("JobMetaData NSplits -> " + numberSplits);
            //Console.WriteLine("JobMetaData bps-> " + bytesPerSplit);
            //Console.WriteLine("JobMetaData eb-> " + extraBytes);
            //Console.WriteLine("JobMetaData spm-> " + splitsPerMachine);
            //Console.WriteLine("JobMetaData es-> " + extraSplits);
            #endregion

            int stopID = 0;
            if (nextNode != null) {   //Compute the ID of the node that shall stop broadcasting
                stopID = mod((this.topologyID - 1), totalNodes);
            }

            Broadcast(stopID, 0 /* begin */, 1 /* split id */, bytesPerSplit, extraBytes, splitsPerMachine, extraSplits, code, className);
        }



        public void ConnectToChain(string entryPointURL, string newNodeURL) {
            RemoteWorkerInterface remoteWorkerConnector =
                (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), entryPointURL);

            int[] connectionData = remoteWorkerConnector.Connect(newNodeURL);
            this.topologyID = connectionData[0];
            this.totalNodes = connectionData[1];
            Console.WriteLine("ConnectToChain -> ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
        }



        public int[] Connect(string workerURL) {
            this.totalNodes++;

            if (nextNode != null) {   //Compute the ID of the node that shall stop broadcasting
                int stopID = mod((this.topologyID - 1), totalNodes);
                nextNode.JoinBroadcast(stopID, this.topologyID + 1);
            }

            //Update nextNodes references, both for the new node and the entry point node
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            nextNode.SetNextNodeURL(nextNodeURL);
            nextNodeURL = workerURL;
            Console.WriteLine("Connect -> ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
            //Returns to the new Node it's ID and Total Nodes in the ring
            return new int[] { topologyID + 1, totalNodes };
        }



        public void JoinBroadcast(int stopID, int previousNodeID) {
            //The node receiving the broadcast updates the number of Total Nodes in the System
            // and computes it's new ID

            totalNodes++;
            this.topologyID = mod((previousNodeID + 1), totalNodes);
            Console.WriteLine("JoinBroadcast -> ID: "+ /*this.id + ", topologyID: " +*/ this.topologyID + " totalNodes: " + this.totalNodes);
            if (stopID != this.topologyID) {
                nextNode.JoinBroadcast(stopID, this.topologyID);
            }
        }

        public void PrintStatus() {
            Console.WriteLine("==================CURRENT STATUS===================");
            if (worker != null) {
                Console.WriteLine("Processing line " + worker.GetCurrentLineProcess() + "/" + worker.GetTotalSplitLinesProcess() +" of split " + this.currentSplit + " which has " + this.currentSplitSize + " bytes");
                Console.WriteLine("Received " + totalDataToProcess + " total bytes to process in " + numberSplitsToProcess + " splits");
            }
            else Console.WriteLine("No Job is currently being performed");
            Console.WriteLine("Worker ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
            Console.WriteLine("===================================================");
        }
    }

}
