//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.IO;

namespace Nezaboodka.Nevod.Negrep
{
    internal class SourceTextInfo
    {
        public string SourceText { get; }
        public TextReader SourceStream { get; }
        public bool ShouldCloseReader { get; }
        public string Path { get; }

        public bool IsStream => (SourceStream != null);

        public SourceTextInfo(TextReader sourceStream, bool shouldCloseReader, string path = "")
        {
            SourceStream = sourceStream;
            ShouldCloseReader = shouldCloseReader;
            Path = path;
        }

        public SourceTextInfo(string sourceText, string path = "")
        {
            SourceText = sourceText;
            Path = path;
        }
    }
}
