using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Reflection;
using PADIMapNoReduce;
using InterfacePMNR;


namespace WorkerPMNR {
    class WorkerPMNR {


        static void Main(string[] args) {
            Console.Write("Enter port here: ");
            int port = Int32.Parse(Console.ReadLine());


            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemoteWorker remoteWorker = new RemoteWorker();
            RemotingServices.Marshal(remoteWorker, "Worker", typeof(RemoteWorker));
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
                        this.classObj = (IMapper) Activator.CreateInstance(type);
                    }
                }
            }
        }

        public IList<KeyValuePair<string, string>> processSplit(IList<string> split){
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            
            foreach(string s in split)
                result.Concat(processLine(s));

            return result;
        }

        private IList<KeyValuePair<string, string>> processLine(string fileLine){
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


        public void setClientURL(string url) {
           client = (RemoteClientInterface)Activator.GetObject(typeof(RemoteClientInterface), url);
        }


        public void Broadcast(int remainingLines, int linesPerMachine, int linesPerSplit, byte[] code, string className) {
             IList<IList<string>> splits = new List<IList<string>>();
             IList<KeyValuePair<string, string>> splitProcessingResult;
            int begin = id*linesPerMachine;
            remainingLines -= linesPerMachine;

            if (remainingLines > 0)
                nextNode.Broadcast(remainingLines, linesPerMachine, linesPerSplit, code, className);
            else
                linesPerMachine += remainingLines;

            int end = begin + linesPerMachine;

            IList<string> split = client.getSplit(begin, end);

            while (split.Count != 0) {
                if (split.Count < linesPerSplit)
                    splits.Add(new List<string>(split.Take(split.Count)));
                else
                    splits.Add(new List<string>(split.Take(linesPerSplit)));
            }

            foreach (IList<string> s in splits) {
                splitProcessingResult = worker.processSplit(split);
                client.sendProcessedSplit(splitProcessingResult);
            }

        } //TODO: Test TAKE -- is worker.processSplit blocking the RemoteWorker? 



        public void JobMetaData(int numberSplits, int numLines, byte[] code, string className) {
            worker = new Worker(code, className);
           int linesPerMachine = numLines/totalNodes;
           int linesPerSplit = numLines/numberSplits;
            
           Broadcast(numLines, linesPerMachine, linesPerSplit, code, className);
        }


        public void connectToChain() {
        //TODO: Connect() to entry point
        //      set own worker reference as next in chain

        }


        public int Connect(string workerURL) {
            if(nextNode != null)
                nextNode.JoinBroadcast(id);

            worker = (RemoteWorkerInterface)Activator.GetObject(typeof(RemoteWorkerInterface), workerURL);
            return id++;
        }

        public void JoinBroadcast(int id){
            if(id != this.id)
                nextNode.JoinBroadcast(id);
            //this.id++ ??
        }
    }

}
