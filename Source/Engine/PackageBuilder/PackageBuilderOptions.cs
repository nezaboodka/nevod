//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using System.IO;

namespace Nezaboodka.Nevod
{
    public class PackageBuilderOptions
    {
        public static readonly PackageBuilderOptions Default = new PackageBuilderOptions();

        public bool PatternReferencesInlined { get; set; }

        public bool SyntaxInformationBinding { get; set; }

        public bool? IsFileSystemCaseSensitive { get; set; }

        public PackageBuilderOptions()
        {
            PatternReferencesInlined = true;
        }

        public PackageBuilderOptions(PackageBuilderOptions source)
        {
            if (source == null)
                source = Default;
            PatternReferencesInlined = source.PatternReferencesInlined;
            SyntaxInformationBinding = source.SyntaxInformationBinding;
            IsFileSystemCaseSensitive = source.IsFileSystemCaseSensitive;
        }
    }
}
