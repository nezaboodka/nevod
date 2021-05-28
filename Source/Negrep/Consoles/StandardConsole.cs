//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Nezaboodka.Nevod.Negrep.ResultTagsPrinters;

namespace Nezaboodka.Nevod.Negrep.Consoles
{
    public class StandardConsole : IConsole
    {
        public TextReader In => Console.In;
        public bool IsInputRedirected => Console.IsInputRedirected;
        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public string ReadToEnd()
        {
            return Console.In.ReadToEnd();
        }

        public void Write(string value, ConsoleColor? color = null)
        {
            WithColor(() => Console.Write(value), color);
        }

        public void WriteLine(string value, ConsoleColor? color = null)
        {
            WithColor(() => Console.WriteLine(value), color);
        }

        public void WriteLineToStderr(string value)
        {
            Console.Error.WriteLine(value);
        }

        private void WithColor(Action action, ConsoleColor? color)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            try
            {
                if (color != null)
                    Console.ForegroundColor = color.Value;
                action();
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
