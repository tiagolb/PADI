using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WorkerPMNR {
    class SplitPool<T> {

        private Queue<T> queue;
        private int elements;

        public SplitPool() {
            queue = new Queue<T>();
            elements = 0;
        }

        public SplitPool(int capacity) {
            queue = new Queue<T>(capacity);
            elements = 0;
        }

        public void Add(T o) {
            lock (this) {
                // TODO: Queremos limitar o tamanho da fila?
                queue.Enqueue(o);
                elements++;
                if (elements == 1) {
                    Monitor.Pulse(this);
                }
            }
        }

        public T Get() {
            T o;
            lock (this) {
                while (elements == 0) {
                    Monitor.Wait(this);
                }
                o = queue.Dequeue();
                elements--;
                // TODO: Queremos limitar o tamanho da fila?
            }
            return o;
        }

        public int getElements() {
            return elements;
        }
    }
}
