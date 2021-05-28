//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using Nezaboodka.Nevod.Negrep.Consoles;
using Nezaboodka.Nevod.Negrep.ResultTagsPrinters;

namespace Nezaboodka.Nevod.Negrep.Tests
{
    public class FakeConsole : IConsole
    {
        private readonly StringBuilder _stdoutBuffer = new StringBuilder();
        private readonly StringBuilder _stderrBuffer = new StringBuilder();

        public TextReader In { get; }
        public bool IsInputRedirected { get; }
        public bool IsOutputRedirected { get; }

        public string Stdout => _stdoutBuffer.ToString();
        public string Stderr => _stderrBuffer.ToString();

        public FakeConsole(string stdin)
            : this(stdin, isInputRedirected: false, isOutputRedirected: false)
        {
        }

        public FakeConsole(string stdin, bool isInputRedirected, bool isOutputRedirected)
        {
            In = new StringReader(stdin);
            IsInputRedirected = isInputRedirected;
            IsOutputRedirected = isOutputRedirected;
        }


        public string ReadToEnd()
        {
            return In.ReadToEnd();
        }

        public void Write(string value, ConsoleColor? color = null)
        {
            WithColor((string colorTag) => _stdoutBuffer.Append($"{colorTag}{value}{colorTag}"), color);
        }

        public void WriteLine(string value, ConsoleColor? color = null)
        {
            WithColor((string colorTag) => _stdoutBuffer.AppendLine($"{colorTag}{value}{colorTag}"), color);
        }

        public void WriteLineToStderr(string value)
        {
            _stderrBuffer.AppendLine(value);
        }

        public void Clear()
        {
            _stdoutBuffer.Clear();
            _stderrBuffer.Clear();
        }

        private void WithColor(Action<string> action, ConsoleColor? color)
        {
            string colorTag = string.Empty;
            if (color != null)
                colorTag = $"{{{color}}}";
            action(colorTag);
        }
    }
}
