//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class CandidateArray<T> : IEnumerable<T> where T : class
    {
        public T[] Elements;
        public int[] CountPerCategory;
        public int LimitOfCategory;
        public int Count;
        public int RemovedCount;
        public int EnumerationCount;
        public int ShrinkThreshold;
        public int ShrinkReallocationThreshold;

        public T this[int index] => Elements[index];
        public int Capacity => Elements.Length;

        public CandidateArray(int totalLimit, int categoryCount, int limitOfCategory, int shrinkThreshold,
            int shrinkReallocationThreshold)
        {
            Elements = new T[totalLimit];
            CountPerCategory = new int[categoryCount];
            LimitOfCategory = limitOfCategory;
            ShrinkThreshold = shrinkThreshold;
            ShrinkReallocationThreshold = shrinkReallocationThreshold;
        }

        public bool TryAdd(int elementCategory, T element)
        {
            if (Count < Elements.Length)
            {
                int countInCategory = CountPerCategory[elementCategory];
                if (countInCategory < LimitOfCategory)
                {
                    Elements[Count] = element;
                    Count++;
                    CountPerCategory[elementCategory] = countInCategory + 1;
                    return true;
                }
            }
            return false;
        }

        public void MarkRemoved(int elementCategory, int elementPosition)
        {
            Elements[elementPosition] = null;
            CountPerCategory[elementCategory] -= 1;
            RemovedCount++;
        }

        public void Clear()
        {
            Count = 0;
            RemovedCount = 0;
            EnumerationCount = 0;
            Array.Clear(CountPerCategory, 0, CountPerCategory.Length);
        }

        public int GetEnumerationCount()
        {
            EnumerationCount = Count;
            return EnumerationCount;
        }

        public void ForEach(Action<int, T> action)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                T element = Elements[i];
                if (element != null)
                    action(i, element);
            }
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                T element = Elements[i];
                if (element != null)
                    action(element);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                T element = Elements[i];
                if (element != null)
                    yield return element;
            }
        }

        public T FirstOrNull()
        {
            T result = null;
            for (int i = 0, n = Count; i < n; i++)
            {
                result = Elements[i];
                if (result != null)
                    break;
            }
            return result;
        }

        public void Shrink()
        {
            if (RemovedCount == Count)
            {
                Count = 0;
                RemovedCount = 0;
                EnumerationCount = 0;
            }
            else if (RemovedCount >= ShrinkThreshold)
            {
                if (RemovedCount < ShrinkReallocationThreshold)
                    RemoveNullElements();
                else
                    RecreateWithoutNullElements();
            }
        }

        // Internal

        private void RemoveNullElements()
        {
            EnumerationCount = 0;
            int i = 0;
            while (i < Count && Elements[i] != null)
                i++;
            int j = i;
            while (i < Count && j < Count)
            {
                while (j < Count && Elements[j] == null)
                    j++;
                while (j < Count && Elements[j] != null)
                {
                    Elements[i] = Elements[j];
                    i++;
                    j++;
                }
            }
            Array.Clear(Elements, i, Count - i);
            Count = i;
            RemovedCount = 0;
        }

        private void RecreateWithoutNullElements()
        {
            EnumerationCount = 0;
            var elementsCopy = new T[Elements.Length];
            int newCount = 0;
            for (int i = 0; i < Count; i++)
            {
                T element = Elements[i];
                if (element != null)
                {
                    elementsCopy[newCount] = element;
                    newCount++;
                }
            }
            Elements = elementsCopy;
            Count = newCount;
            RemovedCount = 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
