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
        //private string[] split;

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

        public IList<KeyValuePair<string, string>> processSplit(string split) {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            int index;
            int newLineSize = Environment.NewLine.Length;
            IList<string> lines = new List<string>();
            while (split.Length > 0) {
                index = split.IndexOf(Environment.NewLine);
                if (index > 0) {
                    lines.Add(split.Substring(0, index));
                    split = split.Substring(index + newLineSize);
                }
                else {
                    lines.Add(split);
                    split = "";
                }
            }

            foreach (string line in lines) {
                //Console.WriteLine("Line: " + line);
                /*foreach (KeyValuePair<string, string> pair in processLine(line)) {
                    Console.WriteLine("Pro Line: " + pair.Key + " " + pair.Value);
                }*/
                result = result.Concat(processLine(line)).ToList();
            }
            return result;
        }

        private IList<KeyValuePair<string, string>> processLine(string fileLine) {
            object[] args = new object[] { fileLine };
            object resultObject = type.InvokeMember("Map",
                   BindingFlags.Default | BindingFlags.InvokeMethod,
                   null,
                   classObj,
                   args);
            IList<KeyValuePair<string, string>> r = (IList<KeyValuePair<string, string>>)resultObject;
            /*foreach (KeyValuePair<string, string> pair in r) {
                Console.WriteLine("Count: " + pair.Key +" " + pair.Value);
            }*/
            return (IList<KeyValuePair<string, string>>)resultObject;
        }

        public void ApplyDelay(int secondsDelay) {
            System.Threading.Thread.Sleep(1000 * secondsDelay);
        }
    }


    public class RemoteWorker : MarshalByRefObject, RemoteWorkerInterface {
        private string url;
        private int totalNodes;
        private int id;
        private int topologyID;
        private int numberSplits;
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


        public void SetCurrentNextNodeURL(string url) {
            nextNodeURL = url;
        }

        public void SetClientURL(string clientURL) {
            if(nextNode != null) {
                int stopID = mod((this.topologyID - 1), totalNodes);
                BroadcastClient(stopID, clientURL);
            }
            client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), clientURL);
        }

        public void Slow(int secondsDelay) {
            //worker.ApplyDelay(secondsDelay);
            //if (workerThread != null) {}
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
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
        }

        private int mod(int x, int m) {
            return (x % m + m) % m;
        }


        private IList<string> getLinesFromBytes(byte[] bytes, int bytesPerSplit, bool first) {
            IList<string> lines = new List<string>();
            string splitsString = System.Text.Encoding.ASCII.GetString(bytes);
            int beginPos;
            int lengthOfSplit;
            string split;
            int newLineSize = Environment.NewLine.Length;

            if (first) {
                beginPos = 0;
            }
            else {
                beginPos = splitsString.IndexOf(Environment.NewLine) + newLineSize;
            }
            splitsString = splitsString.Substring(beginPos);
            int length = splitsString.Length;
            while (length > 0) {
                // Stop condition
                if (length < bytesPerSplit) {
                    lines.Add(splitsString);
                    return lines;
                }

                // new line after end of split
                if (bytesPerSplit >= length / 2)  //Heuristic
                    lengthOfSplit = splitsString.IndexOf(Environment.NewLine, bytesPerSplit - bytesPerSplit / 2);
                else
                    lengthOfSplit = splitsString.IndexOf(Environment.NewLine, bytesPerSplit);

                Console.WriteLine("Comprimento do Split: " + lengthOfSplit);

                if (lengthOfSplit > 0) {
                    split = splitsString.Substring(0, lengthOfSplit);
                    lengthOfSplit += newLineSize;
                    splitsString = splitsString.Substring(lengthOfSplit);
                }
                else {
                    // Last line
                    split = splitsString.Substring(0);
                    splitsString = "";
                }
                lines.Add(split);

                length = splitsString.Length;
                Console.WriteLine("O que falta: " + length);
            }
            return lines;
        }

       /* private IList<string> getLinesFromBytes(byte[] bytes, int bytesPerSplit, bool first) {
            IList<string> lines = new List<string>();
            string splitsString = System.Text.Encoding.ASCII.GetString(bytes);
            int beginPos;
            int lengthOfSplit;
            string split;
            int newLineSize = Environment.NewLine.Length;

            if (first) {
                beginPos = 0;
            }
            else {
                beginPos = splitsString.IndexOf(Environment.NewLine) + newLineSize;
            }
            splitsString = splitsString.Substring(beginPos);
            int length = splitsString.Length;
            while (length > 0) {
                // Stop condition
                if (length < bytesPerSplit) {
                    lines.Add(splitsString);
                    return lines;
                }

                // new line after end of split
                if(bytesPerSplit >= length/2)  //Heuristic
                    lengthOfSplit = splitsString.IndexOf(Environment.NewLine, bytesPerSplit-bytesPerSplit/2);  
                else
                    lengthOfSplit = splitsString.IndexOf(Environment.NewLine, bytesPerSplit);
                
                Console.WriteLine("Comprimento do Split: " +lengthOfSplit);

                if (lengthOfSplit > 0) {
                    split = splitsString.Substring(0, lengthOfSplit);
                    lengthOfSplit += newLineSize;
                    splitsString = splitsString.Substring(lengthOfSplit);
                }
                else {
                    // Last line
                    split = splitsString.Substring(0);
                    splitsString = "";
                }
                lines.Add(split);

                length = splitsString.Length;
                Console.WriteLine("O que falta: " + length);
            }
            return lines;
        }
        */
        public void Broadcast(int remainingBytes, int bytesPerMachine, int bytesPerSplit, byte[] code, string className) {
            Console.WriteLine("remainingBytes: "+ remainingBytes+". bytesPerMachine: " + bytesPerMachine + ". BytesPerSplit: " + bytesPerSplit);
            int begin = topologyID * bytesPerMachine;
            bool first = (topologyID == 0);
            int splitsPerMachine = bytesPerMachine / bytesPerSplit;
            worker = new Worker(code, className);

            //ID of first split, ID starts with 1
            int firstSplit = topologyID * splitsPerMachine + 1;
            
            // We want to request all our lines plus the next split
            int bytesToRequest = bytesPerMachine + bytesPerSplit;

            if (remainingBytes < bytesToRequest) {
                bytesToRequest = remainingBytes;
            } else {
                remainingBytes -= bytesPerMachine;
                nextNode.Broadcast(remainingBytes, bytesPerMachine, bytesPerSplit, code, className);
            }

            workerThread = new Thread(() => processSplitsThread(bytesPerSplit, begin, first, splitsPerMachine, firstSplit, bytesToRequest));
            workerThread.Start();
        }

        private void processSplitsThread(int bytesPerSplit, int begin, bool first, int splitsPerMachine, int firstSplit, int bytesToRequest) {
            IList<KeyValuePair<string, string>> processedSplit;
            int end = begin + bytesToRequest;
            //Console.WriteLine("Before Client");
            byte[] bytes = client.getSplit(begin, end);
            //Console.WriteLine("After Client: " + System.Text.Encoding.ASCII.GetString(bytes));
            //Console.WriteLine("Split: " + System.Text.Encoding.ASCII.GetString(bytes));

            IList<string> splits = getLinesFromBytes(bytes, bytesPerSplit, first);
            Console.WriteLine(splits.Count);
            //Console.WriteLine("BeforeSend");
            int splitId;
            for (int i = 0; (i < splitsPerMachine && i < splits.Count); i++) {
                splitId = firstSplit + i;
                string result = "";
                processedSplit = worker.processSplit(splits[i]);
                //Console.WriteLine(processedSplit.ToString());
                //Console.WriteLine("Split: " + processedSplit[0].Key + "\r\n" + processedSplit[0].Value + "\r\n");
                foreach (KeyValuePair<string, string> pair in processedSplit) {
                    result += pair.Key + " : " + pair.Value + Environment.NewLine;
                }
                Console.WriteLine("Result: " + result);
                Console.WriteLine("SentSplit " + splitId);
                client.sendProcessedSplit(result, splitId);
            }
        }

        public void JobMetaData(int numberSplits, int numLines, byte[] code, string className) {
            Console.WriteLine("JobMetaData");
            //worker = new Worker(code, className);
            this.numberSplits = numberSplits;
            int linesPerMachine = numLines / totalNodes;
            int linesPerSplit = numLines / numberSplits;

            Broadcast(numLines, linesPerMachine, linesPerSplit, code, className);
        }



        public void ConnectToChain(string entryPointURL, string newNodeURL) {
            RemoteWorkerInterface remoteWorkerConnector =
                (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), entryPointURL);

            int[] connectionData = remoteWorkerConnector.Connect(newNodeURL);
            this.topologyID = connectionData[0];
            this.totalNodes = connectionData[1];
            Console.WriteLine("ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
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
            nextNode.SetCurrentNextNodeURL(nextNodeURL);
            nextNodeURL = workerURL;
            Console.WriteLine("ID: " + this.id + " TopologyID: " + this.topologyID + " totalNodes: " + this.totalNodes);
            //Returns to the new Node it's ID and Total Nodes in the ring
            return new int[] { topologyID + 1, totalNodes };
        }



        public void JoinBroadcast(int stopID, int previousNodeID) {
            //The node receiving the broadcast updates the number of Total Nodes in the System
            // and computes it's new ID

            totalNodes++;
            this.topologyID = mod((previousNodeID + 1), totalNodes);
            Console.WriteLine("ID: " + this.topologyID + " totalNodes: " + this.totalNodes);
            if (stopID != this.topologyID) {
                nextNode.JoinBroadcast(stopID, this.topologyID);
            }
        }
    }

}
