//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Text.Parsing
{
    public class PlainTextParser : Parser
    {
        private const int LookAheadSize = 2;
        private readonly string fText;
        private readonly PlainTextTagger fPlainTextParagraphsTagger;
        private int fLookAheadPosition = -1;

        private int CurrentPosition => fLookAheadPosition - LookAheadSize;

        public PlainTextParserOptions Options { get; set; }

        // Public

        public static ParsedText Parse(string plainText, PlainTextParserOptions options = null)
        {
            ParsedText result;
            using (var parser = new PlainTextParser(plainText, options))
                result = parser.Parse();
            return result;
        }

        public PlainTextParser(string plainText, PlainTextParserOptions options = null)
        {
            if (plainText != null)
            {
                if (options != null)
                    Options = options;
                else
                    Options = PlainTextParserOptions.Default;
                fParsedText.AddPlainTextElement(plainText);
                fText = plainText;
                if (Options.DetectParagraphs)
                    fPlainTextParagraphsTagger = new PlainTextTagger(fParsedText);
            }
            else
                throw new ArgumentNullException(nameof(plainText));
        }

        public override void Dispose()
        {
        }

        public override ParsedText Parse()
        {
            if (Options.ProduceStartAndEndTokens)
                fParsedText.AddToken(StartToken);
            InitializeLookahead();
            while (NextCharacter())
            {
                fTokenClassifier.AddCharacter(fText[CurrentPosition]);
                if (fWordBreaker.IsBreak())
                {
                    SaveToken();
                    if (Options.DetectParagraphs)
                        ProcessTags();
                }
            }
            if (Options.ProduceStartAndEndTokens)
            {
                TokenReference endToken = GetEndToken(CurrentPosition + 1);
                fParsedText.AddToken(endToken);
            }
            return fParsedText;
        }

        // Internal

        private void InitializeLookahead()
        {
            NextCharacter();
            NextCharacter();
        }

        private void SaveToken()
        {
            var token = new TokenReference
            {
                TokenKind = fTokenClassifier.TokenKind,
                XhtmlIndex = 0,
                StringPosition = fTokenStartPosition,
                StringLength = CurrentPosition - fTokenStartPosition + 1,
                IsHexadecimal = fTokenClassifier.IsHexadecimal,
            };
            fParsedText.AddToken(token);
            fTokenStartPosition = CurrentPosition + 1;
            fTokenClassifier.Reset();
        }

        private bool NextCharacter()
        {
            bool result;
            if (CurrentPosition < fText.Length - 1)
            {
                fLookAheadPosition++;
                result = true;
            }
            else
                result = false;
            if (fLookAheadPosition < fText.Length)
            {
                WordBreak wordBreak = WordBreakTable.GetCharacterWordBreak(fText[fLookAheadPosition]);
                fWordBreaker.AddWordBreak(wordBreak);
            }
            else
                fWordBreaker.NextWordBreak();
            return result;
        }

        private void ProcessTags()
        {
            if (CurrentTokenIndex >= 0)
            {
                if (CurrentPosition < fText.Length - 1)
                {
                    TokenKind lastTokenKind = fParsedText.PlainTextTokens[CurrentTokenIndex].TokenKind;
                    fPlainTextParagraphsTagger.ProcessToken(lastTokenKind);
                }
                else
                    fPlainTextParagraphsTagger.ProcessEndOfText();
            }
        }
    }
}
