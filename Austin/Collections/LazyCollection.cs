using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Austin.Collections
{
    /// <summary>
    /// A lazy loading collection.  Useful if you only need to access a collection
    /// of objects sequentially and loading each object takes some time.
    /// Items are loaded in the background sequentially.  Items are also loaded on demand.
    /// This collection is readonly once created and a number of methods are not supported.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <typeparam name="TSource">The type of the source information.</typeparam>
    public class LazyCollection<T, TSource> : ICollection<T>, IDisposable
    {
        /// <summary>
        /// A method that loads an object for a LazyCollection.
        /// </summary>
        /// <param name="source">Information to load an object with.</param>
        /// <returns>The loaded object.</returns>
        public delegate T LazyLoad(TSource source);

        private TSource[] sources;
        private T[] values;
        private Exception[] exceptions;
        private object[] locks;
        private LazyLoad loader;
        private bool isDisposed = false;

        /// <summary>
        /// Creates a new LazyCollection.
        /// </summary>
        /// <param name="sources">Sources to be passed to <paramref name="loader"/></param>.
        /// <param name="loader">A method that will load each object.</param>
        public LazyCollection(IList<TSource> sources, LazyLoad loader)
            : this(sources, loader, CancellationToken.None)
        {
        }

        /// <summary>
        /// Creates a new LazyCollection.
        /// </summary>
        /// <param name="sources">Sources to be passed to <paramref name="loader"/></param>.
        /// <param name="loader">A method that will load each object.</param>
        /// <param name="ct">A token to cancel background loading.</param>
        public LazyCollection(IList<TSource> sources, LazyLoad loader, CancellationToken ct)
        {
            this.sources = new TSource[sources.Count];
            this.values = new T[sources.Count];
            this.locks = new object[sources.Count];
            this.exceptions = new Exception[sources.Count];
            this.loader = loader;
            for (int i = 0; i < sources.Count; i++)
            {
                this.sources[i] = sources[i];
                this.values[i] = default(T);
                this.locks[i] = new object();
            }

            ThreadPool.QueueUserWorkItem(threadFunc);

            ct.Register(() => this.isDisposed = true);
        }

        #region Public
        /// <summary>
        /// The number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return this.sources.Length; }
        }

        /// <summary>
        /// Return true if this collection is readonly, false otherwise.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Return an enumerator for this collection.  Items will be loaded on-demand if necessary.
        /// </summary>
        /// <returns>An new IEnumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new LazyIterator(this);
        }

        /// <summary>
        /// Return an enumerator for this collection.  Items will be loaded on-demand if necessary.
        /// </summary>
        /// <returns>An new IEnumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new LazyIterator(this);
        }

        /// <summary>
        /// Disposes the collection.  Just stops the background loading of objects.
        /// </summary>
        public void Dispose()
        {
            this.isDisposed = true;
        }
        #endregion

        #region Private
        private bool hasValue(int i)
        {
            return values[i] != null;
        }

        private void threadFunc(object unused)
        {
            for (int i = 0; i < sources.Length && !isDisposed; i++)
            {
                load(i);
            }
        }

        private void load(int i)
        {
            if (!hasValue(i))
            {
                lock (locks[i])
                {
                    if (!hasValue(i))
                    {
                        values[i] = loader(sources[i]);
                    }
                }
            }
        }
        #endregion

        private class LazyIterator : IEnumerator<T>
        {
            private LazyCollection<T, TSource> coll;
            private int currentLocation = -1;

            public LazyIterator(LazyCollection<T, TSource> c)
            {
                this.coll = c;
            }

            public T Current
            {
                get
                {
                    coll.load(currentLocation);
                    return coll.values[currentLocation];
                }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    try
                    {
                        coll.load(currentLocation);
                        return coll.values[currentLocation];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public bool MoveNext()
            {
                currentLocation++;
                return currentLocation < coll.sources.Length;
            }

            public void Reset()
            {
                currentLocation = -1;
            }
        }

        #region Unsupported Methods
        /// <summary>
        /// NOT SUPPORTED!
        /// </summary>
        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NOT SUPPORTED!
        /// </summary>
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NOT SUPPORTED!
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NOT SUPPORTED!
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NOT SUPPORTED!
        /// </summary>
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
