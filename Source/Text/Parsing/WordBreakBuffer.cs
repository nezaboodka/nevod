//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Text.Parsing
{
    internal struct WordBreakBuffer
    {
        public WordBreak NextOfNextWordBreak;
        public WordBreak NextWordBreak;
        public WordBreak CurrentWordBreak;
        public WordBreak PreviousWordBreak;

        public static WordBreakBuffer Create()
        {
            var result = new WordBreakBuffer();
            result.NextOfNextWordBreak = WordBreak.Empty;
            result.NextWordBreak = WordBreak.Empty;
            result.CurrentWordBreak = WordBreak.Empty;
            result.PreviousWordBreak = WordBreak.Empty;
            return result;
        }

        public void AddWordBreak(WordBreak wordBreak)
        {
            ShiftBuffer();
            NextOfNextWordBreak = wordBreak;
        }

        public void ShiftBuffer()
        {
            PreviousWordBreak = CurrentWordBreak;
            CurrentWordBreak = NextWordBreak;
            NextWordBreak = NextOfNextWordBreak;
            NextOfNextWordBreak = WordBreak.Empty;
        }
    }
}
