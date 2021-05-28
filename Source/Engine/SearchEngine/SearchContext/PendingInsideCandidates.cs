//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class PendingInsideCandidates
    {
        private Dictionary<int, PendingInsideCandidatesOfOuterPattern> fPendingCandidatesByOuterPattern { get; }

        public PendingInsideCandidates()
        {
            fPendingCandidatesByOuterPattern = new Dictionary<int, PendingInsideCandidatesOfOuterPattern>();
        }

        public void Reset()
        {
            fPendingCandidatesByOuterPattern.Clear();
        }

        public void AddPendingCandidate(InsideCandidate candidate)
        {
            int key = (candidate.Expression as InsideExpression).OuterPattern.ReferencedPattern.Id;
            PendingInsideCandidatesOfOuterPattern list = fPendingCandidatesByOuterPattern.GetOrCreate(key);
            list.AddPendingCandidate(candidate);
        }

        public void MatchPendingCandidates(PatternCandidate patternCandidate)
        {
            int key = (patternCandidate.Expression as PatternExpression).Id;
            if (fPendingCandidatesByOuterPattern.TryGetValue(key, out PendingInsideCandidatesOfOuterPattern list)
                && list.Count > 0)
            {
                list.MatchPendingCandidates(patternCandidate);
            }
        }

        public void RejectAll()
        {
            foreach (PendingInsideCandidatesOfOuterPattern list in fPendingCandidatesByOuterPattern.Values)
                list.RejectAll();
            fPendingCandidatesByOuterPattern.Clear();
        }

        public void ForEach(Action<RootCandidate> action)
        {
            foreach (PendingInsideCandidatesOfOuterPattern list in fPendingCandidatesByOuterPattern.Values)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                    action(list[i].GetRootCandidate());
            }
        }

        public void TryRejectPendingInsideCandidates(Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            foreach (KeyValuePair<int, PendingInsideCandidatesOfOuterPattern> kv in fPendingCandidatesByOuterPattern)
            {
                long cleaningTokenNumber = cleaningTokenNumberPerPattern.GetValueOrDefault(kv.Key, long.MaxValue);
                kv.Value.TryRejectPendingInsideCandidates(cleaningTokenNumber);
            }
        }
    }

    internal class PendingInsideCandidatesOfOuterPattern
    {
        public List<InsideCandidate> PendingCandidates { get; }   // сортировка по Start.TokenNumber

        public InsideCandidate this[int index] => PendingCandidates[index];
        public int Count => PendingCandidates.Count;

        public PendingInsideCandidatesOfOuterPattern()
        {
            PendingCandidates = new List<InsideCandidate>();
        }

        public void AddPendingCandidate(InsideCandidate candidate)
        {
            if (PendingCandidates.Count == 0)
                PendingCandidates.Add(candidate);
            else
            {
                int lastIndex = PendingCandidates.Count - 1;
                InsideCandidate lastCandidate = PendingCandidates[lastIndex];
                if (candidate.Start.TokenNumber >= lastCandidate.Start.TokenNumber)
                    PendingCandidates.Add(candidate);
                else
                {
                    int pos = PendingCandidates.BinarySearch(candidate, Candidate.StartTokenNumberComparer);
                    if (pos < 0)
                        pos = ~pos;
                    PendingCandidates.Insert(pos, candidate);
                }
            }
        }

        public void MatchPendingCandidates(PatternCandidate patternCandidate)
        {
            long patternStartTokenNumber = patternCandidate.Start.TokenNumber;
            int pos = PendingCandidates.BinarySearch(patternStartTokenNumber,
                Candidate.StartTokenNumberComparer.CompareToLong);
            if (pos < 0)
                pos = ~pos;
            else if (pos > 0)
            {
                // Поиск начала диапазона
                pos = pos - 1;
                while (pos >= 0 && patternStartTokenNumber == PendingCandidates[pos].Start.TokenNumber)
                    pos--;
                pos++;
            }
            long patternEndTokenNumber = patternCandidate.End.TokenNumber;
            int count = PendingCandidates.Count;
            int i = pos;
            int j = pos;
            while (i < count && PendingCandidates[i].Start.TokenNumber <= patternEndTokenNumber)
            {
                InsideCandidate candidate = PendingCandidates[i];
                if (candidate.End.TokenNumber <= patternEndTokenNumber)
                {
                    candidate.OnOuterPatternMatch();
                    PendingCandidates[i] = null;
                }
                else
                {
                    if (j != i)
                    {
                        PendingCandidates[j] = PendingCandidates[i];
                        PendingCandidates[i] = null;
                    }
                    j++;
                }
                i++;
            }
            // Удалить i - j элементов, т.к. конец списка может быть не достигнут.
            PendingCandidates.RemoveRange(j, i - j);
        }

        public void RejectAll()
        {
            for (int i = 0, n = PendingCandidates.Count; i < n; i++)
            {
                InsideCandidate candidate = PendingCandidates[i];
                candidate.RejectTarget();
            }
            PendingCandidates.Clear();
        }

        public void TryRejectPendingInsideCandidates(long cleaningTokenNumber)
        {
            int count = PendingCandidates.Count;
            int i = 0;
            while (i < count && PendingCandidates[i].Start.TokenNumber < cleaningTokenNumber)
            {
                PendingCandidates[i].OnOuterPatternReject();
                PendingCandidates[i] = null;
                i++;
            }
            PendingCandidates.RemoveRange(0, i);
        }
    }
}
