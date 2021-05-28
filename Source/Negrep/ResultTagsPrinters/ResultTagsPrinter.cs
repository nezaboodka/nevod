//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nezaboodka.Nevod.Negrep.Consoles;

namespace Nezaboodka.Nevod.Negrep.ResultTagsPrinters
{
    internal abstract class ResultTagsPrinter : IResultTagsPrinter
    {
        protected const string LineIsTooLongToPrintMessage = "[line is too long to print]";

        protected IConsole _console;
        protected PrefixingMode _prefixingMode;
        protected PrintMode _printMode;

        public abstract void Print(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags);

        protected void PrintMatchedPartsOnly(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags,
            Action<ResultTagPrefix, string> write)
        {
            foreach (var resultTag in resultTags)
            {
                ResultTagPrefix prefix = GetResultTagPrefix(resultTag);
                foreach (var matchingLine in resultTag.MatchingLines)
                {
                    foreach (var matchedPart in matchingLine.MatchedParts)
                    {
                        int partStart;
                        int partLength;
                        string matchedPartToPrint;
                        if (sourceTextInfo.IsStream)
                        {
                            if (matchingLine.Text != null)
                            {
                                partStart = (int)(matchedPart.Start - matchingLine.Start);
                                partLength = (int)matchedPart.Length;
                                matchedPartToPrint = matchingLine.Text.Substring(partStart, partLength)
                                    .ReplaceLineBreakWithNull();
                            }
                            else
                            {
                                _console.WriteLineToStderr(LineIsTooLongToPrintMessage);
                                continue;
                            }
                        }
                        else
                        {
                            partStart = (int)matchedPart.Start;
                            partLength = (int)matchedPart.Length;
                            matchedPartToPrint = sourceTextInfo.SourceText.Substring(partStart, partLength)
                                .ReplaceLineBreakWithNull();
                        }
                        write(prefix, matchedPartToPrint);
                    }
                }
            }
        }

        protected ResultTagPrefix GetResultTagPrefix(ResultTag resultTag)
        {
            string tagname = string.Empty;
            string filename = string.Empty;

            switch (_prefixingMode)
            {
                case PrefixingMode.NoPrefixes:
                    break;
                case PrefixingMode.PrefixWithTagname:
                    tagname = _console.IsOutputRedirected ? $":{resultTag.Name}:" : $"{resultTag.Name}:";
                    break;
                case PrefixingMode.PrefixWithFilename:
                    filename = $"{resultTag.Source}:";
                    break;
                case PrefixingMode.PrefixWithBoth:
                    filename = $"{resultTag.Source}";
                    tagname = _console.IsOutputRedirected ? $":{resultTag.Name}:" : $"{resultTag.Name}:";
                    break;
            }

            return new ResultTagPrefix(tagname, filename);
        }
    }
}
