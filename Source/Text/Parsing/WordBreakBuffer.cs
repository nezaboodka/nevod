//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Text.Parsing
{
    internal class WordBreakBuffer
    {
        public WordBreak NextOfNextWordBreak { get; private set; } = WordBreak.Empty;
        public WordBreak NextWordBreak { get; private set; } = WordBreak.Empty;
        public WordBreak CurrentWordBreak { get; private set; } = WordBreak.Empty;
        public WordBreak PreviousWordBreak { get; private set; } = WordBreak.Empty;

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
