using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.ChannelServices;


namespace WorkerPMNR {
    class Worker {

        private int port;
        private TcpChannel channel;

        public Worker(int port) {
            this.port = port;
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteWorker), "Worker", WellKnownObjectMode.Singleton);
        }

        static void Main(string[] args) {

            int port = Int32.Parse(Console.ReadLine());
            Worker worker = new Worker(port);


        }

    }

    public class RemoteWorker : MarshalByRefObject {
       
    }

}
