//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nezaboodka.Text.Parsing
{
    internal class TokenClassifier
    {
        private const char CR = '\r';
        private const char LF = '\n';

        // Public

        public TokenKind TokenKind { get; private set; }
        public bool IsHexadecimal;

        public TokenClassifier()
        {
            TokenKind = TokenKind.Undefined;
            IsHexadecimal = true;
        }

        public void Reset()
        {
            TokenKind = TokenKind.Undefined;
            IsHexadecimal = true;
        }

        public void AddCharacter(char c)
        {
            switch (TokenKind)
            {
                case TokenKind.Alphabetic:
                    ProcessAlphabetic(c);
                    break;
                case TokenKind.AlphaNumeric:
                    ProcessAlphaNumeric(c);
                    break;
                case TokenKind.NumericAlpha:
                    ProcessNumericAlpha(c);
                    break;
                case TokenKind.Numeric:
                    ProcessNumeric(c);
                    break;
                case TokenKind.WhiteSpace:
                    ProcessWhiteSpace(c);
                    break;
                case TokenKind.Undefined:
                    ProcessEmpty(c);
                    break;
            }
        }

        // Internal

        private void ProcessAlphabetic(char c)
        {
            if (char.IsDigit(c))
            {
                // IsHexadecimal remains the same
                TokenKind = TokenKind.AlphaNumeric;
            }
            else if (!char.IsLetter(c))
            {
                IsHexadecimal = false;
                TokenKind = TokenKind.Symbol;
            }
            else    // IsLetter
            {
                IsHexadecimal = IsHexadecimal && IsHexCharacter(c);
            }
        }

        private void ProcessAlphaNumeric(char c)
        {
            if (!char.IsLetterOrDigit(c))
            {
                IsHexadecimal = false;
                TokenKind = TokenKind.Symbol;
            }
            else    // IsLetterOrDigit
            {
                IsHexadecimal = IsHexadecimal && IsHexCharacter(c);
            }
        }

        private void ProcessNumericAlpha(char c)
        {
            if (!char.IsLetterOrDigit(c))
            {
                IsHexadecimal = false;
                TokenKind = TokenKind.Symbol;
            }
            else    // IsLetterOrDigit
            {
                IsHexadecimal = IsHexadecimal && IsHexCharacter(c);
            }
        }

        private void ProcessNumeric(char c)
        {
            if (char.IsLetter(c))
            {
                TokenKind = TokenKind.NumericAlpha;
                IsHexadecimal = IsHexCharacter(c);
            }
            else if (!char.IsDigit(c))
            {
                TokenKind = TokenKind.Symbol;
                IsHexadecimal = false;
            }
            // else IsHexadecimal remains true
        }

        private void ProcessWhiteSpace(char c)
        {
            IsHexadecimal = false;
            if (WordBreakTable.GetCharacterWordBreak(c) != WordBreak.Whitespace)
                TokenKind = TokenKind.Symbol;
        }

        private void ProcessEmpty(char c)
        {
            if (char.IsDigit(c))
            {
                IsHexadecimal = true;
                TokenKind = TokenKind.Numeric;
            }
            else if (char.IsLetter(c))
            {
                IsHexadecimal = IsHexCharacter(c);
                TokenKind = TokenKind.Alphabetic;
            }
            else
            {
                IsHexadecimal = false;
                if (WordBreakTable.GetCharacterWordBreak(c) == WordBreak.Whitespace)
                    TokenKind = TokenKind.WhiteSpace;
                else if (IsLineSeparator(c))
                    TokenKind = TokenKind.LineFeed;
                else if (IsPunctuationMark(c))
                    TokenKind = TokenKind.Punctuation;
                else
                    TokenKind = TokenKind.Symbol;
            }
        }

        private static bool IsLineSeparator(char c)
        {
            return (c == CR) || (c == LF);
        }

        private static bool IsHexCharacter(char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsPunctuationMark(char c)
        {
            switch (c)
            {
                case '.':
                case ',':
                case '!':
                case '?':
                case '(':
                case ')':
                case '-':
                case '\'':
                case '"':
                case ';':
                case ':':
                    return true;
                default:
                    return false;
            }
        }
    }
}
