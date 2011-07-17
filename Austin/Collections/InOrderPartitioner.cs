using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Austin.Collections
{
    /// <summary>
    /// An OrderablePartitioner that takes items from a list in order, instead of spliting the list into
    /// chunks like the default paritioner.  Works with with a LazyCollection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InOrderPartitioner<T> : OrderablePartitioner<T>
    {
        private IList<T> baseList;
        /// <summary>
        /// Creates a new InOrderPartitioner that will split up the given list.
        /// </summary>
        /// <param name="list">The list to partition.</param>
        public InOrderPartitioner(IList<T> list)
            : base(true, false, true)
        {
            this.baseList = list;
        }

        /// <summary>
        /// Creates <paramref name="partitionCount"/> enumerators for the underling list
        /// </summary>
        /// <param name="partitionCount"></param>
        /// <returns></returns>
        public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
        {
            return new IteratorHolder(baseList, partitionCount).iterators;
        }

        class IteratorHolder
        {
            private int index = -1;
            public List<IEnumerator<KeyValuePair<long, T>>> iterators;
            public IteratorHolder(IList<T> list, int count)
            {
                iterators = new List<IEnumerator<KeyValuePair<long, T>>>(count);
                for (int i = 0; i < count; i++)
                {
                    iterators.Add(new Iterator(this, list));
                }
            }

            public int NextIndex()
            {
                return Interlocked.Increment(ref this.index);
            }
        }

        class Iterator : IEnumerator<KeyValuePair<long, T>>
        {
            private IteratorHolder holder;
            private IList<T> baseList;
            private int index = -1;

            public Iterator(IteratorHolder holder, IList<T> list)
            {
                this.holder = holder;
                this.baseList = list;
            }

            public KeyValuePair<long, T> Current
            {
                get { return new KeyValuePair<long, T>(index, baseList[index]); }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                this.index = holder.NextIndex();
                return this.index < baseList.Count;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}
