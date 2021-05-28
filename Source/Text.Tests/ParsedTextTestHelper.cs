//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Text.Tests
{
    internal static class ParsedTextTestHelper
    {
        public static void SetXhtml(this ParsedText parsedText, IList<string> xhtml, ISet<int> plainTextElements)
        {
            for (int i = 0; i < xhtml.Count; i++)
            {
                if (plainTextElements.Contains(i))
                {
                    parsedText.AddPlainTextElement(xhtml[i]);
                }
                else
                {
                    parsedText.AddXhtmlElement(xhtml[i]);
                }
            }
        }
    }
}
