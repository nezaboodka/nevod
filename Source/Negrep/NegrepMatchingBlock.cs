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

using NTTokenKind = Nezaboodka.Text.Parsing.TokenKind;

namespace Nezaboodka.Nevod.Negrep
{
    internal class NegrepMatchingBlock
    {
        private readonly NegrepConfig _config;

        public NegrepMatchingBlock(NegrepConfig config)
        {
            _config = config;
        }

        public (SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags) Match(SourceTextInfo sourceTextInfo)
        {
            var searchEngine = new TextSearchEngine(_config.PatternPackage);
            ParsedText parsedText = PlainTextParser.Parse(sourceTextInfo.SourceText);
            SearchResult searchResult = searchEngine.Search(parsedText);
            IEnumerable<ResultTag> resultTags = GetResultTags(searchResult, sourceTextInfo, parsedText.PlainTextTokens);
            return (sourceTextInfo: sourceTextInfo, resultTags: resultTags);
        }

        private IEnumerable<ResultTag> GetResultTags(SearchResult searchResult, SourceTextInfo sourceTextInfo, List<TokenReference> plainTextTokens)
        {
            var resultTags = new Dictionary<string, ResultTag>();

            foreach (var matchedTag in searchResult.GetTags())
            {
                string tagName = matchedTag.PatternFullName;
                var matchingLine = GetMatchingLine(plainTextTokens, matchedTag);

                if (!resultTags.TryGetValue(tagName, out ResultTag tag))
                {
                    tag = new ResultTag(tagName, sourceTextInfo.Path, matchingLine);
                    resultTags.Add(tagName, tag);
                }
                else
                {
                    int matchingLineIndex = tag.MatchingLines.FindIndex(line =>
                        line.Start == matchingLine.Start && line.Length == matchingLine.Length);
                    if (matchingLineIndex != -1)
                        tag.MatchingLines[matchingLineIndex].MatchedParts.AddRange(matchingLine.MatchedParts);
                    else
                        tag.MatchingLines.Add(matchingLine);
                }
            }

            return resultTags.Values;
        }

        private MatchingLine GetMatchingLine(List<TokenReference> plainTextTokens, MatchedTag matchedTag)
        {
            int matchedTagStartTokenNumber = (int)matchedTag.Start.TokenNumber;
            int matchedTagEndTokenNumber = (int)matchedTag.End.TokenNumber;

            TokenReference matchedPartStartToken = plainTextTokens[matchedTagStartTokenNumber];
            int matchedPartStart = matchedPartStartToken.StringPosition;
            TokenReference matchedPartEndToken = plainTextTokens[matchedTagEndTokenNumber];
            int matchedPartEnd = matchedPartEndToken.StringPosition + matchedPartEndToken.StringLength;
            int matchedPartLength = matchedPartEnd - matchedPartStart;

            if (matchedPartStartToken.TokenKind == NTTokenKind.LineFeed)
            {
                matchedTagStartTokenNumber -= 1;
                matchedTagEndTokenNumber += 1;
            }

            int matchingLineStartTokenNumber = plainTextTokens.FindLastIndex(matchedTagStartTokenNumber,
                token => token.TokenKind == NTTokenKind.LineFeed || token.TokenKind == NTTokenKind.Start);
            int matchingLineEndTokenNumber = plainTextTokens.FindIndex(matchedTagEndTokenNumber,
                token => token.TokenKind == NTTokenKind.LineFeed || token.TokenKind == NTTokenKind.End);

            TokenReference matchingLineStartToken = plainTextTokens[matchingLineStartTokenNumber];
            int matchingLineStart = matchingLineStartToken.StringPosition + matchingLineStartToken.StringLength;
            TokenReference matchingLineEndToken = plainTextTokens[matchingLineEndTokenNumber];
            int matchingLineEnd = matchingLineEndToken.StringPosition;
            if (matchedPartEndToken.TokenKind == NTTokenKind.LineFeed)
                matchingLineEnd += matchedPartEndToken.StringLength;
            int matchingLineLength = matchingLineEnd - matchingLineStart;

            return new MatchingLine(matchingLineStart, matchingLineLength,
                new MatchedPart(matchedPartStart, matchedPartLength));
        }
    }
}
