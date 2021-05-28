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
    internal class PendingHavingCandidates
    {
        private Dictionary<int, PendingHavingCandidatesOfInnerPattern> fPendingCandidatesByInnerPattern { get; }

        public PendingHavingCandidates()
        {
            fPendingCandidatesByInnerPattern = new Dictionary<int, PendingHavingCandidatesOfInnerPattern>();
        }

        public void Reset()
        {
            fPendingCandidatesByInnerPattern.Clear();
        }

        public void AddPendingCandidate(HavingCandidate candidate)
        {
            int key = (candidate.Expression as HavingExpression).InnerContent.ReferencedPattern.Id;
            PendingHavingCandidatesOfInnerPattern list = fPendingCandidatesByInnerPattern.GetOrCreate(key);
            list.AddPendingCandidate(candidate);
        }

        public void AddInnerPatternCandidate(PatternCandidate patternCandidate)
        {
            int key = (patternCandidate.Expression as PatternExpression).Id;
            PendingHavingCandidatesOfInnerPattern list = fPendingCandidatesByInnerPattern.GetOrCreate(key);
            list.AddInnerPatternCandidate(patternCandidate);
        }

        public void RejectAll()
        {
            foreach (PendingHavingCandidatesOfInnerPattern list in fPendingCandidatesByInnerPattern.Values)
                list.RejectAll();
            fPendingCandidatesByInnerPattern.Clear();
        }

        public void ForEach(Action<RootCandidate> action)
        {
            foreach (PendingHavingCandidatesOfInnerPattern list in fPendingCandidatesByInnerPattern.Values)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                    action(list[i].GetRootCandidate());
            }
        }

        public void TryMatchPendingHavingCandidates(Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            foreach (KeyValuePair<int, PendingHavingCandidatesOfInnerPattern> kv in fPendingCandidatesByInnerPattern)
            {
                long cleaningTokenNumber = cleaningTokenNumberPerPattern.GetValueOrDefault(kv.Key, long.MaxValue);
                kv.Value.TryMatchPendingHavingCandidates(cleaningTokenNumber);
            }
        }
    }

    internal class PendingHavingCandidatesOfInnerPattern
    {
        public List<HavingCandidate> PendingCandidates { get; } // сортировка по Start.TokenNumber
        public List<PatternCandidate> MatchedCandidatesOfInnerPatterns { get; } // сортировка по Start.TokenNumber

        public HavingCandidate this[int index] => PendingCandidates[index];
        public int Count => PendingCandidates.Count;

        public PendingHavingCandidatesOfInnerPattern()
        {
            PendingCandidates = new List<HavingCandidate>();
            MatchedCandidatesOfInnerPatterns = new List<PatternCandidate>();
        }

        public void AddPendingCandidate(HavingCandidate candidate)
        {
            if (PendingCandidates.Count == 0)
                PendingCandidates.Add(candidate);
            else
            {
                int lastIndex = PendingCandidates.Count - 1;
                HavingCandidate lastCandidate = PendingCandidates[lastIndex];
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

        public void AddInnerPatternCandidate(PatternCandidate patternCandidate)
        {
            MatchPendingCandidates(patternCandidate);
            if (MatchedCandidatesOfInnerPatterns.Count == 0)
                MatchedCandidatesOfInnerPatterns.Add(patternCandidate);
            else
            {
                int lastIndex = MatchedCandidatesOfInnerPatterns.Count - 1;
                PatternCandidate lastCandidate = MatchedCandidatesOfInnerPatterns[lastIndex];
                if (patternCandidate.Start.TokenNumber >= lastCandidate.Start.TokenNumber)
                    MatchedCandidatesOfInnerPatterns.Add(patternCandidate);
                else
                {
                    int pos = MatchedCandidatesOfInnerPatterns.BinarySearch(patternCandidate,
                        Candidate.StartTokenNumberComparer);
                    if (pos < 0)
                        pos = ~pos;
                    MatchedCandidatesOfInnerPatterns.Insert(pos, patternCandidate);
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
            else if (pos >= 0)
            {
                // Поиск конца диапазона
                pos = pos + 1;
                while (pos < PendingCandidates.Count && patternStartTokenNumber == PendingCandidates[pos].Start.TokenNumber)
                    pos++;
            }
            long patternEndTokenNumber = patternCandidate.End.TokenNumber;
            int count = pos;
            int i = 0;
            int j = 0;
            while (i < count)
            {
                HavingCandidate candidate = PendingCandidates[i];
                if (candidate.End.TokenNumber >= patternEndTokenNumber)
                {
                    candidate.OnInnerPatternMatch();
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
            PendingCandidates.RemoveRange(j, i - j);
        }

        public void RejectAll()
        {
            for (int i = 0, n = PendingCandidates.Count; i < n; i++)
            {
                HavingCandidate candidate = PendingCandidates[i];
                candidate.RejectTarget();
            }
            PendingCandidates.Clear();
        }

        public void TryMatchPendingHavingCandidates(long cleaningTokenNumber)
        {
            int i = 0;
            int j = 0;
            int count = MatchedCandidatesOfInnerPatterns.Count;
            while (i < count)
            {
                PatternCandidate outerPattern = MatchedCandidatesOfInnerPatterns[i];
                MatchPendingCandidates(outerPattern);
                if (outerPattern.End.TokenNumber < cleaningTokenNumber)
                    MatchedCandidatesOfInnerPatterns[i] = null;
                else
                {
                    if (j != i)
                    {
                        MatchedCandidatesOfInnerPatterns[j] = MatchedCandidatesOfInnerPatterns[i];
                        MatchedCandidatesOfInnerPatterns[i] = null;
                    }
                    j++;
                }
                i++;
            }
            MatchedCandidatesOfInnerPatterns.RemoveRange(j, i - j);
            count = PendingCandidates.Count;
            j = 0;
            while (j < count && PendingCandidates[j].End.TokenNumber < cleaningTokenNumber)
            {
                PendingCandidates[j].OnInnerPatternReject();
                PendingCandidates[j] = null;
                j++;
            }
            PendingCandidates.RemoveRange(0, j);
        }
    }
}
