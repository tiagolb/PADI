using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacePMNR {
    public interface RemoteClientInterface {
       //IList<string> getSplit(int begin, int end);
        byte[] getSplit(int begin, int end);
        //void sendProcessedSplit(IList<KeyValuePair<string, string>> result, int splitId);
        void sendProcessedSplit(string result, int splitId);
    }

    public interface RemoteWorkerInterface {
        void JobMetaData(int numberSplits, int numLines, byte[] code, string className);
        // Tentar passar apenas beginPos, fileSize e nSplits
        //void BroadCast(int beginPos, int bytesPerSplit, int splitsPerMachine, int extraBytes, int extraSplits, byte[] code, string className); 
        void Broadcast(int remainingLines, int linesPerMachine, int linesPerSplit, byte[] code, string className);
        int[] Connect(string workerURL);
        void JoinBroadcast(int stopID, int previousNodeID);
        //void RemoveBroadcast();
        void SetNextNodeURL(string workerURL);
        void SetCurrentNextNodeURL(string url);
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
