//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nezaboodka.Nevod.Negrep.ResultTagsPrinters
{
    [Flags]
    internal enum PrefixingMode
    {
        NoPrefixes = 0,
        PrefixWithTagname = 1 << 0,
        PrefixWithFilename = 1 << 1,
        PrefixWithBoth = PrefixWithTagname | PrefixWithFilename
    }

    internal enum PrintMode
    {
        PrintMatchingLine,
        PrintMatchedPartsOnly
    }

    internal struct ResultTagPrefix
    {
        public string Tagname { get; }
        public string Filename { get; }

        public ResultTagPrefix(string tagname, string filename)
        {
            Tagname = tagname;
            Filename = filename;
        }

        public override string ToString()
        {
            return $"{Filename}{Tagname}";
        }
    }

    internal interface IResultTagsPrinter
    {
        void Print(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags);
    }
}
