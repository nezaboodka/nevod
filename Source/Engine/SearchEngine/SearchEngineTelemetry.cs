//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class CandidateInfo
    {
        internal Candidate Candidate { get; }
        public long CandidateId { get; }
        public int PatternId { get; }
        public int TextSourceId { get; }
        
        public long StartTokenNumber { get; }
        public long EndTokenNumber { get; set; }
        public long CreationTokenNumber { get; }


        internal CandidateInfo(Candidate candidate, long candidateId, int patternId, long startTokenNumber, 
            long creationTokenNumber, int textSourceId)
        {
            Candidate = candidate;
            CandidateId = candidateId;
            PatternId = patternId;
            StartTokenNumber = startTokenNumber;
            CreationTokenNumber = creationTokenNumber;
            TextSourceId = textSourceId;
        }
    }

    public class SearchEngineTelemetry
    {
        private long fCurrentCandidateId;
        private int fCurrentTextSourceId;
        
        private long fCurrentTokenNumber;

        private readonly bool fCollectTelemetry;
        private readonly Dictionary<RootCandidate, CandidateInfo> fTelemetry = new Dictionary<RootCandidate, CandidateInfo>();

        public IReadOnlyCollection<CandidateInfo> Telemetry => fTelemetry.Values;

        internal SearchEngineTelemetry(bool collectTelemetry)
        {
            fCollectTelemetry = collectTelemetry;
        }

        internal void TrackStart(RootCandidate candidate)
        {
            if (fCollectTelemetry)
            {
                var ci = new CandidateInfo(candidate, fCurrentCandidateId++, candidate.PatternId, 
                    candidate.Start.TokenNumber, fCurrentTokenNumber, fCurrentTextSourceId);
                fTelemetry[candidate] = ci;
            }
        }

        internal void TrackEnd(RootCandidate candidate)
        {
            if (fCollectTelemetry)
            {
                if (fTelemetry.TryGetValue(candidate, out CandidateInfo candidateInfo))
                    candidateInfo.EndTokenNumber = fCurrentTokenNumber;
            }
        }

        internal void SetToken(Token token)
        {
            if (fCollectTelemetry)
                fCurrentTokenNumber = token.Location.TokenNumber;
        }

        public void Reset()
        {
            if (fCollectTelemetry)
            {
                foreach (CandidateInfo ci in Telemetry)
                {
                    if (ci.EndTokenNumber == 0)
                        ci.EndTokenNumber = fCurrentTokenNumber;
                }
            }
        }

        public void ResetTextSource()
        {
            if (fCollectTelemetry)
            {
                Reset();
                fCurrentTextSourceId++;
            }
        }

        public string ToCsv()
        {
            var csv = new StringBuilder();
            csv.AppendLine("cid,pid,tid,pn,sn,en,cn");
            foreach (CandidateInfo ci in Telemetry)
            {
                if (ci.EndTokenNumber == 0)
                    ci.EndTokenNumber = fCurrentTokenNumber;
                string name;
                if (ci.Candidate.Expression.Syntax is PatternSyntax patternSyntax)
                    name = patternSyntax.FullName;
                else
                    name = ci.Candidate.Expression.GetType().Name;
                var line = $"{ci.CandidateId},{ci.PatternId},{ci.TextSourceId},{name},{ci.StartTokenNumber},{ci.EndTokenNumber},{ci.CreationTokenNumber}";
                csv.AppendLine(line);
            }

            return csv.ToString();
        }
    }
}
