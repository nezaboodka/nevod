//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Nezaboodka.Nevod.Negrep.ResultTagsPrinters;

namespace Nezaboodka.Nevod.Negrep.Consoles
{
    public interface IConsole
    {
        TextReader In { get; }
        bool IsInputRedirected { get; }
        bool IsOutputRedirected { get; }

        string ReadToEnd();
        void Write(string value, ConsoleColor? color = null);
        void WriteLine(string value, ConsoleColor? color = null);
        void WriteLineToStderr(string value);
    }
}
