using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacePMNR {
    public interface RemoteClientInterface {
       IList<string> getSplit(int begin, int end);
       void sendProcessedSplit(IList<KeyValuePair<string, string>> result);
    }

    public interface RemoteWorkerInterface {
        void JobMetaData(int numberSplits, int numLines, byte[] code, string className);
        void Broadcast(int remainingLines, int linesPerMachine, int linesPerSplit, byte[] code, string className);
        int[] Connect(string workerURL);
        void JoinBroadcast(int stopID, int previousNodeID);
        //void RemoveBroadcast();
        void SetNextNodeURL(string workerURL);
        void SetCurrentNextNodeURL(string url);
        void SetClientURL(string clientURL);
    }

    public interface RemotePuppetMasterInterface {
        void CreateWorker(int id, string serviceURL, string entryURL);
    }
}
