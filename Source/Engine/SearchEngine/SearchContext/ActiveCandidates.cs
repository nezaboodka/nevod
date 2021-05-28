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
    internal class ActiveCandidates
    {
        public List<RootCandidate> Elements;
        public List<RootCandidate> NewElements;
        public bool IsBeingEnumerated;

        public ActiveCandidates()
        {
            Elements = new List<RootCandidate>();
            NewElements = new List<RootCandidate>();
        }

        public void Reset()
        {
            Clear();
            IsBeingEnumerated = false;
        }

        public void Add(RootCandidate element)
        {
            if (IsBeingEnumerated)
                NewElements.Add(element);
            else
                Elements.Add(element);
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

        public void UpdateOrRemoveEach(Func<RootCandidate, bool> action)
        {
            int count = BeginEnumeration();
            int i = 0;
            int j = 0;
            while (i < count)
            {
                RootCandidate root = Elements[i];
                if (root != null)
                {
                    bool shouldRemove = action(root);
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

        public void ForEach(Action<RootCandidate> action)
        {
            int count = BeginEnumeration();
            for (int i = 0; i < count; i++)
            {
                RootCandidate root = Elements[i];
                if (root != null)
                    action(root);
            }
            EndEnumeration();
        }

        public void RejectAll()
        {
            int count = BeginEnumeration();
            for (int i = 0; i < count; i++)
            {
                RootCandidate root = Elements[i];
                if (root != null)
                    root.Reject();
            }
            EndEnumeration();
            Clear();
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

        private void Clear()
        {
            Elements.Clear();
            NewElements.Clear();
        }
    }
}
