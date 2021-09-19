//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public class TokenAttributes
    {
        public Range LengthRange { get; }

        public TokenAttributes()
        {
            LengthRange = Range.ZeroPlus();
        }

        public TokenAttributes(Range lengthRange)
        {
            LengthRange = lengthRange;
        }

        internal virtual bool CompareTo(Token token, string sampleText)
        {
            int length = token.Text.Length;
            if (sampleText != null)
                length = length - sampleText.Length;
            return length >= LengthRange.LowBound && length <= LengthRange.HighBound;
        }
    }

    public enum CharCase
    {
        Undefined = 0,
        Lowercase = 1,
        Uppercase = 2,
        TitleCase = 3
    }

    public class WordAttributes : TokenAttributes
    {
        public WordClass WordClass { get; }
        public CharCase CharCase { get; }

        public WordAttributes()
        {
        }

        public WordAttributes(WordClass wordClass, Range lengthRange, CharCase charCase)
            : base(lengthRange)
        {
            WordClass = wordClass;
            CharCase = charCase;
        }

        internal override bool CompareTo(Token token, string sampleText)
        {
            int suffixLength = token.Text.Length - sampleText.Length;
            if (suffixLength < LengthRange.LowBound || suffixLength > LengthRange.HighBound)
                return false;
            var tokenClassifier = TokenClassifier.Create();
            bool checkWordClass = WordClass != WordClass.Any;
            if (checkWordClass)
                switch (token.WordClass)
                {
                    case WordClass.Alpha:
                        if (WordClass != WordClass.Alpha)
                            return false;
                        else
                            checkWordClass = false;
                        break;
                    case WordClass.Num:
                        if (WordClass != WordClass.Num)
                            return false;
                        else
                            checkWordClass = false;
                        break;
                    case WordClass.AlphaNum:
                        if (WordClass == WordClass.Alpha)
                            return false;
                        break;
                    case WordClass.NumAlpha:
                        if (WordClass == WordClass.Num)
                            return false;
                        break;
                }
            switch (CharCase)
            {
                case CharCase.Undefined:
                    if (checkWordClass)
                        for (int i = sampleText.Length, n = token.Text.Length; i < n; i++)
                            tokenClassifier.AddCharacter(token.Text[i]);
                    break;
                case CharCase.Lowercase:
                    for (int i = sampleText.Length, n = token.Text.Length; i < n; i++)
                    {
                        if (!char.IsLower(token.Text, i))
                            return false;
                        else if (checkWordClass)
                            tokenClassifier.AddCharacter(token.Text[i]);
                    }
                    break;
                case CharCase.Uppercase:
                    for (int i = sampleText.Length, n = token.Text.Length; i < n; i++)
                    {
                        if (!char.IsUpper(token.Text, i))
                            return false;
                        else if (checkWordClass)
                            tokenClassifier.AddCharacter(token.Text[i]);
                    }
                    break;
                case CharCase.TitleCase:
                    int k = sampleText.Length;
                    if (k < token.Text.Length && !char.IsUpper(token.Text, k))
                        return false;
                    else if (checkWordClass)
                        tokenClassifier.AddCharacter(token.Text[k]);
                    for (int i = k + 1, n = token.Text.Length; i < n; i++)
                    {
                        if (!char.IsLower(token.Text, i))
                            return false;
                        else if (checkWordClass)
                            tokenClassifier.AddCharacter(token.Text[k]);
                    }
                    break;
            }
            if (checkWordClass)
            {
                WordClass suffixWordClass = TextSource.WordClassByTokenReferenceKind[(int)tokenClassifier.TokenKind];
                if (suffixWordClass != WordClass)
                    return false;
            }
            return true;
        }
    }
}
