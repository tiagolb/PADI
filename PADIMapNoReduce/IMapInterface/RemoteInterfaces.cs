using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacePMNR {
    interface RemoteClientInterface {
        public IList<KeyValuePair<string, string>> getSplit(int begin, int end);
        public void sendProcessedSplit(IList<KeyValuePair<string, string>> result);
    }

    interface RemoteWorkerInterface {
        public void JobMetaData(int numberSplits, int numLines, byte[] code, string className);
        public void Broadcast(int remainingLines, int linesPerMachine, int linesPerSplit, byte[] code, string className);
        public int Connect(string workerURL);
        public void JoinBroadcast(int id);
        public void RemoveBroadcast();
    }
}
