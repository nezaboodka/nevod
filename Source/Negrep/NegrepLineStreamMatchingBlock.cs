//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Nezaboodka.Text.Parsing;
using Nezaboodka.Nevod;

namespace Nezaboodka.Nevod.Negrep
{
    internal class NegrepLineStreamMatchingBlock
    {
        private readonly NegrepConfig _config;
        private Dictionary<string, ResultTag> _resultTags;
        private SourceTextInfo _sourceTextInfo;

        private const int BufferSizeInChars = StreamTextSource.DefaultBufferSizeInChars;

        public NegrepLineStreamMatchingBlock(NegrepConfig config)
        {
            _config = config;
        }

        public (SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags) Match(SourceTextInfo sourceTextInfo)
        {
            _sourceTextInfo = sourceTextInfo;
            _resultTags = new Dictionary<string, ResultTag>();
            var searchEngine = new TextSearchEngine(_config.PatternPackage);
            ITextSource textSource = new LineStreamTextSource(sourceTextInfo.SourceStream,
                sourceTextInfo.ShouldCloseReader, BufferSizeInChars);
            searchEngine.Search(textSource, resultCallback: ProcessMatchedTag);
            return (sourceTextInfo: sourceTextInfo, resultTags: _resultTags.Values);
        }

        public void ProcessMatchedTag(SearchEngine _, MatchedTag matchedTag)
        {
            string tagName = matchedTag.PatternFullName;
            var matchingLine = GetMatchingLine(matchedTag);

            if (!_resultTags.TryGetValue(tagName, out ResultTag tag))
            {
                tag = new ResultTag(tagName, _sourceTextInfo.Path);
                _resultTags.Add(tagName, tag);
            }
            int matchingLineIndex = tag.MatchingLines.FindIndex(line =>
                line.Start == matchingLine.Start && line.Length == matchingLine.Length);
            if (matchingLineIndex != -1)
                tag.MatchingLines[matchingLineIndex].MatchedParts.AddRange(matchingLine.MatchedParts);
            else
            {
                var startLocation = new TextLocation(matchingLine.StartTokenNumber, position: 0, length: 0);
                var endLocation = new TextLocation(matchingLine.EndTokenNumber, position: 0, length: 0);
                ITextSource textSource = matchedTag.TextSource;
                matchingLine.Text = textSource.GetText(startLocation, endLocation);
                tag.MatchingLines.Add(matchingLine);
            }
        }

        private MatchingLine GetMatchingLine(MatchedTag matchedTag)
        {
            long matchedPartStart = matchedTag.Start.Position;
            long matchedPartEnd = matchedTag.End.Position + matchedTag.End.Length;
            long matchedPartLength = matchedPartEnd - matchedPartStart;

            TextLocation previousLineBreak = matchedTag.Start.Context.PreviousLineBreak;
            TextLocation currentLineBreak = matchedTag.End.Context.CurrentLineBreak;

            long matchingLineStart = previousLineBreak.Position + previousLineBreak.Length;
            long matchingLineStartTokenNumber = previousLineBreak.TokenNumber + 1;
            long matchingLineEnd = currentLineBreak.Position;
            long matchingLineEndTokenNumber = currentLineBreak.TokenNumber - 1;
            if (matchedTag.End.TokenNumber == currentLineBreak.TokenNumber)
            {
                matchingLineEnd += currentLineBreak.Length;
                matchingLineEndTokenNumber += 1;
            }
            long matchingLineLength = matchingLineEnd - matchingLineStart;

            return new MatchingLine(matchingLineStart, matchingLineLength,
                new MatchedPart(matchedPartStart, matchedPartLength),
                matchingLineStartTokenNumber, matchingLineEndTokenNumber);
        }
    }
}
