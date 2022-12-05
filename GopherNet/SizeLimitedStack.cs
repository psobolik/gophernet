using System;
using System.Collections;
using System.Collections.Generic;

namespace GopherNet
{
    // This is a stack that holds a limited number of items.
    // If pushing an item would exceed the stack's capacity, it 
    // removes the one at the bottom first. It's similar to the 
    // System.Collections.Generic.Stack class, but less complete.
    // Also, it's backed by a List instead of an array, so it
    // might be less efficient, too.
    public class SizeLimitedStack<T> : ICollection, IReadOnlyCollection<T>
    {
        private static readonly List<T> _theList;
        private readonly uint _sizeLimit;

        static SizeLimitedStack()
        {
            _theList = new List<T>();
        }

        public SizeLimitedStack(uint sizeLimit)
        {
            _sizeLimit = sizeLimit;
        }

        #region Interface implementations
        public int Count => _theList.Count;

        public object SyncRoot => throw new NotImplementedException();

        public bool IsSynchronized => false;

        public void CopyTo(Array array, int index)
        {
            _theList.ToArray().CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _theList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _theList.GetEnumerator();
        }
        #endregion Interface implementations

        public void Clear()
        {
            _theList.Clear();
        }

        public void Push(T item)
        {
            if (_theList.Count >= _sizeLimit)
            {
                _theList.RemoveAt(0);
            }
            _theList.Add(item);
        }

        public T Pop()
        {
            T result = Peek();
            _theList.RemoveAt(_theList.Count - 1);
            return result;
        }

        public T Peek()
        {
            if (_theList.Count >= 1)
            {
                return _theList[^1];
            }
            else
            {
                throw new InvalidOperationException("The stack is empty");
            }
        }

        public bool Contains(T item)
        {
            return _theList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _theList.CopyTo(array, arrayIndex);
        }

        public T[] ToArray()
        {
            return _theList.ToArray();
        }
    }
}
