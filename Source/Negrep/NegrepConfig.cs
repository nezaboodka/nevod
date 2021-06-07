//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using Nezaboodka.Nevod.Negrep.Consoles;
using Nezaboodka.Nevod.Negrep.ResultTagsPrinters;
using Nezaboodka.Nevod;

namespace Nezaboodka.Nevod.Negrep
{
    internal enum NegrepConfigStatus
    {
        Failed,
        HelpRequest,
        VersionRequest,
        UsageRequest,
        ReadyToMatch
    }

    internal struct NegrepConfigInfo
    {
        public ParserResult<NegrepCommandLineArguments> ParserResult { get; set; }
        public NegrepConfigStatus Status { get; set; }

        public NegrepConfigInfo(ParserResult<NegrepCommandLineArguments> parserResult, NegrepConfigStatus status)
        {
            ParserResult = parserResult;
            Status = status;
        }
    }

    internal class NegrepConfig
    {
        private const int MaxPatternPackageFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

        private enum CommandLineFormatType
        {
            Empty,
            SearchTargetFromPositionalArguments,
            SearchTargetFromOptions
        }

        private readonly IConsole _console;

        public IResultTagsPrinter ResultTagsPrinter { get; private set; }
        public PatternPackage PatternPackage { get; private set; }
        public List<string> FilePaths { get; private set; }
        public bool IsSourceTextFromStdin { get; private set; }
        public NegrepConfigInfo Info { get; private set; }
        public bool IsStreamModeEnabled { get; private set; }

        public NegrepConfig(IList<string> args, IConsole console, bool isStreamModeEnabled)
        {
            _console = console;
            FilePaths = new List<string>();
            IsSourceTextFromStdin = _console.IsInputRedirected;
            IsStreamModeEnabled = isStreamModeEnabled;

            var clFormatType = GetCommandLineFormatType(args);
            switch (clFormatType)
            {
                case CommandLineFormatType.Empty:
                    Info = new NegrepConfigInfo(null, NegrepConfigStatus.UsageRequest);
                    break;
                default:
                    var parser = new Parser(config =>
                    {
                        config.AutoHelp = false;
                        config.AutoVersion = false;
                    });
                    var parserResult = parser.ParseArguments<NegrepCommandLineArguments>(args);
                    var status = parserResult.MapResult(
                        negrepArgs => InitNegrepConfig(negrepArgs, clFormatType),
                        errors => NegrepConfigStatus.Failed
                    );
                    Info = new NegrepConfigInfo(parserResult, status);
                    break;
            }
        }

        private CommandLineFormatType GetCommandLineFormatType(IList<string> args)
        {
            string firstArg = args.FirstOrDefault();
            if (firstArg == null)
                return CommandLineFormatType.Empty;

            CommandLineFormatType clFormatType = CommandLineFormatType.Empty;
            if (!firstArg.BeginsWithDashOrDoubleDash())
                clFormatType = CommandLineFormatType.SearchTargetFromPositionalArguments;
            else
                clFormatType = CommandLineFormatType.SearchTargetFromOptions;

            return clFormatType;
        }

        private NegrepConfigStatus InitNegrepConfig(NegrepCommandLineArguments negrepArgs,
            CommandLineFormatType clFormatType)
        {
            if (negrepArgs.Help)
                return NegrepConfigStatus.HelpRequest;

            if (negrepArgs.Version)
                return NegrepConfigStatus.VersionRequest;

            NegrepConfigStatus status = NegrepConfigStatus.ReadyToMatch;
            switch (clFormatType)
            {
                case CommandLineFormatType.SearchTargetFromPositionalArguments:
                    PackageBuilder packageBuilder = GetPackageBuilder();
                    PatternPackage = packageBuilder.BuildPackageFromExpressionText(negrepArgs.Expression);
                    break;
                case CommandLineFormatType.SearchTargetFromOptions:
                    if (!negrepArgs.IsSearchTargetFromPositionalArguments)
                        PatternPackage = GetPatternPackageFromOptions(negrepArgs);
                    else
                        status = NegrepConfigStatus.UsageRequest;
                    break;
            }

            PrefixingMode prefixingMode;
            if (status != NegrepConfigStatus.UsageRequest)
            {
                if (IsSourceTextFromStdin)
                {
                    prefixingMode = PrefixingMode.NoPrefixes;
                    if (PatternPackage.SearchTargets.Count > 1)
                        prefixingMode = PrefixingMode.PrefixWithTagname;
                }
                else
                {
                    if (negrepArgs.FilesProvided)
                        FilePaths.AddRange(negrepArgs.FilePaths);
                    else
                        FilePaths.Add("**/*");

                    prefixingMode = PrefixingMode.PrefixWithFilename;

                    if (PatternPackage.SearchTargets.Count > 1)
                        prefixingMode = PrefixingMode.PrefixWithBoth;

                    if (negrepArgs.NoFilename ^ negrepArgs.WithFilename)
                    {
                        if (negrepArgs.NoFilename)
                            prefixingMode &= ~PrefixingMode.PrefixWithFilename;
                        else
                            prefixingMode |= PrefixingMode.PrefixWithFilename;
                    }
                }

                PrintMode printMode =
                    negrepArgs.OnlyMatching ? PrintMode.PrintMatchedPartsOnly : PrintMode.PrintMatchingLine;
                if (_console.IsOutputRedirected)
                    ResultTagsPrinter = new PacketModePrinter(_console, prefixingMode, printMode);
                else
                    ResultTagsPrinter = new InteractiveModePrinter(_console, prefixingMode, printMode);
            }

            return status;
        }

        private PatternPackage GetPatternPackageFromOptions(NegrepCommandLineArguments negrepArgs)
        {
            PackageBuilder packageBuilder = GetPackageBuilder();
            PatternPackage patternPackage = null;

            if (negrepArgs.PatternPackage != null)
            {
                patternPackage = packageBuilder.BuildPackageFromText(negrepArgs.PatternPackage);
            }
            else if (negrepArgs.FileWithPatterns != null)
            {
                var fileInfo = new FileInfo(negrepArgs.FileWithPatterns);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{negrepArgs.FileWithPatterns}: no such file or directory");
                else
                {
                    if (fileInfo.Length <= MaxPatternPackageFileSizeInBytes)
                        patternPackage = packageBuilder.BuildPackageFromFile(negrepArgs.FileWithPatterns);
                    else
                        throw new FileNotFoundException($"{negrepArgs.FileWithPatterns}: pattern package is too large. Consider using of multiple packages.");
                }
            }
            else if (negrepArgs.Expression != null)
            {
                patternPackage = packageBuilder.BuildPackageFromExpressionText(negrepArgs.Expression);
            }

            return patternPackage;
        }

        private static PackageBuilder GetPackageBuilder()
        {
            var packageBuilderOptions = new PackageBuilderOptions
            {
                SyntaxInformationBinding = true
            };
            return new PackageBuilder(packageBuilderOptions, Environment.CurrentDirectory, PackageCache.Global,
                NegrepFileContentProvider.FileContentProvider);
        }
    }
}
