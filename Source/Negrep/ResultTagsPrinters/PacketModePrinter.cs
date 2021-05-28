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
    internal class PacketModePrinter : ResultTagsPrinter
    {
        public PacketModePrinter(IConsole console, PrefixingMode prefixingMode, PrintMode printMode)
        {
            _console = console;
            _prefixingMode = prefixingMode;
            _printMode = printMode;
        }

        public override void Print(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags)
        {
            switch (_printMode)
            {
                case PrintMode.PrintMatchingLine:
                    PrintMatchingLine(sourceTextInfo, resultTags);
                    break;
                case PrintMode.PrintMatchedPartsOnly:
                    PrintMatchedPartsOnly(sourceTextInfo, resultTags, (ResultTagPrefix prefix, string matchedPart) =>
                    {
                        _console.WriteLine($"{prefix}{matchedPart}");
                    });
                    break;
            }
        }

        private void PrintMatchingLine(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags)
        {
            foreach (var resultTag in resultTags)
            {
                ResultTagPrefix prefix = GetResultTagPrefix(resultTag);
                foreach (var matchingLine in resultTag.MatchingLines)
                {
                    string valueToPrint;
                    if (sourceTextInfo.IsStream)
                    {
                        if (matchingLine.Text != null)
                            valueToPrint = matchingLine.Text.ReplaceLineBreakWithNull();
                        else
                        {
                            _console.WriteLineToStderr(LineIsTooLongToPrintMessage);
                            continue;
                        }
                    }
                    else
                    {
                        int lineStart = (int)matchingLine.Start;
                        int lineLength = (int)matchingLine.Length;
                        valueToPrint = sourceTextInfo.SourceText.Substring(lineStart, lineLength)
                            .ReplaceLineBreakWithNull();
                    }
                    _console.WriteLine($"{prefix}{valueToPrint}");
                }
            }
        }
    }
}
