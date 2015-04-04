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


namespace WorkerPMNR {
    class WorkerPMNR {


        static void Main(string[] args) {
            string host = "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            Console.Write("Enter port here: ");
            int port = Int32.Parse(Console.ReadLine());
            string newNodeURL = "tcp://" + host + ":" + port + "/Worker";

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemoteWorker remoteWorker = new RemoteWorker(port);
            RemotingServices.Marshal(remoteWorker, "Worker", typeof(RemoteWorker));

            Console.WriteLine("Insert entry point port");
            string entryPointURL = Console.ReadLine();

            if (entryPointURL != "")
                remoteWorker.ConnectToChain("tcp://"+host+":"+entryPointURL+"/Worker", newNodeURL);
           
            Console.WriteLine("<enter> para sair...");
            Console.ReadLine();
        }

    }

    public class Worker {
        private Type type;
        private IMapper classObj;
        private string[] split;

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
                result.Concat(processLine(line));
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
            return (IList<KeyValuePair<string, string>>)resultObject;
        }
    }


    public class RemoteWorker : MarshalByRefObject, RemoteWorkerInterface {
        private string url;
        private int totalNodes;
        private int id;
        private int linesPerMachine;
        private int linesPerSplit;
        private Worker worker;
        private RemoteWorkerInterface nextNode;
        private RemoteClientInterface client;
        private string nextNodeURL;

        public RemoteWorker(int port) {
            string host =  "" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            this.url = "tcp://" + host+":" + port + "/Worker";
            this.totalNodes = 1;
            this.nextNodeURL = this.url;
        }

        public void SetCurrentNextNodeURL(string url) {
            nextNodeURL = url;
        }

        public void SetClientURL(string clientURL) {
            client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), url);
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
                beginPos = splitsString.IndexOf(Environment.NewLine);
            }
            int length = splitsString.Length;
            while (length > 0) {
                // Stop condition
                if (length < bytesPerSplit) {
                    lines.Add(splitsString);
                    return lines;
                }

                // new line after end of split
                lengthOfSplit = splitsString.IndexOf(Environment.NewLine, bytesPerSplit);

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
            }
            return lines;
        }

        public void Broadcast(int remainingBytes, int bytesPerMachine, int bytesPerSplit, byte[] code, string className) {
            int begin = id * bytesPerMachine;
            bool first = id == 0;
            
            // We want to request all our lines plus the next split
            int bytesToRequest = bytesPerMachine + bytesPerSplit;

            if (remainingBytes < bytesToRequest) {
                bytesToRequest = remainingBytes;
            }
            else {
                remainingBytes -= bytesPerMachine;
                nextNode.Broadcast(remainingBytes, bytesPerMachine, bytesPerSplit, code, className);
            }
            int end = begin + bytesToRequest;

            byte[] bytes = client.getSplit(begin, end);

            IList<string> splits = getLinesFromBytes(bytes, bytesPerSplit, first);

            foreach (string s in splits) {
                worker.processSplit(s);
            }


            /*IList<IList<string>> splits = new List<IList<string>>();
            IList<KeyValuePair<string, string>> splitProcessingResult;
            int begin = id * linesPerMachine;
            remainingLines -= linesPerMachine;

            if (remainingLines > 0) {
                nextNode.Broadcast(remainingLines, linesPerMachine, linesPerSplit, code, className);
                linesPerMachine += linesPerSplit;
            }
            else
                linesPerMachine = remainingLines;

            int end = begin + linesPerMachine;

            //IList<string> split = client.getSplit(begin, end);
            byte[] split = client.getSplit(begin, end);

            while (split.Count != 0) {
                if (split.Count < linesPerSplit)
                    splits.Add(new List<string>(split.Take(split.Count)));
                else
                    splits.Add(new List<string>(split.Take(linesPerSplit)));
            }

            foreach (IList<string> s in splits) {
                splitProcessingResult = worker.processSplit(split);
                client.sendProcessedSplit(splitProcessingResult);
            }*/

        } //TODO: Test TAKE -- is worker.processSplit blocking the RemoteWorker? 




        public void JobMetaData(int numberSplits, int numLines, byte[] code, string className) {
            worker = new Worker(code, className);
            int linesPerMachine = numLines / totalNodes;
            int linesPerSplit = numLines / numberSplits;

            Broadcast(numLines, linesPerMachine, linesPerSplit, code, className);
        }



        public void ConnectToChain(string entryPointURL, string newNodeURL) {
            RemoteWorkerInterface remoteWorkerConnector =
                (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), entryPointURL);

            int[] connectionData = remoteWorkerConnector.Connect(newNodeURL);
            this.id = connectionData[0];
            this.totalNodes = connectionData[1];
            //Console.WriteLine("ID: " + this.id + " totalNodes: " + this.totalNodes);
        }



        public int[] Connect(string workerURL) {
            this.totalNodes++;

            if (nextNode != null)
            {   //Compute the ID of the node that shall stop broadcasting
                int stopID = mod((this.id - 1), totalNodes);
                nextNode.JoinBroadcast(stopID, this.id+1);
            }

            //Update nextNodes references, both for the new node and the entry point node
            nextNode = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            nextNode.SetNextNodeURL(nextNodeURL);
            nextNode.SetCurrentNextNodeURL(nextNodeURL);
            nextNodeURL = workerURL;
            //Console.WriteLine("ID: " + this.id + " totalNodes: " + this.totalNodes);
            //Returns to the new Node it's ID and Total Nodes in the ring
            return new int[] { id + 1, totalNodes };
        }



        public void JoinBroadcast(int stopID, int previousNodeID) {
            //The node receiving the broadcast updates the number of Total Nodes in the System
            // and computes it's new ID

            totalNodes++;
            this.id = mod((previousNodeID + 1), totalNodes);
            //Console.WriteLine("ID: " + this.id + " totalNodes: " + this.totalNodes);
            if (stopID != this.id) {
                nextNode.JoinBroadcast(stopID, this.id);
            }
        }
    }

}
