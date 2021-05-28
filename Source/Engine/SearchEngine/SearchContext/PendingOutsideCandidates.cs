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
    internal class PendingOutsideCandidates
    {
        private Dictionary<int, PendingOutsideCandidatesOfOuterPattern> fPendingCandidatesByOuterPattern { get; }

        public PendingOutsideCandidates()
        {
            fPendingCandidatesByOuterPattern = new Dictionary<int, PendingOutsideCandidatesOfOuterPattern>();
        }

        public void Reset()
        {
            fPendingCandidatesByOuterPattern.Clear();
        }

        public void AddPendingCandidate(OutsideCandidate candidate)
        {
            int key = (candidate.Expression as OutsideExpression).OuterPattern.ReferencedPattern.Id;
            PendingOutsideCandidatesOfOuterPattern list = fPendingCandidatesByOuterPattern.GetOrCreate(key);
            list.AddPendingCandidate(candidate);
        }

        public void AddOuterPatternCandidate(PatternCandidate patternCandidate)
        {
            int key = (patternCandidate.Expression as PatternExpression).Id;
            PendingOutsideCandidatesOfOuterPattern list = fPendingCandidatesByOuterPattern.GetOrCreate(key);
            list.AddOuterPatternCandidate(patternCandidate);
        }

        public void RejectAll()
        {
            foreach (PendingOutsideCandidatesOfOuterPattern list in fPendingCandidatesByOuterPattern.Values)
                list.RejectAll();
            fPendingCandidatesByOuterPattern.Clear();
        }

        public void ForEach(Action<RootCandidate> action)
        {
            foreach (PendingOutsideCandidatesOfOuterPattern list in fPendingCandidatesByOuterPattern.Values)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                    action(list[i].GetRootCandidate());
            }
        }

        public void TryMatchPendingOutsideCandidates(Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            foreach (KeyValuePair<int, PendingOutsideCandidatesOfOuterPattern> kv in fPendingCandidatesByOuterPattern)
            {
                long cleaningTokenNumber = cleaningTokenNumberPerPattern.GetValueOrDefault(kv.Key, long.MaxValue);
                kv.Value.TryMatchPendingOutsideCandidates(cleaningTokenNumber);
            }
        }
    }

    internal class PendingOutsideCandidatesOfOuterPattern
    {
        public List<OutsideCandidate> PendingCandidates { get; }  // сортировка по End.TokenNumber
        public List<PatternCandidate> MatchedCandidatesOfOuterPatterns { get; }   // сортировка по Start.TokenNumber

        public OutsideCandidate this[int index] => PendingCandidates[index];
        public int Count => PendingCandidates.Count;

        public PendingOutsideCandidatesOfOuterPattern()
        {
            PendingCandidates = new List<OutsideCandidate>();
            MatchedCandidatesOfOuterPatterns = new List<PatternCandidate>();
        }

        public void AddPendingCandidate(OutsideCandidate candidate)
        {
            if (PendingCandidates.Count == 0)
                PendingCandidates.Add(candidate);
            else
            {
                int lastIndex = PendingCandidates.Count - 1;
                OutsideCandidate lastCandidate = PendingCandidates[lastIndex];
                if (candidate.End.TokenNumber >= lastCandidate.End.TokenNumber)
                    PendingCandidates.Add(candidate);
                else
                {
                    int pos = PendingCandidates.BinarySearch(candidate, Candidate.EndTokenNumberComparer);
                    if (pos < 0)
                        pos = ~pos;
                    PendingCandidates.Insert(pos, candidate);
                }
            }
        }

        public void AddOuterPatternCandidate(PatternCandidate patternCandidate)
        {
            RejectPendingCandidates(patternCandidate);
            if (MatchedCandidatesOfOuterPatterns.Count == 0)
                MatchedCandidatesOfOuterPatterns.Add(patternCandidate);
            else
            {
                int lastIndex = MatchedCandidatesOfOuterPatterns.Count - 1;
                PatternCandidate lastCandidate = MatchedCandidatesOfOuterPatterns[lastIndex];
                if (patternCandidate.Start.TokenNumber >= lastCandidate.Start.TokenNumber)
                    MatchedCandidatesOfOuterPatterns.Add(patternCandidate);
                else
                {
                    int pos = MatchedCandidatesOfOuterPatterns.BinarySearch(patternCandidate,
                        Candidate.StartTokenNumberComparer);
                    if (pos < 0)
                        pos = ~pos;
                    MatchedCandidatesOfOuterPatterns.Insert(pos, patternCandidate);
                }
            }
        }

        public void RejectPendingCandidates(PatternCandidate patternCandidate)
        {
            long patternStartTokenNumber = patternCandidate.Start.TokenNumber;
            int pos = PendingCandidates.BinarySearch(patternStartTokenNumber,
                Candidate.EndTokenNumberComparer.CompareToLong);
            if (pos < 0)
                pos = ~pos;
            else if (pos > 0)
            {
                // Поиск начала диапазона
                pos = pos - 1;
                while (pos >= 0 && patternStartTokenNumber == PendingCandidates[pos].End.TokenNumber)
                    pos--;
                pos++;
            }
            long patternEndTokenNumber = patternCandidate.End.TokenNumber;
            int count = PendingCandidates.Count;
            int i = pos;
            int j = pos;
            while (i < count)
            {
                OutsideCandidate candidate = PendingCandidates[i];
                if (candidate.Start.TokenNumber <= patternEndTokenNumber)
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
            PendingCandidates.RemoveRange(j, i - j);
        }

        public void RejectAll()
        {
            for (int i = 0, n = PendingCandidates.Count; i < n; i++)
            {
                OutsideCandidate candidate = PendingCandidates[i];
                candidate.RejectTarget();
            }
            PendingCandidates.Clear();
            MatchedCandidatesOfOuterPatterns.Clear();
        }

        public void TryMatchPendingOutsideCandidates(long cleaningTokenNumber)
        {
            int i = 0;
            int j = 0;
            int count = MatchedCandidatesOfOuterPatterns.Count;
            while (i < count)
            {
                PatternCandidate outerPattern = MatchedCandidatesOfOuterPatterns[i];
                RejectPendingCandidates(outerPattern);
                if (outerPattern.End.TokenNumber < cleaningTokenNumber)
                    MatchedCandidatesOfOuterPatterns[i] = null;
                else
                {
                    if (j != i)
                    {
                        MatchedCandidatesOfOuterPatterns[j] = MatchedCandidatesOfOuterPatterns[i];
                        MatchedCandidatesOfOuterPatterns[i] = null;
                    }
                    j++;
                }
                i++;
            }
            MatchedCandidatesOfOuterPatterns.RemoveRange(j, i - j);
            count = PendingCandidates.Count;
            j = 0;
            while (j < count && PendingCandidates[j].End.TokenNumber < cleaningTokenNumber)
            {
                PendingCandidates[j].OnOuterPatternReject();
                PendingCandidates[j] = null;
                j++;
            }
            PendingCandidates.RemoveRange(0, j);
        }
    }
}
