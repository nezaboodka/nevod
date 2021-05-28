//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using CommandLine;

namespace Nezaboodka.Nevod.Negrep
{
    internal sealed class NegrepCommandLineArguments
    {
        private string _expression;

        [Option('e', "expression",
            HelpText = "Obtain search target from an expression.")]
        public string Expression
        {
            get => _expression ?? UnnamedPositionalArguments.FirstOrDefault();
            set => _expression = value;
        }

        [Option('p', "pattern-package",
            HelpText = "Obtain search target from a pattern package.")]
        public string PatternPackage { get; set; }

        [Option('f', "file",
            HelpText = "Obtain search target from FILE.")]
        public string FileWithPatterns { get; set; }

        [Option('o', "only-matching",
            HelpText = "Print only the matched (non-empty) parts of a matching line, " +
                       "with each such part on a separate output line.")]
        public bool OnlyMatching { get; set; }

        [Option('H', "with-filename",
            HelpText = "Print the file name for each match. " +
                       "This is the default when there is more than one file to search in.")]
        public bool WithFilename { get; set; }

        [Option('h', "no-filename",
            HelpText = "Suppress the prefixing of file names on output. " +
                       "This is the default when there is only one file (or only standard input) to search in.")]
        public bool NoFilename { get; set; }

        [Option(HelpText = "Output a usage message and exit.")]
        public bool Help { get; set; }

        [Option(HelpText = "Display version information.")]
        public bool Version { get; set; }

        [Value(0, MetaName = "files")]
        public IEnumerable<string> UnnamedPositionalArguments { get; set; }

        public bool IsSearchTargetFromPositionalArguments => (FileWithPatterns == null) && (PatternPackage == null)
            && (_expression == null);
        public bool FilesProvided => FilePaths.Any();
        public IEnumerable<string> FilePaths =>
            IsSearchTargetFromPositionalArguments ? UnnamedPositionalArguments.Skip(1) : UnnamedPositionalArguments;
    }
}
