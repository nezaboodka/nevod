//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal static class SearchContextTelemetryExtension
    {
        public static PatternStats GetAllCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            stats.AddActiveCandidates(context.ActiveCandidates);
            IEnumerable<RootCandidate> waitingCandidates = GetWaitingCandidates(context.WaitingCandidates);
            stats.AddWaitingCandidates(waitingCandidates);
            IEnumerable<RootCandidate> pendingCandidates = GetPendingCandidates(context);
            stats.AddPendingCandidates(pendingCandidates, skipActiveAndWaiting: true);
            return stats;
        }

        public static object GetAllCandidatesGrouped(this SearchContext context)
        {
            var result = new
            {
                Active = GetActiveCandidates(context),
                Waiting = GetWaitingCandidates(context),
                Pending = new
                {
                    Having = GetPendingHavingCandidates(context),
                    Inside = GetPendingInsideCandidates(context),
                    Outside = GetPendingOutsideCandidates(context)
                }
            };
            return result;
        }

        public static PatternStats GetActiveCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            stats.AddActiveCandidates(context.ActiveCandidates);
            return stats;
        }

        public static PatternStats GetWaitingCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            IEnumerable<RootCandidate> waitingCandidates = GetWaitingCandidates(context.WaitingCandidates);
            stats.AddWaitingCandidates(waitingCandidates);
            return stats;
        }

        public static PatternStats GetAllPendingCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            IEnumerable<RootCandidate> pendingCandidates = GetPendingCandidates(context);
            stats.AddPendingCandidates(pendingCandidates, skipActiveAndWaiting: false);
            return stats;
        }

        public static PatternStats GetPendingHavingCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            HashSet<RootCandidate> pendingCandidates = new HashSet<RootCandidate>();
            GetPendingHavingCandidates(context.PendingHavingCandidates, pendingCandidates);
            stats.AddPendingCandidates(pendingCandidates, skipActiveAndWaiting: false);
            return stats;
        }

        public static PatternStats GetPendingInsideCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            HashSet<RootCandidate> pendingCandidates = new HashSet<RootCandidate>();
            GetPendingInsideCandidates(context.PendingInsideCandidates, pendingCandidates);
            stats.AddPendingCandidates(pendingCandidates, skipActiveAndWaiting: false);
            return stats;
        }

        public static PatternStats GetPendingOutsideCandidates(this SearchContext context)
        {
            var stats = new PatternStats();
            HashSet<RootCandidate> pendingCandidates = new HashSet<RootCandidate>();
            GetPendingOutsideCandidates(context.PendingOutsideCandidates, pendingCandidates);
            stats.AddPendingCandidates(pendingCandidates, skipActiveAndWaiting: false);
            return stats;
        }

        // Internal

        private static IEnumerable<RootCandidate> GetWaitingCandidates(this WaitingCandidatesIndex waitingCandidateIndex)
        {
            HashSet<RootCandidate> result = new HashSet<RootCandidate>();
            waitingCandidateIndex.ForEach((RootCandidate x) => result.Add(x));
            return result;
        }

        private static IEnumerable<RootCandidate> GetPendingCandidates(SearchContext context)
        {
            HashSet<RootCandidate> result = new HashSet<RootCandidate>();
            GetPendingHavingCandidates(context.PendingHavingCandidates, result);
            GetPendingInsideCandidates(context.PendingInsideCandidates, result);
            GetPendingOutsideCandidates(context.PendingOutsideCandidates, result);
            return result;
        }

        private static void GetPendingHavingCandidates(PendingHavingCandidates pendingHavingCandidates,
            HashSet<RootCandidate> result)
        {
            pendingHavingCandidates.ForEach((RootCandidate x) => result.Add(x));
        }

        private static void GetPendingInsideCandidates(PendingInsideCandidates pendingInsideCandidates,
            HashSet<RootCandidate> result)
        {
            pendingInsideCandidates.ForEach((RootCandidate x) => result.Add(x));
        }

        private static void GetPendingOutsideCandidates(PendingOutsideCandidates pendingOutsideCandidates,
            HashSet<RootCandidate> result)
        {
            pendingOutsideCandidates.ForEach((RootCandidate x) => result.Add(x));
        }
    }

    internal class PatternStats
    {
        public Dictionary<string, HashSet<PatternCandidate>> PatternCandidatesByPatternName;
        public Dictionary<string, HashSet<ExceptionStubCandidate>> ExceptionCandidatesByPatternName;
        public PatternStats()
        {
            PatternCandidatesByPatternName = new Dictionary<string, HashSet<PatternCandidate>>();
            ExceptionCandidatesByPatternName = new Dictionary<string, HashSet<ExceptionStubCandidate>>();
        }

        public void AddActiveCandidates(ActiveCandidates activeCandidates)
        {
            activeCandidates.ForEach((RootCandidate rootCandidate) =>
            {
                if (!rootCandidate.IsCompletedOrWaiting)
                    AddRootCandidate(rootCandidate);
            });
        }

        public void AddWaitingCandidates(IEnumerable<RootCandidate> waitingCandidates)
        {
            foreach (var rootCandidate in waitingCandidates)
            {
                AddRootCandidate(rootCandidate);
            }
        }

        public void AddPendingCandidates(IEnumerable<RootCandidate> pendingCandidates, bool skipActiveAndWaiting)
        {
            foreach (var rootCandidate in pendingCandidates)
            {
                if (!skipActiveAndWaiting || rootCandidate.IsCompleted)
                    AddRootCandidate(rootCandidate);
            }
        }

        public void AddRootCandidate(RootCandidate rootCandidate)
        {
            if (rootCandidate is PatternCandidate pattern)
                AddPatternCandidate(pattern);
            else if (rootCandidate is ExceptionStubCandidate exception)
                AddExceptionCandidate(exception);
        }

        public void AddPatternCandidate(PatternCandidate candidate)
        {
            var patternExpression = candidate.Expression as PatternExpression;
            string key = GetRootKey(patternExpression);
            PatternCandidatesByPatternName.GetOrCreate(key).Add(candidate);
        }

        public void AddExceptionCandidate(ExceptionStubCandidate candidate)
        {
            var rootExpression = (RootExpression)candidate.Expression;
            string key = GetRootKey(rootExpression);
            ExceptionCandidatesByPatternName.GetOrCreate(key).Add(candidate);
        }

        public string GetRootKey(RootExpression rootExpression)
        {
            string key = $"({rootExpression.Id}) {(rootExpression as PatternExpression)?.Name ?? "[anonymous]"}";
            return key;
        }
    }

    internal static class CandidateExtension
    {
        public static string GetTreeTextInfo(this Candidate candidate)
        {
            var result = new StringBuilder();
            RootCandidate root = candidate.GetRootCandidate();
            Candidate current = root;
            while (current.CurrentEventObserver != null)
                current = current.CurrentEventObserver;
            string candidateInfo;
            while (current != root)
            {
                candidateInfo = current.GetTextInfo();
                result.Insert(0, candidateInfo);
                if (current == candidate)
                    result.Insert(0, " [this] ");
                string relationLine;
                if (current.ParentCandidate != null)
                {
                    current = current.ParentCandidate;
                    relationLine = " <== ";
                }
                else
                {
                    current = current.TargetParentCandidate;
                    relationLine = " <-- ";
                }
                result.Insert(0, relationLine);
            }
            candidateInfo = current.GetTextInfo();
            result.Insert(0, candidateInfo);
            return result.ToString();
        }

        public static string GetTextInfo(this Candidate candidate)
        {
            string info;
            switch (candidate)
            {
                case PatternCandidate c:
                    info = $"{c.PatternId},'{(c.Expression as PatternExpression).Name}'"
                        + $"{(c.IsCompleted && !c.IsFinalMatch && !c.IsRejected ? " Completed" : "")}"
                        + $"{(c.IsFinalMatch ? " FinalMatch" : "")}{(c.IsRejected ? " Rejected" : "")}"
                        + $"{(c.IsWaiting ? " Waiting" : "")}";
                    break;
                case PatternReferenceCandidate c:
                    info = $"{(c.Expression as PatternReferenceExpression).ReferencedPattern.Name}";
                    break;
                default:
                    info = string.Empty;
                    break;
            }
            return $"{candidate.GetType().Name}{(string.IsNullOrEmpty(info) ? "" : $"({info})")}";
        }
    }
}
