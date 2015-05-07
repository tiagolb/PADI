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
            string puppetMasterURL = args[3];
            string host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            Uri baseUri = new Uri(serviceURL);
            int port = baseUri.Port;

            Console.WriteLine("ServiceURL: " + serviceURL);
            Console.WriteLine("EntryPointURL: " + entryPointURL);


            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemoteWorker remoteWorker = new RemoteWorker(serviceURL, puppetMasterURL, id);

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
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>(split.Count);
            int newLineSize = Environment.NewLine.Length;
            this.totalSplitLinesProcess = split.Count - 1;
            for (int i = 0; i < split.Count; i++) {
                this.currentLineProcess = i;
                result.AddRange(processLine(split[i]));
            }
            return result;
        }

        private IList<KeyValuePair<string, string>> processLine(string fileLine) {
            object[] args = new object[] { fileLine };

            if (this.delay > 0) {
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
        private RemotePuppetMasterInterface puppetMaster;
        private string nextNodeURL;
        private string clientURL;
        private SplitPool<IList<string>> splitPool;
        private IList<string> remoteWorkers;
        private TimerCallback TimerDelegate;
        private const int TIMEOUT = 10000;

        public delegate void RemoteAsyncDelegate();

        //infinite lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
                  
        // Thread 
        Thread workerThread;

        public RemoteWorker() { }

        public RemoteWorker(string serviceURL, string puppetMasterURL, int id) {
            this.url = serviceURL;
            this.totalNodes = 1;
            this.id = id;
            this.nextNodeURL = this.url;
            this.remoteWorkers = new List<string>();
            this.remoteWorkers.Add(this.url);
            this.puppetMaster = (RemotePuppetMasterInterface)Activator.GetObject(typeof(RemotePuppetMasterInterface), puppetMasterURL);

            TimerDelegate = new TimerCallback(CheckAlive);
            Timer TimerItem = new Timer(TimerDelegate, new object(), TIMEOUT, TIMEOUT);
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


        public void SetNextNodeURL(string workerURL, IList<string> remoteWorkers) {
            /*if (nextNode == null) {
                Timer TimerItem = new Timer(TimerDelegate, new object(), TIMEOUT, TIMEOUT);
            }*/
            Console.WriteLine("NextNodeURL: " + workerURL);
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            nextNodeURL = workerURL;
            this.remoteWorkers = remoteWorkers;
        }

        /*private void checkNewNextNodeURL(int newNextNodeID) {
            string newNextNode = remoteWorkers[newNextNodeID];
            Console.WriteLine("Repaired - NextNodeURL: " + newNextNode);
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), newNextNode);
            nextNodeURL = newNextNode;
            nextNode.Check();
        }*/

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

            splitPool = new SplitPool<IList<string>>(numberSplits);

            int correctedBytesPerMachine = bytesPerSplit * numberSplits;

            if (extraBytes > 0) {
                if (extraBytes >= splitsPerMachine) {
                    correctedBytesPerMachine += splitsPerMachine;
                    extraBytes -= splitsPerMachine;
                }
                else {
                    correctedBytesPerMachine += extraBytes;
                    extraBytes = 0;
                }
            }

            int end = begin + correctedBytesPerMachine;
            int extraSplitBytes = bytesPerSplit;

            int beginNext = end + 1;
            if (this.topologyID != stopID) {
                // TODO: Async?
                Thread broadcastThread = new Thread(() => this.nextNode.Broadcast(stopID, beginNext, firstSplit + numberSplits, bytesPerSplit, extraBytes, splitsPerMachine, extraSplits, code, className));
                broadcastThread.Start();
            }
            
            Thread downloadThread = new Thread(() => getSplits(begin, end, bytesPerSplit, extraSplitBytes, stopID));
            downloadThread.Start();
            this.totalDataToProcess = end - begin;


            // TODO: Podiamos passar uma ref da splitPool para o worker e tê-lo a ele a
            //  espera de splits (pode poupar-nos uma thread)
            this.numberSplitsToProcess = numberSplits;
            workerThread = new Thread(() => processSplitsThread(numberSplits, firstSplit));
            workerThread.Start();
        }

        private void getSplits(int begin, int end, int splitSize, int extraSplitSize, int stopId) {

            int beginSplit = begin;
            int endSplit = beginSplit + splitSize;
            byte[] split;
            string splitText;
            IList<string> splitLines;
            int id = 1;
            while (endSplit < end) {
                split = this.client.getSplits(beginSplit, endSplit, extraSplitSize, id);
                splitText = System.Text.Encoding.ASCII.GetString(split);
                id++;

                splitLines = LowMemSplit(ref splitText, Environment.NewLine);

                splitPool.Add(splitLines);

                beginSplit = endSplit + 1;
                endSplit = beginSplit + splitSize;
            }

            if (this.topologyID == stopId) {
                extraSplitSize = 0;
            }

            split = this.client.getSplits(beginSplit, endSplit, extraSplitSize, id);
            splitText = System.Text.Encoding.ASCII.GetString(split);
            splitLines = LowMemSplit(ref splitText, Environment.NewLine);

            splitPool.Add(splitLines);
        }

        private void processSplitsThread(int myNumberSplits, int firstSplit) {

            int splitId = firstSplit;
            IList<KeyValuePair<string, string>> processedSplit;
            while (myNumberSplits > 0) {
                IList<string> split;
                split = splitPool.Get();
                this.currentSplit = splitId;
                this.currentSplitSize = 0;

                //for status monitoring purpose
                foreach (string s in split) {
                    this.currentSplitSize += s.Length;
                }

                StringBuilder result = new StringBuilder();
                processedSplit = worker.processSplit(split);
                foreach (KeyValuePair<string, string> pair in processedSplit) {
                    result.Append(pair.Key);
                    result.Append(" : ");
                    result.Append(pair.Value);
                    result.Append(Environment.NewLine);
                }
                #region debugComments
                //Console.WriteLine("Result: " + result);
                //Console.WriteLine("SentSplit " + splitId);
                #endregion
                client.sendProcessedSplit(result.ToString(), splitId);
                splitId++;
                myNumberSplits--;
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
            int newTopologyId = topologyID + 1;

            if (nextNode != null) {   //Compute the ID of the node that shall stop broadcasting
                int stopID = mod((this.topologyID - 1), totalNodes);
                nextNode.JoinBroadcast(stopID, newTopologyId, newTopologyId, workerURL);
            } /*else {
                Timer TimerItem = new Timer(TimerDelegate, new object(), TIMEOUT, TIMEOUT);
            }*/

            // Inserts in list the new WorkerURL
            // It is inserted after my own URL so the list stays ordered
            remoteWorkers.Insert(newTopologyId, workerURL);

            //Update nextNodes references, both for the new node and the entry point node
            // TODO: O prof disse que se houvesse uma chamada a um objecto remoto dentro de uma chamada desse objecto remoto
            //  podia haver problemas
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            nextNode.SetNextNodeURL(nextNodeURL, remoteWorkers);
            nextNodeURL = workerURL;

            Console.WriteLine("Connect -> ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
            //Returns to the new Node it's ID and Total Nodes in the ring
            return new int[] { topologyID + 1, totalNodes };
        }



        public void JoinBroadcast(int stopID, int previousNodeID, int newTopologyId, string newWorker) {
            //The node receiving the broadcast updates the number of Total Nodes in the System
            // and computes it's new ID

            totalNodes++;
            this.topologyID = mod((previousNodeID + 1), totalNodes);
            this.remoteWorkers.Insert(newTopologyId, newWorker);
            Console.WriteLine("JoinBroadcast -> ID: " + /*this.id + ", topologyID: " +*/ this.topologyID + " totalNodes: " + this.totalNodes);
            if (stopID != this.topologyID) {
                nextNode.JoinBroadcast(stopID, this.topologyID, newTopologyId, newWorker);
            }
        }

        private void CheckAlive(object StateObj) {
            if (remoteWorkers.Count > 1) {
                // From the node behind him
                /*int previousTopologyId = mod((this.topologyID - 1), totalNodes);
                string previousWorkerURL = remoteWorkers[previousTopologyId];

                RemoteWorkerInterface previousWorker = 
                    (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), previousWorkerURL);
                */
                // TODO: we can make a more traditional imAlive with async calls

                //RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(previousWorker.Check);
                // Call delegate to remote method
                //IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);

                try {
                    this.nextNode.Check();
                }
                catch (SocketException) {
                    // TODO: Handle Failure
                    Console.WriteLine("{0} has died - Initiate CPR protocol", this.nextNodeURL);
                    RepairTopologyChain();
                }
            }
        }

        private void RepairTopologyChain() {
            int deadNodeID = mod((this.topologyID + 1), totalNodes);
            string deadNodeURL = nextNodeURL;

            int newNextNodeID = mod((deadNodeID + 1), totalNodes);
            this.totalNodes--;

            string newNextNode = remoteWorkers[newNextNodeID];
            Console.WriteLine("Repaired - NextNodeURL: " + newNextNode);
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), newNextNode);
            nextNodeURL = newNextNode;

            remoteWorkers.RemoveAt(deadNodeID);
            this.topologyID = remoteWorkers.IndexOf(this.url);

            int stopID = mod((this.topologyID - 1), totalNodes);
            nextNode.RepairTopologyChain(stopID, deadNodeID);

            

            #region GG
            /*List<string> toRemoveURL = new List<string>();
            toRemoveURL.Add(deadNodeURL); // failed node

            for (int newNextNodeID = deadNodeID + 1; newNextNodeID != this.topologyID; newNextNodeID = mod((newNextNodeID + 1), totalNodes)) {
                try {
                    checkNewNextNodeURL(newNextNodeID);
                    Console.WriteLine("Chain repaired with worker {0}, ID: {1}", nextNodeURL, newNextNodeID);
                } catch (SocketException) {
                    deadNodeURL = nextNodeURL;
                    toRemoveURL.Add(deadNodeURL);
                }
            }

            // remove failed nodes from list
            foreach (string toRemoveNodeURL in toRemoveURL) {
                remoteWorkers.Remove(toRemoveNodeURL);
            }*/
            #endregion
        }

        public void RepairTopologyChain(int stopID, int deadNodeID) {
            // remove failed nodes from list

            //Should be made async
            string deadNodeURL = remoteWorkers[deadNodeID];
            Console.WriteLine("Node Fell: " + deadNodeID + " " + deadNodeURL);
            puppetMaster.RegisterLostWorker(deadNodeID, deadNodeURL);

            remoteWorkers.RemoveAt(deadNodeID);
            this.topologyID = remoteWorkers.IndexOf(this.url);

            if (this.topologyID != stopID) {
                nextNode.RepairTopologyChain(stopID, deadNodeID);
            }
        }

        // Just to check connection
        public void Check() {
            Console.WriteLine("I am Alive");
        }

        public void PrintStatus() {
            Console.WriteLine("==================CURRENT STATUS===================");
            if (worker != null) {
                Console.WriteLine("Processing line " + worker.GetCurrentLineProcess() + "/" + worker.GetTotalSplitLinesProcess() + " of split " + this.currentSplit + " which has " + this.currentSplitSize + " bytes");
                Console.WriteLine("Received " + totalDataToProcess + " total bytes to process in " + numberSplitsToProcess + " splits");
            }
            else Console.WriteLine("No Job is currently being performed");
            Console.WriteLine("Worker ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
            Console.WriteLine("===================================================");
            Console.WriteLine("Remote Workers list:");
            foreach (string remoteWorker in remoteWorkers) {
                Console.WriteLine(remoteWorker);
            }
            Console.WriteLine("===================================================");
            //DEBUG
            foreach (string remoteWorker in remoteWorkers) {
                Console.WriteLine("worker: {0}", remoteWorker);
            }
        }

        private List<string> LowMemSplit(ref string s, string seperator) {
            List<string> list = new List<string>();
            int lastPos = 0;
            int pos = s.IndexOf(seperator);
            while (pos > -1) {
                while (pos == lastPos) {
                    lastPos += seperator.Length;
                    pos = s.IndexOf(seperator, lastPos);
                    if (pos == -1)
                        return list;
                }

                string tmp = s.Substring(lastPos, pos - lastPos);
                if (tmp.Trim().Length > 0)
                    list.Add(tmp);
                lastPos = pos + seperator.Length;
                pos = s.IndexOf(seperator, lastPos);
            }

            if (lastPos < s.Length) {
                string tmp = s.Substring(lastPos, s.Length - lastPos);
                if (tmp.Trim().Length > 0)
                    list.Add(tmp);
            }

            return list;
        }
    }

}
