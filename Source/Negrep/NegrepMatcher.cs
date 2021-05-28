//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using GlobExpressions;
using Nezaboodka.Nevod.Negrep.Consoles;

namespace Nezaboodka.Nevod.Negrep
{
    [Flags]
    internal enum MatchStatus
    {
        MatchesFound = 0,
        NoMatchesFound = 1
    }

    internal class NegrepMatcher
    {
        private const int MaxFileSizeInBytesToReadAllText = 10 * 1024 * 1024; // 10 MB

        private readonly NegrepConfig _config;
        private readonly IConsole _console;
        private ConcurrentDictionary<string, byte> _matchedFiles = new ConcurrentDictionary<string, byte>();

        public NegrepMatcher(NegrepConfig config, IConsole console)
        {
            _config = config;
            _console = console;
            _matchedFiles.Clear();
        }

        public async Task<int> Match()
        {
            var globbingBlock = new TransformManyBlock<string, string>(
                globPattern => GetPathsMatchingGlob(globPattern),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }
            );

            var readingBlock = new TransformBlock<string, SourceTextInfo>(
                async sourcePath =>
                {
                    if (!_matchedFiles.TryAdd(Path.GetFullPath(sourcePath), 0))
                        return null;
                    return await GetSourceTextInfo(sourcePath);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 100
                }
            );

            var matchingBlock =
                new TransformBlock<SourceTextInfo, (SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags)>(
                    sourceTextInfo =>
                    {
                        if (sourceTextInfo.IsStream)
                            return new NegrepLineStreamMatchingBlock(_config).Match(sourceTextInfo);
                        else
                            return new NegrepMatchingBlock(_config).Match(sourceTextInfo);
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount,
                        BoundedCapacity = 100
                    }
                );

            var printingBlock =
                new TransformBlock<(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags), MatchStatus>(
                    outputData =>
                    {
                        if (outputData.sourceTextInfo != null && outputData.resultTags != null)
                        {
                            if (outputData.resultTags.Any())
                            {
                                _config.ResultTagsPrinter.Print(outputData.sourceTextInfo, outputData.resultTags);
                                return MatchStatus.MatchesFound;
                            }
                        }
                        return MatchStatus.NoMatchesFound;
                    },
                    // MaxDegreeOfParallelism is set to 1 to avoid concurrency when output operations are performed
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = 1,
                        BoundedCapacity = 100
                    }
                );

            var resultBlock = new BufferBlock<MatchStatus>(
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }
            );

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            globbingBlock.LinkTo(readingBlock, linkOptions);
            readingBlock.LinkTo(matchingBlock, linkOptions, sourceTextInfo => sourceTextInfo != null);
            readingBlock.LinkTo(DataflowBlock.NullTarget<SourceTextInfo>());
            matchingBlock.LinkTo(printingBlock, linkOptions, outputData => outputData.resultTags.Any());
            matchingBlock.LinkTo(
                DataflowBlock.NullTarget<(SourceTextInfo sourceTextInfo, IEnumerable<ResultTag> resultTags)>());
            printingBlock.LinkTo(resultBlock, linkOptions);

            if (_config.IsSourceTextFromStdin)
            {
                if (_config.IsStreamModeEnabled)
                    matchingBlock.Post(new SourceTextInfo(_console.In, shouldCloseReader: false));
                else
                    matchingBlock.Post(new SourceTextInfo(_console.ReadToEnd()));
            }
            else
            {
                foreach (var filepath in _config.FilePaths)
                {
                    if (filepath.IsGlob())
                        await globbingBlock.SendAsync(filepath);
                    else
                        await readingBlock.SendAsync(filepath);
                }
            }

            globbingBlock.Complete();

            await globbingBlock.Completion;
            await readingBlock.Completion;
            await matchingBlock.Completion;
            await printingBlock.Completion;

            int exitCode = 1;
            if (resultBlock.TryReceiveAll(out IList<MatchStatus> exitCodes))
                exitCode = (int)exitCodes.Aggregate((val1, val2) => val1 & val2);
            _matchedFiles.Clear();
            return exitCode;
        }

        private IEnumerable<string> GetPathsMatchingGlob(string globPattern)
        {
            const string currentDirectoryDot = ".";
            string rootDir = currentDirectoryDot;
            if (Path.IsPathRooted(globPattern))
            {
                rootDir = Path.GetPathRoot(globPattern);
                globPattern = globPattern.Substring(rootDir.Length, globPattern.Length - rootDir.Length);
            }

            var root = new DirectoryInfo(rootDir);
            var filepaths = root.GlobFiles(globPattern);

            if (!filepaths.Any())
                _console.WriteLineToStderr($"{globPattern}: no such file or directory");

            IEnumerable<string> result;
            if (rootDir == currentDirectoryDot)
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                int namePositionInPath = currentDirectory.Length + 1;
                result = filepaths.Select(fileinfo =>
                    fileinfo.FullName.Substring(namePositionInPath, fileinfo.FullName.Length - namePositionInPath));
            }
            else
                result = filepaths.Select(fileinfo => fileinfo.FullName);
            return result;
        }

        private async Task<SourceTextInfo> GetSourceTextInfo(string path)
        {
            SourceTextInfo sourceTextInfo = null;

            try
            {
                if (!File.Exists(path))
                    _console.WriteLineToStderr($"{path}: no such file or directory");
                else
                {
                    var fileInfo = new FileInfo(path);
                    StreamReader fileReader = File.OpenText(path);
                    if (!_config.IsStreamModeEnabled || fileInfo.Length <= MaxFileSizeInBytesToReadAllText)
                    {
                        sourceTextInfo = new SourceTextInfo(await fileReader.ReadToEndAsync(), path);
                        fileReader.Close();
                    }
                    else
                        sourceTextInfo = new SourceTextInfo(fileReader, shouldCloseReader: true, path);
                }
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
            {
                _console.WriteLineToStderr(e.Message);
            }

            return sourceTextInfo;
        }
    }
}
