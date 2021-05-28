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
    public class CandidateList<T> : IEnumerable<T> where T : class
    {
        public List<T> Elements;
        public List<T> NewElements;
        public int RemovedCount;
        public bool IsBeingEnumerated;

        public T this[int index] { get { return Elements[index]; } set { Elements[index] = value; } }

        public CandidateList()
        {
            Elements = new List<T>();
            NewElements = new List<T>();
        }

        public void Add(T element)
        {
            if (IsBeingEnumerated)
                NewElements.Add(element);
            else
                Elements.Add(element);
        }

        public void Clear()
        {
            Elements.Clear();
            NewElements.Clear();
            RemovedCount = 0;
        }

        public int BeginEnumeration()
        {
            if (!IsBeingEnumerated)
            {
                MergeNewElements();
                IsBeingEnumerated = true;
            }
            return Elements.Count;
        }

        public void EndEnumeration()
        {
            IsBeingEnumerated = false;
        }

        public void UpdateOrRemoveEach(int count, Func<T, bool> action)
        {
            BeginEnumeration();
            int i = 0;
            int j = 0;
            while (i < count)
            {
                T element = Elements[i];
                if (element != null)
                {
                    bool shouldRemove = action(element);
                    if (shouldRemove)
                        Elements[i] = null;
                    else
                    {
                        if (j != i)
                        {
                            Elements[j] = Elements[i];
                            Elements[i] = null;
                        }
                        j++;
                    }
                }
                i++;
            }
            Elements.RemoveRange(j, i - j);
            EndEnumeration();
        }

        public void ForEach(Action<T> action)
        {
            int count = BeginEnumeration();
            for (int i = 0; i < count; i++)
            {
                T element = Elements[i];
                if (element != null)
                    action(element);
            }
            EndEnumeration();
        }

        public T FirstOrNull()
        {
            T result = null;
            for (int i = 0, n = Elements.Count; i < n; i++)
            {
                result = Elements[i];
                if (result != null)
                    break;
            }
            if (result == null)
            {
                for (int i = 0, n = NewElements.Count; i < n; i++)
                {
                    result = NewElements[i];
                    if (result != null)
                        break;
                }
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int count = BeginEnumeration();
            for (int i = 0; i < count; i++)
            {
                T element = Elements[i];
                if (element != null)
                    yield return element;
            }
            EndEnumeration();
        }

        // Internal

        private void MergeNewElements()
        {
            if (NewElements.Count > 0)
            {
                Elements.AddRange(NewElements);
                NewElements.Clear();
            }
        }

        private void RemoveNullElements()
        {
            if (RemovedCount > 0)
            {
                int i = 0;
                while (i < Elements.Count && Elements[i] != null)
                    i++;
                int j = i;
                while (i < Elements.Count)
                {
                    while (i < Elements.Count && Elements[i] == null)
                        i++;
                    while (i < Elements.Count && Elements[i] != null)
                    {
                        Elements[j] = Elements[i];
                        i++;
                        j++;
                    }
                }
                Elements.RemoveRange(j, i - j);
            }
            RemovedCount = 0;
        }

        private void RecreateWithoutNullElements()
        {
            var elementsCopy = new List<T>();
            for (int i = 0, n = Elements.Count; i < n; i++)
            {
                T element = Elements[i];
                if (element != null)
                    elementsCopy.Add(element);
            }
            Elements = elementsCopy;
            RemovedCount = 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
