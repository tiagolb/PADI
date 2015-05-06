using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacePMNR {
    public interface RemoteClientInterface {
        byte[] getSplits(int begin, int end, int extraSplit, int id);
        void sendProcessedSplit(string result, int splitId);
    }

    public interface RemoteWorkerInterface {
        void JobMetaData(int numberSplits, int numLines, byte[] code, string className);
        void Broadcast(int stopID, int begin, int firstSplit, int bytesPerSplit, int extraBytes, int splitsPerMachine, int extraSplits, byte[] code, string className);
        int[] Connect(string workerURL);
        void JoinBroadcast(int stopID, int previousNodeID, int newTopologyId, string newWorker);
        void SetNextNodeURL(string workerURL, IList<string> remoteWorkers);
        void SetClientURL(string clientURL);
        void BroadcastClient(int stopId, string clientURL);
        void Slow(int secondsDelay);
        void PrintStatus();
    }

    public interface RemotePuppetMasterInterface {
        void CreateWorker(int id, string serviceURL, string entryURL);
        void Connect(string newPuppetMasterURL);
        void ReceiveWorker(int workerID, string serviceURL);
        void PrintWorkerStatus();
        void ReceiveJobTracker(KeyValuePair<int, string> jobtracker);
    }
}