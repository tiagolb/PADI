using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacePMNR {
    public interface RemoteClientInterface {
       //IList<string> getSplit(int begin, int end);
        byte[] getSplits(int begin, int end, int extraSplit);
        //void sendProcessedSplit(IList<KeyValuePair<string, string>> result, int splitId);
        void sendProcessedSplit(string result, int splitId);
    }

    public interface RemoteWorkerInterface {
        void JobMetaData(int numberSplits, int numLines, byte[] code, string className);
        void Broadcast(int stopID, int begin, int firstSplit, int bytesPerSplit, int extraBytes, int splitsPerMachine, int extraSplits, byte[] code, string className);
        int[] Connect(string workerURL);
        void JoinBroadcast(int stopID, int previousNodeID);
        //void RemoveBroadcast();
        void SetNextNodeURL(string workerURL);
        void SetClientURL(string clientURL);
        void BroadcastClient(int stopId, string clientURL);
        void Slow(int secondsDelay);
    }

    public interface RemotePuppetMasterInterface {
        void CreateWorker(int id, string serviceURL, string entryURL);
        void Connect(string newPuppetMasterURL);
        void ReceiveWorker(int workerID, string serviceURL);
        void PrintWorkerStatus();
    }
}
