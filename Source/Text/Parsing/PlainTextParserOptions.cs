//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Text.Parsing
{
    public class PlainTextParserOptions
    {
        public bool ProduceStartAndEndTokens;
        public bool DetectParagraphs;

        public static PlainTextParserOptions Default = new PlainTextParserOptions()
        {
            ProduceStartAndEndTokens = true,
            DetectParagraphs = true,
        };
    }
}
