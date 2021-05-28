//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public class SearchResult
    {
        public int TagCount { get; internal set; }
        public Dictionary<string, List<MatchedTag>> TagsByName { get; internal set; }
        public List<MatchedTag> Candidates { get; internal set; }
        public bool WasCandidateLimitExceeded { get; internal set; }

        public IEnumerable<MatchedTag> GetTags()
        {
            foreach (List<MatchedTag> matchedTags in TagsByName.Values)
            {
                foreach (MatchedTag matchedTag in matchedTags)
                    yield return matchedTag;
            }
        }

        public List<MatchedTag> GetTagsSortedByLocationInText()
        {
            var result = GetTags().ToList();
            result.Sort();
            return result;
        }

        internal SearchResult()
        {
        }
    }

    public class MatchedText
    {
        public ITextSource TextSource { get; }
        public TextLocation Start { get; }
        public TextLocation End { get; }

        internal MatchedText(ITextSource textSource, TextLocation start, TextLocation end)
        {
            TextSource = textSource;
            Start = start;
            End = end;
        }

        public string GetText()
        {
            string result = null;
            if (TextSource != null)
                result = TextSource.GetText(Start, End);
            return result;
        }

        public override string ToString()
        {
            return GetText();
        }
    }

    public sealed class MatchedTag : MatchedText, IComparable<MatchedTag>
    {
        public long Timestamp { get; }
        public string PatternFullName { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<MatchedText>> Extractions { get; }

        internal bool WasPassedToCallback { get; set; } // для режима FirstMatchOnly

        internal MatchedTag(long timestamp, string patternFullName,
            IReadOnlyDictionary<string, IReadOnlyList<MatchedText>> extractions,
            ITextSource textSource, TextLocation start, TextLocation end)
            : base(textSource, start, end)
        {
            Timestamp = timestamp;
            PatternFullName = patternFullName;
            Extractions = extractions;
        }

        public int CompareTo(MatchedTag other)
        {
            int result = Start.TokenNumber.CompareTo(other.Start.TokenNumber);
            if (result == 0)
                result = End.TokenNumber.CompareTo(other.End.TokenNumber);
            return result;
        }

        internal static readonly MatchedTagStartTokenNumberComparer StartTokenNumberComparer =
            new MatchedTagStartTokenNumberComparer();
    }

    internal class MatchedTagStartTokenNumberComparer : IComparer<MatchedTag>
    {
        public int Compare(MatchedTag x, MatchedTag y)
        {
            return x.Start.TokenNumber.CompareTo(y.Start.TokenNumber);
        }
    }
}
