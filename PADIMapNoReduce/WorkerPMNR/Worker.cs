using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            try
            {
                TcpChannel channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);
            }
            catch (SocketException) {
                Console.WriteLine("A worker is already registered at port {0}. Press <Enter> to close this process", port);
                Console.ReadLine();
                Environment.Exit(0);
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("ServiceURL: " + serviceURL);
            Console.WriteLine("EntryPointURL: " + entryPointURL);

            RemoteWorker remoteWorker = new RemoteWorker(serviceURL, puppetMasterURL, entryPointURL, id);

            RemotingServices.Marshal(remoteWorker, "W", typeof(RemoteWorkerInterface));


            if (entryPointURL != "NOENTRYPOINT")
                remoteWorker.ConnectToChain(entryPointURL, serviceURL);
            Console.ResetColor();
            Console.ReadLine();
        }

    }

    public class Worker {
        private Type type;
        private IMapper classObj;
        private int currentLineProcess;
        private int totalSplitLinesProcess;
        private int delay;
        private bool frozen;

        public Worker(byte[] code, string className) {
            this.frozen = false;
            this.delay = 0;
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

            lock (this) {
                while (this.frozen) {
                    Monitor.Wait(this);
                }
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

        public void ApplyFreeze() {
            lock (this) {
                this.frozen = true;
            }
        }

        public void ApplyUnfreeze() {
            lock (this) {
                this.frozen = false;
                Monitor.Pulse(this);
            }
        }
    }


    public class RemoteWorker : MarshalByRefObject, RemoteWorkerInterface {
        private string url;
        private string previousNodeURL;
        private int totalNodes;
        private int id;
        private int topologyID;
        private int currentSplit;
        private int currentSplitSize;
        private int totalDataToProcess;
        private int numberSplitsToProcess;
        private int lastSentByte;
        private int lastByteToProcess;
        private Worker worker;
        private RemoteWorkerInterface nextNode;
        private RemoteClientInterface client;
        private RemotePuppetMasterInterface puppetMaster;
        private string nextNodeURL;
        private string clientURL;
        private SplitPool<KeyValuePair<int[], IList<string>>> splitPool;
        private IList<string> remoteWorkers;
        private ExtraWorkWrapper extraWork;
        private ExtraWorkWrapper myWork;
        private TimerCallback TimerDelegate;
        private const int TIMEOUT = 3000;
        private Timer TimerItem;
        private bool frozenComm;
        private int splitsSent;
        private bool neighborDied = false;
        private IList<string> deadNodes;
        private int sendId;
        private int firstSplit;

        public delegate void ReconnectDelegate();
        public delegate void IdLocationDelegate(int stopId, string clientURL);
        public delegate void splitResultDelegate(string result, int splitId);
        public delegate void BroadcastDelegate(int stopID, int begin, int firstSplit, int bytesPerSplit, int extraBytes, int splitsPerMachine, int extraSplits, byte[] code, string className);
        public delegate void JoinBroadcastDelegate(int stopID, int previousNodeID, int newTopologyId, string newWorker);
        public delegate void SetNextNodeURLDelegate(string workerURL, IList<string> remoteWorkers);
        public delegate void RepairTopologyDelegate(int stopID, int deadNodeID);

        //infinite lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
                  
        // Thread 
        Thread workerThread;

        public RemoteWorker(string serviceURL, string puppetMasterURL, string entryPointURL, int id) {
            this.previousNodeURL = entryPointURL;
            this.frozenComm = false;
            this.url = serviceURL;
            this.totalNodes = 1;
            this.id = id;
            this.nextNodeURL = this.url;
            this.extraWork = new ExtraWorkWrapper();
            this.myWork = new ExtraWorkWrapper();
            this.remoteWorkers = new List<string>();
            this.remoteWorkers.Add(this.url);
            this.puppetMaster = (RemotePuppetMasterInterface)Activator.GetObject(typeof(RemotePuppetMasterInterface), puppetMasterURL);

            this.deadNodes = new List<string>();

            TimerDelegate = new TimerCallback(CheckAlive);
            TimerItem = new Timer(TimerDelegate, new object(), TIMEOUT, TIMEOUT);
        }

        public void SetClientURL(string clientURL) {
            if (frozenComm)
                return;

            if (nextNode != null) {
                int stopID = mod((this.topologyID - 1), totalNodes);

                IdLocationDelegate RemoteDel = new IdLocationDelegate(nextNode.BroadcastClient);
                RemoteDel.BeginInvoke(stopID, clientURL, null, null);
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Client: " + clientURL);
            Console.ResetColor();
            client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), clientURL);
        }

        public void Slow(int secondsDelay) {
            if (frozenComm)
                return;

            worker.ApplyDelay(secondsDelay);
        }

        public void BroadcastClient(int stopId, string clientURL) {
            if (frozenComm)
                return;

            this.clientURL = clientURL;
            client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), clientURL);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Client: " + clientURL);
            Console.ResetColor();
            if (this.topologyID != stopId && nextNode != null) {
                IdLocationDelegate RemoteDel = new IdLocationDelegate(nextNode.BroadcastClient);
                RemoteDel.BeginInvoke(stopId, clientURL, null, null);
            }
        }


        public void SetNextNodeURL(string workerURL, IList<string> remoteWorkers) {
            if (frozenComm)
                return;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("NextNodeURL: " + workerURL);
            Console.ResetColor();
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            nextNodeURL = workerURL;
            this.remoteWorkers = remoteWorkers;
        }


        private int mod(int x, int m) {
            return (x % m + m) % m;
        }

        public void Broadcast(int stopID, int begin, int firstSplit, int bytesPerSplit, int extraBytes, int splitsPerMachine, int extraSplits, byte[] code, string className) {
            if (frozenComm)
                return;

            this.firstSplit = firstSplit;

            // init worker for each job
            worker = new Worker(code, className);

            int numberSplits = splitsPerMachine;
            if (extraSplits > 0) {
                extraSplits--;
                numberSplits++;
            }

            splitPool = new SplitPool<KeyValuePair<int[], IList<string>>>(numberSplits);

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

                BroadcastDelegate RemoteDel = new BroadcastDelegate(this.nextNode.Broadcast);
                RemoteDel.BeginInvoke(stopID, beginNext, firstSplit + numberSplits, bytesPerSplit, extraBytes, splitsPerMachine, extraSplits, code, className, null, null);
            }

            //lastSplitToProcess = firstSplit + numberSplits -1;
            lastByteToProcess = end;
            lastSentByte = begin;
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
            int bytes = 0;
            IList<string> splitLines;
            int id = firstSplit;
            while (endSplit < end-1) {
                split = this.client.getSplits(beginSplit, endSplit, extraSplitSize, id);
                splitText = System.Text.Encoding.ASCII.GetString(split);

                splitLines = LowMemSplit(ref splitText, Environment.NewLine);

                bytes = split.Count();
                KeyValuePair<int[], IList<string>> splitPairWhile = new KeyValuePair<int[], IList<string>>(new int[] {bytes, id}, splitLines);

                splitPool.Add(splitPairWhile);
                id++;

                beginSplit = endSplit + 1;
                endSplit = beginSplit + splitSize;
            }

            if (this.topologyID == stopId) {
                extraSplitSize = 0;
            }

            split = this.client.getSplits(beginSplit, endSplit, extraSplitSize, id);
            splitText = System.Text.Encoding.ASCII.GetString(split);
            splitLines = LowMemSplit(ref splitText, Environment.NewLine);

            bytes = split.Count();
            KeyValuePair<int[], IList<string>> splitPair = new KeyValuePair<int[], IList<string>>(new int[] {bytes, id}, splitLines); 

            splitPool.Add(splitPair);
        }

        private void ProcessExtraJobs() {
            int firstByte, lastByte, numSplits, splitSize, splitId;

            byte[] split;
            string splitText;
            int bytes = 0;
            IList<string> splitLines;
            int endSplit;
            int id;
            IList<KeyValuePair<string, int[]>> extraWCopy = new List<KeyValuePair<string, int[]>>(myWork.getExtraWork());

            foreach (KeyValuePair<string, int[]> extra in extraWCopy) {

                /*
                Console.WriteLine("extra.Key: {0}", extra.Key);
                foreach (string deadNode in deadNodes) {
                    Console.WriteLine("deadNode: {0}", deadNode);
                }*/

                if (deadNodes.Contains(extra.Key)) { 
                    Console.WriteLine("Fetching ExtraJobs");
                    lastByte = extra.Value[1];
                    firstByte = extra.Value[0];
                    numSplits = extra.Value[2];
                    splitId = extra.Value[3];
                    // Nao esquecer que pode ser o resto
                    splitSize = (lastByte - firstByte) / numSplits;
                    id = splitId;

                    for (int i = 0; i < numSplits; i++) {
                        endSplit = firstByte + splitSize;

                        //Console.WriteLine("firstByte {0}, endSplit {1}, splitSize {2}, sendId {3}", firstByte, endSplit, splitSize, sendId);
                        
                        split = this.client.getSplits(firstByte, endSplit, splitSize, sendId);

                        splitText = System.Text.Encoding.ASCII.GetString(split);

                        splitLines = LowMemSplit(ref splitText, Environment.NewLine);

                        bytes = split.Count();

                        //Console.WriteLine("Extra ID: " + id);
                        KeyValuePair<int[], IList<string>> splitPairWhile = new KeyValuePair<int[], IList<string>>(new int[] { bytes, id }, splitLines);
                        id++;

                        splitPool.Add(splitPairWhile);

                        firstByte = endSplit + 1;
                    }
                }
            }
        }

        private void processSplitsThread(int myNumberSplits, int firstSplit) {

            sendId = firstSplit;
            IList<KeyValuePair<string, string>> processedSplit;
            while (myNumberSplits > 0 || splitPool.getElements() > 0) {
                KeyValuePair<int[], IList<string>> split;
                split = splitPool.Get();
                IList<string> splitList = split.Value;
                int bytes = split.Key[0];
                int id = split.Key[1];
                this.currentSplit = sendId;
                this.currentSplitSize = 0;

                //for status monitoring purpose
                foreach (string s in splitList) {
                    this.currentSplitSize += s.Length;
                }

                StringBuilder result = new StringBuilder();
                processedSplit = worker.processSplit(splitList);
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
                splitResultDelegate RemoteDel = new splitResultDelegate(client.sendProcessedSplit);
                RemoteDel.BeginInvoke(result.ToString(), id, null, null);
                //lastSentByte = splitId;
                lastSentByte += bytes;
                splitsSent++;
 
                sendId++;
                myNumberSplits--;
            }

            worker = null;
        }

        public void JobMetaData(int numberSplits, int nBytes, byte[] code, string className) {
            if (frozenComm)
                return;

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
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("ConnectToChain -> ID: {0} topologyID: {1} totalNodes: {2}", this.id, this.topologyID, this.totalNodes);
            Console.WriteLine("PreviousNode: {0}", entryPointURL);
            Console.ResetColor();
        }



        public int[] Connect(string workerURL) {
            if (frozenComm)
                throw new SocketException();

            this.totalNodes++;
            int newTopologyId = topologyID + 1;

            if (nextNode != null) {   //Compute the ID of the node that shall stop broadcasting
                int stopID = mod((this.topologyID - 1), totalNodes);
                JoinBroadcastDelegate RemoteDel = new JoinBroadcastDelegate(nextNode.JoinBroadcast);
                RemoteDel.BeginInvoke(stopID, newTopologyId, newTopologyId, workerURL, null, null);

            }
            else this.previousNodeURL = workerURL;

            // Inserts in list the new WorkerURL
            // It is inserted after my own URL so the list stays ordered
            remoteWorkers.Insert(newTopologyId, workerURL);

            //Update nextNodes references, both for the new node and the entry point node
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);

            SetNextNodeURLDelegate nextNodeUrlDel = new SetNextNodeURLDelegate(nextNode.SetNextNodeURL);
            nextNodeUrlDel.BeginInvoke(nextNodeURL, remoteWorkers, null, null);
            nextNodeURL = workerURL;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Connect -> ID: {0} topologyID: {1} totalNodes: {2}", this.id, this.topologyID, this.totalNodes);
            Console.WriteLine("My previous node is {0}", this.previousNodeURL);
            Console.ResetColor();
            //Returns to the new Node it's ID and Total Nodes in the ring
            return new int[] { topologyID + 1, totalNodes };
        }



        public void JoinBroadcast(int stopID, int previousNodeID, int newTopologyId, string newWorker) {
            if (frozenComm)
                return;
            //The node receiving the broadcast updates the number of Total Nodes in the System
            // and computes it's new ID

            totalNodes++;
            this.topologyID = mod((previousNodeID + 1), totalNodes);
            this.remoteWorkers.Insert(newTopologyId, newWorker);
            this.previousNodeURL = remoteWorkers[mod((this.topologyID - 1), totalNodes)];
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("JoinBroadcast -> ID: {0} topologyID: {1} totalNodes: {2}", this.id, this.topologyID, this.totalNodes);
            Console.WriteLine("My previous node is {0}", this.previousNodeURL);
            Console.ResetColor();
            if (stopID != this.topologyID) {
                JoinBroadcastDelegate RemoteDel = new JoinBroadcastDelegate(nextNode.JoinBroadcast);
                RemoteDel.BeginInvoke(stopID, this.topologyID, newTopologyId, newWorker, null, null);
            }
        }

        private void CheckAlive(object StateObj) {
            if (!frozenComm) {
                if (remoteWorkers.Count > 1 && nextNode != null) {
                    try {
                        //int[] checkResult = this.nextNode.Check(this.url);
                        ExtraWorkWrapper extraCheck = this.nextNode.Check(this.url);
                        extraWork = extraCheck;
                        //extraWork.AddWork(nextNodeURL, checkResult[0], checkResult[1], checkResult[2], checkResult[3]);
                    }
                    catch (Exception ex) {
                        if (ex is RemotingException || ex is SocketException) {
                            neighborDied = true;

                            
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Worker {0} has died - Initiate CPR protocol", this.nextNodeURL);
                            Console.ResetColor();
                            RepairTopologyChain();
                            
                            myWork.addExtraWork(extraWork);

                            IList<KeyValuePair<string, int[]>> extraList = myWork.getExtraWork();
                           
                            #region debug
                            /*foreach (KeyValuePair<string, int[]> pair in extraList) {
                                int[] array = pair.Value;
                                Console.WriteLine("myWork - nodeURL {0}, lastSentByte {1}, lastByteToProcess {2}, numSplitsLeft {3}", pair.Key, array[0], array[1], array[2]);
                            }*/
                            #endregion

                            Thread downloadExtraWorkThread = new Thread(() => ProcessExtraJobs());
                            downloadExtraWorkThread.Start();  

                        }
                        else throw ex;
                    }
                }
            }
            TimerItem.Change(TIMEOUT, TIMEOUT);
        }

        public void FreezeW() {
            worker.ApplyFreeze();
            FreezeC();
        }

        public void FreezeC() {
            this.frozenComm = true;
        }

        public void UnfreezeW() {
            worker.ApplyUnfreeze();
            UnfreezeC();
        }

        public void UnfreezeC() {
            this.frozenComm = false;
        }

        private void RepairTopologyChain() {
            int deadNodeID = mod((this.topologyID + 1), totalNodes);
            string deadNodeURL = nextNodeURL;

            int newNextNodeID = mod((deadNodeID + 1), totalNodes);
            this.totalNodes--;

            string newNextNode = remoteWorkers[newNextNodeID];
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Repaired - NextNodeURL: " + newNextNode);
            Console.ResetColor();
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), newNextNode);
            nextNodeURL = newNextNode;

            deadNodes.Add(deadNodeURL);
            remoteWorkers.RemoveAt(deadNodeID);

            //Inform PuppetMasters of node failure
            IdLocationDelegate RemoteDel = new IdLocationDelegate(puppetMaster.RegisterLostWorker);
            RemoteDel.BeginInvoke(deadNodeID, deadNodeURL, null, null);

            //Update TopID if we detect a failure at start of the chain
            if (this.topologyID > deadNodeID)
                this.topologyID--;

            int stopID = mod((this.topologyID - 1), totalNodes);

            if (totalNodes > 1) {
                RepairTopologyDelegate RepairDel = new RepairTopologyDelegate(nextNode.RepairTopologyChain);
                RepairDel.BeginInvoke(stopID, deadNodeID, null, null);
            }

        }

        public void RepairTopologyChain(int stopID, int deadNodeID) {
            if (frozenComm)
                return;
            // remove failed nodes from list
            

            string deadNodeURL = remoteWorkers[deadNodeID];

            if (mod((this.topologyID-1), totalNodes) == deadNodeID)
                this.previousNodeURL = this.remoteWorkers[mod((this.topologyID-2), totalNodes)];

            totalNodes--;

            if (this.topologyID > deadNodeID)
                this.topologyID--;
            deadNodes.Add(remoteWorkers[deadNodeID]);
            remoteWorkers.RemoveAt(deadNodeID);

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Node Down: {0} - {1}", deadNodeID, deadNodeURL);
            Console.WriteLine("My previous node now is {0}", this.previousNodeURL);
            Console.ResetColor();

            if (this.topologyID != stopID) {
                RepairTopologyDelegate RemoteDel = new RepairTopologyDelegate(nextNode.RepairTopologyChain);
                RemoteDel.BeginInvoke(stopID, deadNodeID, null, null);
            }
        }

        // Just to check connection
        public ExtraWorkWrapper Check(string workerUrl) {
            if (frozenComm)
                throw new SocketException();

            RemoteWorkerInterface remoteWorkerConnector;

            if (workerUrl != this.previousNodeURL) {
                remoteWorkerConnector = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerUrl);
                ReconnectDelegate RemoteDel = new ReconnectDelegate(remoteWorkerConnector.Reconnect);
                RemoteDel.BeginInvoke(null, null);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Received message from presumed dead node");
                Console.ResetColor();
            }

            int numSplitsLeft = numberSplitsToProcess - this.splitsSent;

            #region heartbeatDebug
            /*Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Pinged by {0}", workerUrl);
            if(worker!= null)
                Console.WriteLine("Added to ExtraWork - previousNodeURL {0}, lastSentByte {1}, lastByteToProcess {2}, numSplitsLeft {3}", previousNodeURL, lastSentByte, lastByteToProcess,numSplitsLeft);
            Console.ResetColor();
            */
             #endregion 

            myWork.AddWork(this.url, lastSentByte, lastByteToProcess, numSplitsLeft, this.currentSplit);
            return myWork;
        }

        public void Reconnect() {
            Console.WriteLine("RECONNECT - Previous url {0}, URL:{1}", this.previousNodeURL, this.url);
            ConnectToChain(this.previousNodeURL, this.url);
        }


        public void PrintStatus() {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("==================CURRENT STATUS===================");
            if (worker != null) {
                Console.WriteLine("Processing line {0} /  {1} of split {2} which has {3} bytes", worker.GetCurrentLineProcess(), worker.GetTotalSplitLinesProcess(), this.currentSplit, this.currentSplitSize);
                Console.WriteLine("Received {0} total bytes to process in {1} splits", totalDataToProcess, numberSplitsToProcess);
            }
            else Console.WriteLine("No Job is currently being performed");
            Console.WriteLine("Worker ID: {0} TopologyID: {1} totalNodes: {2}", this.id, this.topologyID, this.totalNodes);
            Console.WriteLine("===================================================");
            Console.WriteLine("Remote Workers list:");
            foreach (string remoteWorker in remoteWorkers) {
                Console.WriteLine(remoteWorker);
            }
            Console.WriteLine("===================================================");
            Console.ResetColor();
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
