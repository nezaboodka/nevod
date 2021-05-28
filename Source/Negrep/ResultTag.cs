//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod.Negrep
{
    public class ResultTag
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public List<MatchingLine> MatchingLines { get; set; }

        public ResultTag(string name, string source, MatchingLine matchingLine) : this(name, source)
        {
            MatchingLines.Add(matchingLine);
        }

        public ResultTag(string name, string source)
        {
            Name = name;
            Source = source;
            MatchingLines = new List<MatchingLine>();
        }
    }

    public struct MatchingLine
    {
        public long Start { get; set; }
        public long Length { get; set; }
        public List<MatchedPart> MatchedParts { get; set; }
        public long StartTokenNumber { get; }
        public long EndTokenNumber { get; }
        public string Text { get; set; }

        public MatchingLine(long start, long length, MatchedPart matchedPart)
            : this(start, length, matchedPart, startTokenNumber: -1, endTokenNumber: -1)
        {
        }

        public MatchingLine(long start, long length, MatchedPart matchedPart, long startTokenNumber,
            long endTokenNumber)
        {
            Start = start;
            Length = length;
            MatchedParts = new List<MatchedPart>();
            MatchedParts.Add(matchedPart);
            StartTokenNumber = startTokenNumber;
            EndTokenNumber = endTokenNumber;
            Text = null;
        }
    }

    public struct MatchedPart
    {
        public long Start { get; set; }
        public long Length { get; set; }

        public MatchedPart(long start, long length)
        {
            Start = start;
            Length = length;
        }
    }
}
