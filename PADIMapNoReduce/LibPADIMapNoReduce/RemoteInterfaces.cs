using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        void FreezeW();
        void FreezeC();
        void UnfreezeW();
        void UnfreezeC();
        void PrintStatus();
        ExtraWorkWrapper Check(string url);
        void RepairTopologyChain(int stopID, int deadNodeID);
        void Reconnect();
    }

    public interface RemotePuppetMasterInterface {
        void CreateWorker(int id, string serviceURL, string entryURL);
        void Connect(string newPuppetMasterURL);
        void ReceiveWorker(int workerID, string serviceURL);
        void PrintWorkerStatus();
        void ReceiveJobTracker(KeyValuePair<int, string> jobtracker);
        void RegisterLostWorker(int id, string workerURL);
        void RefreshWorkersOnFail(int id, string workerURL);
    }

    [Serializable]
    public class ExtraWorkWrapper {
        IList<KeyValuePair<string, int[]>> extraWorkList;

        public ExtraWorkWrapper() {
            this.extraWorkList = new List<KeyValuePair<string, int[]>>();
        }

        public void AddWork(string url, int lastByte, int lastSentByte, int numSplits, int splitId) {
            KeyValuePair<string, int[]> toAdd = new KeyValuePair<string, int[]>(url, new int[] { lastByte, lastSentByte, numSplits, splitId });
            KeyValuePair<string, int[]> toRemove = new KeyValuePair<string, int[]>(null, new int[] { 0, 0, 0, 0 });

            foreach (KeyValuePair<string, int[]> p in extraWorkList) {
                if (p.Key == toAdd.Key) {
                    toRemove = p;
                    break;
                }
            }
            extraWorkList.Remove(toRemove);
            extraWorkList.Add(toAdd);
        }

        public IList<KeyValuePair<string, int[]>> getExtraWork() {
            return this.extraWorkList;
        }

        public void addExtraWork(ExtraWorkWrapper extra) {
            IList<KeyValuePair<string, int[]>> extraList = extra.getExtraWork();

            foreach(KeyValuePair<string, int[]> pair in extraList) {
                this.extraWorkList.Add(pair);
            }
        }
    }
}