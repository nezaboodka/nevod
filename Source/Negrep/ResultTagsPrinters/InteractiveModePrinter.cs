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
    internal class InteractiveModePrinter : ResultTagsPrinter
    {
        private readonly bool _isSourceTextFromStdin;

        private readonly ConsoleColor FilenameColor = ConsoleColor.DarkGray;
        private readonly ConsoleColor TagnameColor = ConsoleColor.Green;
        private readonly ConsoleColor MatchColor = ConsoleColor.Red;

        public InteractiveModePrinter(IConsole console, PrefixingMode prefixingMode, PrintMode printMode,
            bool isSourceTextFromStdin)
        {
            _isSourceTextFromStdin = isSourceTextFromStdin;
            _console = console;
            _prefixingMode = prefixingMode;
            _printMode = printMode;
        }

        public override void Print(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags)
        {
            if (!_isSourceTextFromStdin &&
                (_prefixingMode & PrefixingMode.PrefixWithFilename) == PrefixingMode.PrefixWithFilename)
            {
                _console.WriteLine($"{sourceTextInfo.Path}:", FilenameColor);
            }

            switch (_printMode)
            {
                case PrintMode.PrintMatchingLine:
                    PrintMatchingLine(sourceTextInfo, resultTags);
                    break;
                case PrintMode.PrintMatchedPartsOnly:
                    PrintMatchedPartsOnly(sourceTextInfo, resultTags, (ResultTagPrefix prefix, string matchedPart) =>
                    {
                        if (prefix.Tagname != string.Empty)
                            _console.Write(prefix.Tagname, TagnameColor);

                        _console.WriteLine(matchedPart, MatchColor);
                    });
                    break;
            }
        }

        private void PrintMatchingLine(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags)
        {
            foreach (var resultTag in resultTags)
            {
                ResultTagPrefix prefix = GetResultTagPrefix(resultTag);
                string text = sourceTextInfo.SourceText;
                bool perLine = sourceTextInfo.IsStream;
                foreach (var matchingLine in resultTag.MatchingLines)
                {
                    if (prefix.Tagname != string.Empty)
                        _console.Write(prefix.Tagname, TagnameColor);
                    if (perLine && matchingLine.Text == null)
                        _console.WriteLineToStderr(LineIsTooLongToPrintMessage);
                    else
                    {
                        if (perLine)
                            text = matchingLine.Text;
                        long currentPosition = matchingLine.Start;
                        long currentPositionInText;
                        int start;
                        int length;
                        foreach (var matchedPart in matchingLine.MatchedParts)
                        {
                            currentPositionInText = currentPosition;
                            if (perLine)
                                currentPositionInText -= matchingLine.Start;
                            if (currentPosition != matchedPart.Start)
                            {
                                start = (int)currentPositionInText;
                                length = (int)(matchedPart.Start - currentPosition);
                                _console.Write(text.Substring(start, length).ReplaceLineBreakWithNull());
                            }
                            long matchedPartStartInText = matchedPart.Start;
                            if (perLine)
                                matchedPartStartInText -= matchingLine.Start;
                            start = (int)matchedPartStartInText;
                            length = (int)matchedPart.Length;
                            _console.Write(text.Substring(start, length).ReplaceLineBreakWithNull(), MatchColor);
                            currentPosition = matchedPart.Start + matchedPart.Length;
                        }
                        if (currentPosition != matchingLine.Start + matchingLine.Length)
                        {
                            currentPositionInText = currentPosition;
                            if (perLine)
                                currentPositionInText -= matchingLine.Start;
                            start = (int)currentPositionInText;
                            length = (int)(matchingLine.Start + matchingLine.Length - currentPosition);
                            _console.Write(text.Substring(start, length).ReplaceLineBreakWithNull());
                        }
                        _console.Write(Environment.NewLine);
                    }
                }
            }
        }
    }
}
