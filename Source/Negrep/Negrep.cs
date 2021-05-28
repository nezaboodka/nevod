//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Nezaboodka.Nevod.Negrep.Consoles;
using Nezaboodka.Nevod;

namespace Nezaboodka.Nevod.Negrep
{
    public class Negrep
    {
        private const string UsageHeading = "Usage: negrep [OPTION]... 'EXPRESSION' [FILE]...";
        private const string HelpHeading = @"
            " + UsageHeading + @"
            Try 'negrep --help' for more information.
        ";
        private const string ExamplesHeading = @"
            Examples:

            cd <negrep installation directory>/examples
            negrep ""{'Android', 'IPhone'}"" example.txt
            negrep -f patterns.np example.txt
            echo Android or IPhone | negrep -e ""{'android', 'iphone'}""
        ";

        private readonly IConsole _console;
        private readonly NegrepConfig _config;

        public Negrep(IList<string> args, IConsole console, bool isStreamModeEnabled)
        {
            _console = console;
            try
            {
                _config = new NegrepConfig(args, _console, isStreamModeEnabled);
            }
            catch (FileNotFoundException e)
            {
                _console.WriteLineToStderr(e.Message);
            }
            catch (Exception e) when (e is SyntaxException)
            {
                _console.WriteLineToStderr(e.Message);
            }
        }

        public async Task<int> Execute()
        {
            int exitCode;

            switch (_config?.Info.Status)
            {
                case NegrepConfigStatus.Failed:
                    _console.WriteLineToStderr(HelpText.DefaultParsingErrorsHandler(_config.Info.ParserResult,
                        new HelpText(HelpHeading.TrimEachLine())));
                    exitCode = 1;
                    break;
                case NegrepConfigStatus.HelpRequest:
                    _console.WriteLine(GetHelpScreen(_config.Info.ParserResult));
                    exitCode = 0;
                    break;
                case NegrepConfigStatus.VersionRequest:
                    _console.WriteLine(GetVersion());
                    exitCode = 0;
                    break;
                case NegrepConfigStatus.UsageRequest:
                    _console.WriteLine(new HelpText(HelpHeading.TrimEachLine()));
                    exitCode = 0;
                    break;
                case NegrepConfigStatus.ReadyToMatch:
                    exitCode = await new NegrepMatcher(_config, _console).Match();
                    break;
                default:
                    exitCode = 1;
                    break;
            }

            return exitCode;
        }

        private string GetHelpScreen(ParserResult<NegrepCommandLineArguments> parserResult)
        {
            var helpText = new HelpText(UsageHeading);
            helpText.MaximumDisplayWidth = int.MaxValue;
            helpText.AutoHelp = false;
            helpText.AutoVersion = false;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AddOptions(parserResult);
            helpText.AddPostOptionsText(ExamplesHeading.TrimEachLine());

            return helpText;
        }

        private static string GetVersion()
        {
            Version version = typeof(Negrep).Assembly.GetName().Version;
            string result = $"{version.Major}.{version.Minor}";
            return result;
        }
    }
}
