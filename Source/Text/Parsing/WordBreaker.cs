//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Text.Parsing
{
    internal class WordBreaker
    {
        private readonly WordBreakBuffer fWordBreakBuffer = new WordBreakBuffer();

        // Public

        public void AddWordBreak(WordBreak wordBreak)
        {
            if (!ShouldIgnore(wordBreak)) // WB4.
            {
                fWordBreakBuffer.AddWordBreak(wordBreak);
            }
        }

        public bool IsBreak()
        {
            bool result;
            // WB2.
            if (!HasEnoughCharacters() && !IsLastCharacter())
                result = false;
            // WB3.
            else if (IsLineBreak(fWordBreakBuffer.CurrentWordBreak) || IsLineBreak(fWordBreakBuffer.NextWordBreak))
                result = !((fWordBreakBuffer.CurrentWordBreak == WordBreak.CarriageReturn) && (fWordBreakBuffer.NextWordBreak == WordBreak.LineFeed));
            // WB5.
            else if (IsAlphabeticOrHebrewLetter(fWordBreakBuffer.CurrentWordBreak) && IsAlphabeticOrHebrewLetter(fWordBreakBuffer.NextWordBreak))
                result = false;
            // WB6. (without single quotes)
            else if (IsAlphabeticOrHebrewLetter(fWordBreakBuffer.CurrentWordBreak) && ((fWordBreakBuffer.NextWordBreak == WordBreak.MidLetter) ||
                (fWordBreakBuffer.NextWordBreak == WordBreak.MidNumberAndLetter))
                && IsAlphabeticOrHebrewLetter(fWordBreakBuffer.NextOfNextWordBreak))
                result = false;
            // WB7. (without single quotes)
            else if (IsAlphabeticOrHebrewLetter(fWordBreakBuffer.PreviousWordBreak) && ((fWordBreakBuffer.CurrentWordBreak == WordBreak.MidLetter)
                || (fWordBreakBuffer.CurrentWordBreak == WordBreak.MidNumberAndLetter))
                && IsAlphabeticOrHebrewLetter(fWordBreakBuffer.NextWordBreak))
                result = false;
            // WB7a.
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.HebrewLetter) && (fWordBreakBuffer.NextWordBreak == WordBreak.SingleQuote))
                result = false;
            // WB7b.
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.HebrewLetter) && (fWordBreakBuffer.NextWordBreak == WordBreak.DoubleQuote)
                && (fWordBreakBuffer.NextOfNextWordBreak == WordBreak.HebrewLetter))
                result = false;
            // WB7c.
            else if ((fWordBreakBuffer.PreviousWordBreak == WordBreak.HebrewLetter) && (fWordBreakBuffer.CurrentWordBreak == WordBreak.DoubleQuote)
                && (fWordBreakBuffer.NextWordBreak == WordBreak.HebrewLetter))
                result = false;
            // WB8.
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.Numeric) && (fWordBreakBuffer.NextWordBreak == WordBreak.Numeric))
                result = false;
            // WB9.
            else if (IsAlphabeticOrHebrewLetter(fWordBreakBuffer.CurrentWordBreak) && fWordBreakBuffer.NextWordBreak == WordBreak.Numeric)
                result = false;
            // WB10.
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.Numeric) && IsAlphabeticOrHebrewLetter(fWordBreakBuffer.NextWordBreak))
                result = false;
            // WB13.
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.Katakana) && (fWordBreakBuffer.NextWordBreak == WordBreak.Katakana))
                result = false;
            // WB13a.
            else if ((IsAlphabeticOrHebrewLetter(fWordBreakBuffer.CurrentWordBreak) || (fWordBreakBuffer.CurrentWordBreak == WordBreak.Numeric)
                || (fWordBreakBuffer.CurrentWordBreak == WordBreak.Katakana) || (fWordBreakBuffer.CurrentWordBreak == WordBreak.ExtenderForNumbersAndLetters))
                && (fWordBreakBuffer.NextWordBreak == WordBreak.ExtenderForNumbersAndLetters))
                result = false;
            // WB13b.
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.ExtenderForNumbersAndLetters) && (IsAlphabeticOrHebrewLetter(fWordBreakBuffer.NextWordBreak)
                || (fWordBreakBuffer.NextWordBreak == WordBreak.Numeric) || (fWordBreakBuffer.NextWordBreak == WordBreak.Katakana)))
                result = false;
            // custom: do not break between whitespaces
            else if ((fWordBreakBuffer.CurrentWordBreak == WordBreak.Whitespace) && (fWordBreakBuffer.NextWordBreak == WordBreak.Whitespace))
                result = false;
            // WB14.
            else
                result = true;
            return result;
        }

        public void NextWordBreak()
        {
            fWordBreakBuffer.ShiftBuffer();
        }

        public bool IsEmptyBuffer()
        {
            return (fWordBreakBuffer.CurrentWordBreak == WordBreak.Empty) && (fWordBreakBuffer.NextWordBreak == WordBreak.Empty) && (fWordBreakBuffer.NextOfNextWordBreak == WordBreak.Empty);
        }

        // Internal

        private bool HasEnoughCharacters()
        {
            return (fWordBreakBuffer.NextWordBreak != WordBreak.Empty) && (fWordBreakBuffer.CurrentWordBreak != WordBreak.Empty);
        }

        private bool IsLastCharacter()
        {
            return (fWordBreakBuffer.CurrentWordBreak != WordBreak.Empty) && (fWordBreakBuffer.NextWordBreak == WordBreak.Empty);
        }        

        private bool IsFirstCharacter()
        {
            return (fWordBreakBuffer.PreviousWordBreak == WordBreak.Empty) && (fWordBreakBuffer.NextOfNextWordBreak == WordBreak.Empty);
        }

        private bool ShouldIgnore(WordBreak wordBreak)
        {
            return ((wordBreak == WordBreak.Extender) || (wordBreak == WordBreak.Format)) && !IsFirstCharacter();
        }

        // Static internal       

        private static bool IsAlphabeticOrHebrewLetter(WordBreak wordBreak)
        {
            return (wordBreak == WordBreak.AlphabeticLetter) || (wordBreak == WordBreak.HebrewLetter);
        }

        private static bool IsLineBreak(WordBreak wordBreak)
        {
            return (wordBreak == WordBreak.Newline) || (wordBreak == WordBreak.LineFeed) || (wordBreak == WordBreak.CarriageReturn);
        }        
    }
}
