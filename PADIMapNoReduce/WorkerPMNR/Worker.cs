using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Reflection;


namespace WorkerPMNR {
    class Worker {

        private int port;
        private TcpChannel channel;

        public Worker(int port) {
            this.port = port;
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteWorker), "Worker", WellKnownObjectMode.Singleton);
            Console.WriteLine("<enter> para sair...");
            Console.ReadLine();
        }

        static void Main(string[] args) {

            Console.Write("Enter port here: ");
            int port = Int32.Parse(Console.ReadLine());
            Worker worker = new Worker(port);
        }

    }

    public class RemoteWorker : MarshalByRefObject {

        public void JobMetaData(int numberSplits, int fileSizeBytes) {
            //Console.WriteLine("Bytes in File: {0}", fileSizeBytes);
        }

        public bool SendMapper(byte[] code, string className) {
            Assembly assembly = Assembly.Load(code);
            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes()) {
                if (type.IsClass == true) {
                    if (type.FullName.EndsWith("." + className)) {
                        // create an instance of the object
                        object ClassObj = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        object[] args = new object[] { "testValue" };
                        object resultObject = type.InvokeMember("Map",
                          BindingFlags.Default | BindingFlags.InvokeMethod,
                               null,
                               ClassObj,
                               args);
                        IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>)resultObject;
                        Console.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result) {
                            Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }
                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
        }
    }

}
