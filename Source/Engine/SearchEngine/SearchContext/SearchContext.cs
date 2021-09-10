//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class SearchContext : MatchingEventObserver
    {
        private long fTimestamp;
        private MatchedTagsOfPattern[] fMatchedTagsPerPattern;
        private Dictionary<int, long> fCleaningTokenNumberPerPattern;
        private long fProcessedTokenCount;
        private bool fWasTotalCandidateLimitExceededOnCurrentToken;
        private bool fIsProcessingOfRemainingCandidates;

        private RootIndexHandler fRootIndexHandler;
        private WaitingIndexHandler fWaitingIndexHandler;
        private LocalIndexHandler fLocalIndexHandler;
        private FieldReferenceIndexHandler fFieldReferenceIndexHandler;
        private Queue<PatternEvent> fPatternEventQueue;

        private bool fWereConditionalHavingPatternsEnabled;
        private bool[] fExcludeFlagPerPattern;
        private bool[] fAnySpanExcludeFlagPerPattern;
        private bool[] fInternalExcludeFlagPerPattern;

        // Чтобы не создавать новый массив на каждом токене, попеременно используются два одинаковых массива.
        private bool[] fConditionalHavingExcludeFlagPerPattern;
        private bool[] fConditionalHavingExcludeFlagPerPatternShadow;

        public SearchEngine SearchEngine { get; }
        public CandidateFactory CandidateFactory { get; }
        public SearchExpression SearchQuery { get; }
        public SearchOptions SearchOptions { get; }
        public ITextSource TextSource { get; private set; }
        public SearchResultCallback ResultCallback { get; private set; }
        public SearchEngineTelemetry Telemetry { get; }

        public int ProcessedWordCount { get; private set; }
        private int fProcessedWordCountAfterCurrentToken;

        public bool WasCandidateLimitExceeded { get; private set; }
        public HashSet<int> ExceededLimitPatterns { get; private set; }

        public bool[] WasMatchPerPattern { get; private set; }

        public ActiveCandidates ActiveCandidates { get; private set; }
        public WaitingCandidatesIndex WaitingCandidates { get; private set; }
        public PendingHavingCandidates PendingHavingCandidates { get; private set; }
        public PendingInsideCandidates PendingInsideCandidates { get; private set; }
        public PendingOutsideCandidates PendingOutsideCandidates { get; private set; }

        public SearchContext(SearchEngine searchEngine, CandidateFactory candidateFactory, SearchExpression searchQuery,
            SearchOptions searchOptions, ITextSource textSource, SearchResultCallback resultCallback, SearchEngineTelemetry telemetry)
        {
            SearchEngine = searchEngine;
            CandidateFactory = candidateFactory;
            SearchQuery = searchQuery;
            SearchOptions = searchOptions;
            TextSource = textSource;
            ResultCallback = resultCallback;
            Telemetry = telemetry;

            ActiveCandidates = new ActiveCandidates();
            WaitingCandidates = new WaitingCandidatesIndex(this);
            PendingHavingCandidates = new PendingHavingCandidates();
            PendingInsideCandidates = new PendingInsideCandidates();
            PendingOutsideCandidates = new PendingOutsideCandidates();

            fRootIndexHandler = new RootIndexHandler(this);
            fWaitingIndexHandler = new WaitingIndexHandler(this);
            fLocalIndexHandler = new LocalIndexHandler(this);
            fFieldReferenceIndexHandler = new FieldReferenceIndexHandler(this);
            fPatternEventQueue = new Queue<PatternEvent>();

            fMatchedTagsPerPattern = new MatchedTagsOfPattern[searchQuery.PatternIndexLength];
            fCleaningTokenNumberPerPattern = new Dictionary<int, long>();
            ExceededLimitPatterns = new HashSet<int>();
            WasMatchPerPattern = new bool[searchQuery.PatternIndexLength];

            int patternIndexLength = searchQuery.PatternIndexLength;
            fAnySpanExcludeFlagPerPattern = new bool[patternIndexLength];
            if (SearchQuery.IsConditionalHavingIndexCreated || SearchOptions.FirstMatchOnly)
                fInternalExcludeFlagPerPattern = new bool[patternIndexLength];
            if (SearchQuery.IsConditionalHavingIndexCreated)
            {
                fExcludeFlagPerPattern = new bool[patternIndexLength];
                fConditionalHavingExcludeFlagPerPattern = new bool[patternIndexLength];
                fConditionalHavingExcludeFlagPerPatternShadow = new bool[patternIndexLength];
                for (int i = 0; i < patternIndexLength; i++)
                    fExcludeFlagPerPattern[i] = SearchQuery.InitialExcludeFlagPerPattern[i];
            }
            else
                fExcludeFlagPerPattern = null;
        }

        public void Reset(ITextSource textSource, SearchResultCallback resultCallback)
        {
            TextSource = textSource;
            ResultCallback = resultCallback;
            Telemetry.ResetTextSource();
            Reset();
        }

        public override void Reset()
        {
            fTimestamp = 0;
            Array.Clear(fMatchedTagsPerPattern, 0, fMatchedTagsPerPattern.Length);
            fCleaningTokenNumberPerPattern.Clear();
            fProcessedTokenCount = 0;
            fPatternEventQueue.Clear();

            Array.Clear(fAnySpanExcludeFlagPerPattern, 0, fAnySpanExcludeFlagPerPattern.Length);
            fWereConditionalHavingPatternsEnabled = false;
            if (SearchQuery.IsConditionalHavingIndexCreated)
            {
                for (int i = 0; i < fExcludeFlagPerPattern.Length; i++)
                    fExcludeFlagPerPattern[i] = SearchQuery.InitialExcludeFlagPerPattern[i];
            }

            ProcessedWordCount = 0;
            fProcessedWordCountAfterCurrentToken = 0;

            WasCandidateLimitExceeded = false;
            ExceededLimitPatterns.Clear();

            Array.Clear(WasMatchPerPattern, 0, WasMatchPerPattern.Length);

            ResetAllMatchingCandidates();
            base.Reset();
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            if (matchingEvent is TokenEvent tokenEvent)
            {
                if (tokenEvent.Token.Kind == TokenKind.Word)
                    fProcessedWordCountAfterCurrentToken++;
                OnNextToken(tokenEvent);
                fProcessedTokenCount++;
            }
            else if (matchingEvent is PatternEvent patternEvent)
                fPatternEventQueue.Enqueue(patternEvent);
            while (fPatternEventQueue.Count > 0)
            {
                PatternEvent patternEvent = fPatternEventQueue.Dequeue();
                OnNextPattern(patternEvent);
            }
            ProcessedWordCount = fProcessedWordCountAfterCurrentToken;
            if (fProcessedTokenCount % SearchOptions.TokenCountToWaitToPerformGarbageCollection == 0)
                PerformGarbageCollection();
            else if (CandidateFactory.NewWaitingTokensCount > SearchOptions.NewWaitingTokenCountToPerformGarbageCollection)
                PerformGarbageCollecitonOfWaitingCandidates();
        }

        public override void OnCompleted()
        {
            if (!IsCompleted)
            {
                ProcessRemainingCandidates();
                if (!SearchOptions.FirstMatchOnly)
                {
                    if (!SearchOptions.SelfOverlappingTagsInResults)
                        RemoveOverlapsOfMatchedTagsAndInvokeResultCallback();
                }
                else if (ResultCallback != null) // && (SearchOptions.FirstMatchOnly == true)
                {
                    TryPassFirstOfMatchedTagsToCallback(cleaningTokenNumberPerPattern: null);
                }
                base.OnCompleted();
            }
        }

        public SearchResult GetResult()
        {
            var result = new SearchResult();
            var matchedTags = new Dictionary<string, List<MatchedTag>>();
            for (int i = 0, n = fMatchedTagsPerPattern.Length; i < n; i++)
            {
                MatchedTagsOfPattern matchedTagsOfPattern = fMatchedTagsPerPattern[i];
                if (matchedTagsOfPattern != null)
                {
                    string patternFullName = matchedTagsOfPattern.Pattern.Name;
                    matchedTags[patternFullName] = new List<MatchedTag>(matchedTagsOfPattern.MatchedTags);
                    result.TagCount += matchedTagsOfPattern.MatchedTags.Count;
                }
            }
            result.TagsByName = matchedTags;
            if (SearchOptions.IsDebugMode)
            {
                result.Candidates = new List<MatchedTag>();
                // TODO: implement
            }
            result.WasCandidateLimitExceeded = WasCandidateLimitExceeded;
            return result;
        }

        // Для поэтапной обработки вложенных друг в друга операторов @having (#P = A @having B @having C;) при работе
        // с корневым индексом заполняется промежуточный массив флагов fConditionalHavingExcludeFlagPerPattern,
        // который используется на следующей итерации для включения в обработку контекстов операторов @having
        // следующего уровня вложенности.
        // Чтобы сразу включить в обработку корневого индекса внутренний контекст локальных операторов @having,
        // при работе с локальным (не корневым) индексом массив флагов fExcludeFlagPerPattern обновляется напрямую.
        public void EnableConditionalHavingPattern(int patternId, bool isRootIndex)
        {
            if (fExcludeFlagPerPattern[patternId])
            {
                if (isRootIndex)
                    fConditionalHavingExcludeFlagPerPattern[patternId] = false;
                else
                    fExcludeFlagPerPattern[patternId] = false;
                fWereConditionalHavingPatternsEnabled = true;
            }
        }

        public void DisableAnySpanPattern(int patternId)
        {
            fAnySpanExcludeFlagPerPattern[patternId] = true;
        }

        public void EnableAnySpanPattern(int patternId)
        {
            fAnySpanExcludeFlagPerPattern[patternId] = false;
        }

        public bool TryAddToActiveCandidates(RootCandidate rootCandidate)
        {
            bool success;
            if (CandidateFactory.TotalCandidateCount < SearchOptions.CandidateLimit)
            {
                int patternCandidateCount = CandidateFactory.CandidateCountPerPattern[rootCandidate.PatternId];
                if (patternCandidateCount < SearchOptions.PatternCandidateLimit)
                {
                    ActiveCandidates.Add(rootCandidate);
                    CandidateFactory.RegisterRootCandidate(rootCandidate);
                    Telemetry.TrackStart(rootCandidate);
                    success = true;
                }
                else
                {
                    ExceededLimitPatterns.Add(rootCandidate.PatternId);
                    WasCandidateLimitExceeded = true;
                    success = false;
                }
            }
            else
            {
                fWasTotalCandidateLimitExceededOnCurrentToken = true;
                WasCandidateLimitExceeded = true;
                success = false;
            }
            return success;
        }

        public bool TryAddToWaitingCandidates(AnySpanCandidate candidate)
        {
            bool success;
            RootCandidate rootCandidate = candidate.GetRootCandidate();
            if (CandidateFactory.TotalCandidateCount < SearchOptions.CandidateLimit)
            {
                int patternCandidateCount = CandidateFactory.CandidateCountPerPattern[rootCandidate.PatternId];
                if (patternCandidateCount < SearchOptions.PatternCandidateLimit)
                {
                    candidate.SavedWordCount = fProcessedWordCountAfterCurrentToken;
                    WaitingCandidates.AddWaitingCandidate(candidate);
                    CandidateFactory.RegisterRootCandidate(rootCandidate);
                    Telemetry.TrackStart(rootCandidate);
                    success = true;
                }
                else
                {
                    ExceededLimitPatterns.Add(rootCandidate.PatternId);
                    WasCandidateLimitExceeded = true;
                    success = false;
                }
            }
            else
            {
                fWasTotalCandidateLimitExceededOnCurrentToken = true;
                WasCandidateLimitExceeded = true;
                success = false;
            }
            return success;
        }

        public void AddToPendingHavingCandidates(HavingCandidate candidate)
        {
            RejectionTargetCandidate rejectionTarget = candidate.GetRejectionTargetCandidate();
            rejectionTarget.AddPendingHavingCandidate(candidate);
            PendingHavingCandidates.AddPendingCandidate(candidate);
        }

        public void AddToPendingInsideCandidates(InsideCandidate candidate)
        {
            RejectionTargetCandidate rejectionTarget = candidate.GetRejectionTargetCandidate();
            rejectionTarget.AddPendingInsideCandidate(candidate);
            PendingInsideCandidates.AddPendingCandidate(candidate);
        }

        public void AddToPendingOutsideCandidates(OutsideCandidate candidate)
        {
            RejectionTargetCandidate rejectionTarget = candidate.GetRejectionTargetCandidate();
            rejectionTarget.AddPendingOutsideCandidate(candidate);
            PendingOutsideCandidates.AddPendingCandidate(candidate);
        }

        public void CreatePatternEvent(PatternCandidate patternCandidate)
        {
            var patternEvent = new PatternEvent()
            {
                Pattern = patternCandidate,
                Start = patternCandidate.Start,
                End = patternCandidate.End
            };
            fPatternEventQueue.Enqueue(patternEvent);
        }

        public RootCandidate CreateRootCandidate(TokenExpression tokenExpression, MatchingEvent matchingEvent)
        {
            RootCandidate rootCandidate = tokenExpression.CreateRootCandidate(this);
            Candidate tokenCandidate = tokenExpression.CreateCandidate(this, rootCandidate);
            rootCandidate.CurrentEventObserver = tokenCandidate;
            rootCandidate.Start = matchingEvent.Location;
            rootCandidate.End = matchingEvent.Location;
            rootCandidate.PatternId = tokenExpression.PatternId;
            return rootCandidate;
        }

        public bool CandidateOnNextToken(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression[] nextExpressions, bool[] excludeFlagPerPosition, bool includeOptional,
            bool alwaysCloneCandidateToContinueMatching)
        {
            return fLocalIndexHandler.CandidateOnNextToken(candidate, tokenEvent, nextExpressions,
                excludeFlagPerPosition, includeOptional, alwaysCloneCandidateToContinueMatching);
        }

        public bool CandidateOnNextToken(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression nextExpression, bool includeOptional, bool alwaysCloneCandidateToContinueMatching)
        {
            return fLocalIndexHandler.CandidateOnNextToken(candidate, tokenEvent, nextExpression, includeOptional,
                alwaysCloneCandidateToContinueMatching);
        }

        public bool FieldReferenceCandidateOnFirstToken(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression nextExpression)
        {
            return fFieldReferenceIndexHandler.CandidateOnFirstToken(candidate, tokenEvent, nextExpression);
        }

        public string GetText(TextLocation start, TextLocation end)
        {
            string result = TextSource.GetText(start, end);
            return result;
        }

        // Internal

        private void OnNextToken(TokenEvent tokenEvent)
        {
            // Во время возобновления ожидающих кандидатов добавляются новые кандидаты, которые должны обрабатывать
            // события только со следующей лексемы. Поэтому до обработки ожидающих кандидатов необходимо
            // "засечь" текущее количество активных кандидатов, чтобы игнорировать новых до окончания текущего цикла
            // обработки.
            ActiveCandidates.BeginEnumeration();
            // Для поиска самого первого совпадения шаблона с минимальным Start.TokenNumber необходимо
            // возобновлять всех ожидающих кандидатов этого шаблона, даже после первого (по времени)
            // совпадения, поэтому excludeFlagPerPattern: null.
            fWaitingIndexHandler.ResumeCandidatesWaitingForToken(WaitingCandidates, tokenEvent,
                excludeFlagPerPattern: null);
            if (fWasTotalCandidateLimitExceededOnCurrentToken)
                ResetAllMatchingCandidates();
            else // выхода за лимит числа кандидатов не было
            {
                // Проход только по существующим корневым кандидатам. Если во время обработки существующих
                // добавляются новые кандидаты, то они должны обрабатывать события только со следующей лексемы.
                // Поэтому все новые кандидаты попадают в отдельный список и игнорируются до окончания текущего цикла
                // обработки.
                ActiveCandidates.UpdateOrRemoveEach((RootCandidate currentRootCandidate) =>
                {
                    bool shouldRemove = true;
                    if (!currentRootCandidate.IsCompletedOrWaiting)
                    {
                        tokenEvent.ClearResults();
                        currentRootCandidate.OnNext(tokenEvent);
                        shouldRemove = currentRootCandidate.IsCompletedOrWaiting;
                    }
                    return shouldRemove;
                });
                if (fWasTotalCandidateLimitExceededOnCurrentToken)
                    ResetAllMatchingCandidates();
            }
            bool[] excludeFlagPerPattern = GetExcludeFlagPerPattern();
            fRootIndexHandler.CreateNewRootCandidatesOnToken(SearchQuery.RootIndex, tokenEvent,
                excludeFlagPerPattern);
            if (fWasTotalCandidateLimitExceededOnCurrentToken)
                ResetAllMatchingCandidates();
            else // выхода за лимит числа кандидатов не было
            {
                // Необходимо несколько проходов по условному индексу, т.к. созданные кандидаты
                // могут являться условием для поиска других шаблонов, если последние начинаются со ссылки
                // внутри левой части @having.
                while (fWereConditionalHavingPatternsEnabled)
                {
                    fWereConditionalHavingPatternsEnabled = false;
                    for (int i = 0; i < fConditionalHavingExcludeFlagPerPattern.Length; i++)
                        fExcludeFlagPerPattern[i] = fExcludeFlagPerPattern[i] && fConditionalHavingExcludeFlagPerPattern[i];
                    excludeFlagPerPattern = fConditionalHavingExcludeFlagPerPattern;
                    SwapToCleanConditionalHavingExcludeFlagPerPattern();

                    fRootIndexHandler.CreateNewRootCandidatesOnToken(SearchQuery.ConditionalHavingIndex, tokenEvent,
                        excludeFlagPerPattern);
                    if (fWasTotalCandidateLimitExceededOnCurrentToken)
                        ResetAllMatchingCandidates();
                }
            }
        }

        private bool[] GetExcludeFlagPerPattern()
        {
            bool[] excludeFlagPerPattern;
            if (!SearchOptions.FirstMatchOnly)
            {
                if (!SearchQuery.IsConditionalHavingIndexCreated)
                {
                    excludeFlagPerPattern = fAnySpanExcludeFlagPerPattern;
                }
                else // (!FirstMatchOnly && IsConditionalHavingIndexCreated)
                {
                    for (int i = 0; i < fInternalExcludeFlagPerPattern.Length; i++)
                    {
                        fInternalExcludeFlagPerPattern[i] =
                            fExcludeFlagPerPattern[i] || fAnySpanExcludeFlagPerPattern[i];
                    }
                    excludeFlagPerPattern = fInternalExcludeFlagPerPattern;
                    if (fWereConditionalHavingPatternsEnabled)
                        SwapToCleanConditionalHavingExcludeFlagPerPattern();
                }
            }
            else // (SearchOptions.FirstMatchOnly == true)
            {
                if (!SearchQuery.IsConditionalHavingIndexCreated)
                {
                    for (int i = 0; i < fInternalExcludeFlagPerPattern.Length; i++)
                    {
                        fInternalExcludeFlagPerPattern[i] =
                            WasMatchPerPattern[i] || fAnySpanExcludeFlagPerPattern[i];
                    }
                    excludeFlagPerPattern = fInternalExcludeFlagPerPattern;
                }
                else // (FirstMatchOnly && IsConditionalHavingIndexCreated)
                {
                    for (int i = 0; i < fInternalExcludeFlagPerPattern.Length; i++)
                    {
                        fInternalExcludeFlagPerPattern[i] =
                            fExcludeFlagPerPattern[i] || WasMatchPerPattern[i] || fAnySpanExcludeFlagPerPattern[i];
                    }
                    excludeFlagPerPattern = fInternalExcludeFlagPerPattern;
                }
            }
            return excludeFlagPerPattern;
        }

        private void SwapToCleanConditionalHavingExcludeFlagPerPattern()
        {
            bool[] old = fConditionalHavingExcludeFlagPerPattern;
            for (int i = 0; i < fConditionalHavingExcludeFlagPerPatternShadow.Length; i++)
                fConditionalHavingExcludeFlagPerPatternShadow[i] = true;
            fConditionalHavingExcludeFlagPerPattern = fConditionalHavingExcludeFlagPerPatternShadow;
            fConditionalHavingExcludeFlagPerPatternShadow = old;
        }

        private void OnNextPattern(PatternEvent patternEvent)
        {
            PatternCandidate patternCandidate = patternEvent.Pattern;
            if (patternCandidate.IsFinalMatch)
            {
                var patternExpression = (PatternExpression)patternCandidate.Expression;
                var patternAttributes = SearchQuery.GetPatternAttributes(patternExpression);
                if (patternAttributes.IsInnerContentOfHaving)
                    PendingHavingCandidates.AddInnerPatternCandidate(patternCandidate);
                if (patternAttributes.IsOuterPatternOfInside)
                    PendingInsideCandidates.MatchPendingCandidates(patternCandidate);
                if (patternAttributes.IsOuterPatternOfOutside)
                    PendingOutsideCandidates.AddOuterPatternCandidate(patternCandidate);
                if (patternAttributes.IsTarget)
                {
                    if (SearchOptions.FirstMatchOnly && WasMatchPerPattern[patternExpression.Id])
                    {
                        // Пропускать все совпадения шаблона, кроме первого.
                        // При этом, если пришло совпадение более раннее по Start.TokenNumber,
                        // то необходимо сделать его первым.
                        MatchedTagsOfPattern matchedTagsOfPattern = fMatchedTagsPerPattern[patternExpression.Id];
                        MatchedTag first = matchedTagsOfPattern.MatchedTags[0];
                        if (patternCandidate.Start.TokenNumber < first.Start.TokenNumber
                            || (patternCandidate.Start.TokenNumber == first.Start.TokenNumber
                                && patternCandidate.End.TokenNumber > first.End.TokenNumber))
                        {
                            MatchedTag matchedTag = CreateMatchedTag(patternCandidate);
                            matchedTagsOfPattern.MatchedTags[0] = matchedTag;
                            if (ResultCallback != null)
                            {
                                long cleaningTokenNumber = GetCleaningTokenNumberForPattern(patternExpression.Id);
                                if (matchedTag.Start.TokenNumber < cleaningTokenNumber)
                                {
                                    ResultCallback(SearchEngine, matchedTag);
                                    matchedTag.WasPassedToCallback = true;
                                }
                            }
                        }
                        else if (ResultCallback != null && !first.WasPassedToCallback)
                        {
                            long cleaningTokenNumber = GetCleaningTokenNumberForPattern(patternExpression.Id);
                            if (first.Start.TokenNumber < cleaningTokenNumber)
                            {
                                ResultCallback(SearchEngine, first);
                                first.WasPassedToCallback = true;
                            }
                        }
                    }
                    else
                    {
                        WasMatchPerPattern[patternExpression.Id] = true;
                        MatchedTag matchedTag = CreateMatchedTag(patternCandidate);
                        MatchedTagsOfPattern matchedTagsOfPattern = fMatchedTagsPerPattern[patternExpression.Id];
                        if (matchedTagsOfPattern == null)
                        {
                            matchedTagsOfPattern = new MatchedTagsOfPattern(pattern: patternExpression,
                                selfOverlapping: SearchOptions.SelfOverlappingTagsInResults);
                            fMatchedTagsPerPattern[patternExpression.Id] = matchedTagsOfPattern;
                        }
                        matchedTagsOfPattern.Add(matchedTag);
                        if (!SearchOptions.FirstMatchOnly)
                        {
                            if (!SearchOptions.SelfOverlappingTagsInResults)
                            {
                                if (!fIsProcessingOfRemainingCandidates)
                                {
                                    if (matchedTagsOfPattern.CountOfWaitingForCleanup > SearchOptions.MaxCountOfMatchedTagsWaitingForCleanup)
                                    {
                                        long cleaningTokenNumber = GetCleaningTokenNumberForPattern(patternExpression.Id);
                                        TryRemoveOverlapsAndInvokeResultCallback(matchedTagsOfPattern, cleaningTokenNumber);
                                    }
                                }
                            }
                            else if (ResultCallback != null) // && (SearchOptions.SelfOverlappingTagsInResults == true)
                            {
                                ResultCallback(SearchEngine, matchedTag);
                                matchedTag.WasPassedToCallback = true;
                            }
                        }
                        else if (ResultCallback != null)    // && (SearchOptions.FirstMatchOnly == true)
                        {
                            long cleaningTokenNumber = GetCleaningTokenNumberForPattern(patternExpression.Id);
                            if (matchedTag.Start.TokenNumber < cleaningTokenNumber)
                            {
                                ResultCallback(SearchEngine, matchedTag);
                                matchedTag.WasPassedToCallback = true;
                            }
                        }
                    }
                }
                else // нецелевой шаблон (!SearchQuery.ContainsTargetPattern(patternExpression))
                {
                    // TODO: Обновить после принятия решения по #56 https://lab.nezaboodka.com/nv/nevod/issues/56.
                    // if (SearchOptions.FirstMatchOnly) ...
                    //     Оптимизировать поиск нецелевых шаблонов в режиме FirstMatchOnly.
                }
            }
            if (fWasTotalCandidateLimitExceededOnCurrentToken)
                ResetAllMatchingCandidates();
        }

        private MatchedTag CreateMatchedTag(PatternCandidate patternCandidate)
        {
            var patternExpression = (PatternExpression)patternCandidate.Expression;
            Dictionary<string, IReadOnlyList<MatchedText>> extractions = GetMatchedExtractions(patternCandidate);
            MatchedTag result = new MatchedTag(fTimestamp, patternExpression.Name, extractions,
                TextSource, patternCandidate.Start, patternCandidate.End);
            fTimestamp++;
            return result;
        }

        private Dictionary<string, IReadOnlyList<MatchedText>> GetMatchedExtractions(PatternCandidate patternCandidate)
        {
            // TODO: inner matches
            Dictionary<string, IReadOnlyList<MatchedText>> extractions = null;
            List<ExtractionCandidate> extractionCandidates = patternCandidate.Extractions;
            if (extractionCandidates != null)
            {
                extractions = new Dictionary<string, IReadOnlyList<MatchedText>>();
                FieldExpression[] fields = ((PatternExpression)patternCandidate.Expression).Fields;
                for (int i = 0, n = extractionCandidates.Count; i < n; i++)
                {
                    ExtractionCandidate extractionCandidate = extractionCandidates[i];
                    ExtractionExpression extractionExpression = (ExtractionExpression)extractionCandidate.Expression;
                    FieldExpression field = fields[extractionExpression.FieldNumber];
                    if (!field.IsInternal)
                    {
                        var matchedExtraction = new MatchedText(TextSource,
                            extractionCandidate.Start, extractionCandidate.End);
                        List<MatchedText> list;
                        if (!extractions.TryGetValue(field.Name, out IReadOnlyList<MatchedText> readonlyList))
                        {
                            list = new List<MatchedText>();
                            extractions.Add(field.Name, list);
                        }
                        else
                        {
                            // Приведение типов вызвано несовместимостью словарей по параметру TValue - тип значения
                            list = (List<MatchedText>)readonlyList;
                        }
                        list.Add(matchedExtraction);
                    }
                }
            }
            return extractions;
        }

        private void PerformGarbageCollection()
        {
            WaitingCandidates.RejectCandidatesWithWordLimitExceeded();
            FindCleaningTokenNumberForEachPattern(fCleaningTokenNumberPerPattern);
            TryMatchAndRejectPendingCandidates(fCleaningTokenNumberPerPattern);
            if (!SearchOptions.FirstMatchOnly)
            {
                if (!SearchOptions.SelfOverlappingTagsInResults)
                    TryRemoveOverlapsOfMatchedTagsAndInvokeResultCallback(fCleaningTokenNumberPerPattern);
            }
            else if (ResultCallback != null) // && (SearchOptions.FirstMatchOnly == true)
            {
                TryPassFirstOfMatchedTagsToCallback(fCleaningTokenNumberPerPattern);
            }
            WaitingCandidates.TryRemoveDisposedCandidates();
            CandidateFactory.ResetNewWaitingTokensCount();
        }

        private void PerformGarbageCollecitonOfWaitingCandidates()
        {
            WaitingCandidates.RejectCandidatesWithWordLimitExceeded();
            WaitingCandidates.TryRemoveDisposedCandidates();
            CandidateFactory.ResetNewWaitingTokensCount();
        }

        private void ResetAllMatchingCandidates()
        {
            ActiveCandidates.Reset();
            WaitingCandidates.Reset();
            PendingHavingCandidates.Reset();
            PendingInsideCandidates.Reset();
            PendingOutsideCandidates.Reset();
            CandidateFactory.ResetCandidateCount();
            Telemetry.Reset();
            fWasTotalCandidateLimitExceededOnCurrentToken = false;
        }

        private void ProcessRemainingCandidates()
        {
            fIsProcessingOfRemainingCandidates = true;
            ActiveCandidates.RejectAll();
            WaitingCandidates.RejectAll();
            while (fPatternEventQueue.Count > 0)
                OnNextPattern(fPatternEventQueue.Dequeue());
            FindCleaningTokenNumberForEachPattern(fCleaningTokenNumberPerPattern);
            while (fCleaningTokenNumberPerPattern.Count > 0)
            {
                TryMatchAndRejectPendingCandidates(fCleaningTokenNumberPerPattern);
                while (fPatternEventQueue.Count > 0)
                    OnNextPattern(fPatternEventQueue.Dequeue());
                FindCleaningTokenNumberForEachPattern(fCleaningTokenNumberPerPattern);
            }
            fIsProcessingOfRemainingCandidates = false;
        }

        private void FindCleaningTokenNumberForEachPattern(Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            cleaningTokenNumberPerPattern.Clear();
            ActiveCandidates.ForEach(x => UpdateCleaningTokenNumberForPattern(x, in cleaningTokenNumberPerPattern));
            WaitingCandidates.ForEach(x => UpdateCleaningTokenNumberForPattern(x, in cleaningTokenNumberPerPattern));
            PendingHavingCandidates.ForEach(x => UpdateCleaningTokenNumberForPattern(x, in cleaningTokenNumberPerPattern));
            PendingInsideCandidates.ForEach(x => UpdateCleaningTokenNumberForPattern(x, in cleaningTokenNumberPerPattern));
            PendingOutsideCandidates.ForEach(x => UpdateCleaningTokenNumberForPattern(x, in cleaningTokenNumberPerPattern));
        }

        private void UpdateCleaningTokenNumberForPattern(RootCandidate rootCandidate,
            in Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            if (!rootCandidate.IsFinalMatch)
            {
                if (cleaningTokenNumberPerPattern.TryGetValue(rootCandidate.PatternId, out long cleaningTokenNumber))
                {
                    if (rootCandidate.Start.TokenNumber < cleaningTokenNumber)
                        cleaningTokenNumberPerPattern[rootCandidate.PatternId] = rootCandidate.Start.TokenNumber;
                }
                else
                    cleaningTokenNumberPerPattern[rootCandidate.PatternId] = rootCandidate.Start.TokenNumber;
            }
        }

        private long GetCleaningTokenNumberForPattern(int patternId)
        {
            long cleaningTokenNumber = long.MaxValue;

            void UpdateCleaningTokenNumber(RootCandidate rootCandidate)
            {
                if (rootCandidate.PatternId == patternId && !rootCandidate.IsFinalMatch)
                {
                    if (rootCandidate.Start.TokenNumber < cleaningTokenNumber)
                        cleaningTokenNumber = rootCandidate.Start.TokenNumber;
                }
            }

            ActiveCandidates.ForEach(x => UpdateCleaningTokenNumber(x));
            WaitingCandidates.ForEach(x => UpdateCleaningTokenNumber(x));
            PendingHavingCandidates.ForEach(x => UpdateCleaningTokenNumber(x));
            PendingInsideCandidates.ForEach(x => UpdateCleaningTokenNumber(x));
            PendingOutsideCandidates.ForEach(x => UpdateCleaningTokenNumber(x));
            return cleaningTokenNumber;
        }

        private void TryMatchAndRejectPendingCandidates(Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            PendingHavingCandidates.TryMatchPendingHavingCandidates(cleaningTokenNumberPerPattern);
            PendingInsideCandidates.TryRejectPendingInsideCandidates(cleaningTokenNumberPerPattern);
            PendingOutsideCandidates.TryMatchPendingOutsideCandidates(cleaningTokenNumberPerPattern);
        }

        private void TryRemoveOverlapsOfMatchedTagsAndInvokeResultCallback(
            Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            for (int i = 0, n = fMatchedTagsPerPattern.Length; i < n; i++)
            {
                MatchedTagsOfPattern matchedTagsOfPattern = fMatchedTagsPerPattern[i];
                if (matchedTagsOfPattern != null)
                {
                    long cleaningTokenNumber = cleaningTokenNumberPerPattern.GetValueOrDefault(i, long.MaxValue);
                    TryRemoveOverlapsAndInvokeResultCallback(matchedTagsOfPattern, cleaningTokenNumber);
                }
            }
        }

        private void RemoveOverlapsOfMatchedTagsAndInvokeResultCallback()
        {
            long cleaningTokenNumber = long.MaxValue;
            for (int i = 0, n = fMatchedTagsPerPattern.Length; i < n; i++)
            {
                MatchedTagsOfPattern matchedTagsOfPattern = fMatchedTagsPerPattern[i];
                if (matchedTagsOfPattern != null)
                {
                    TryRemoveOverlapsAndInvokeResultCallback(matchedTagsOfPattern, cleaningTokenNumber);
                }
            }
        }

        private void TryRemoveOverlapsAndInvokeResultCallback(MatchedTagsOfPattern matchedTagsOfPattern,
            long cleaningTokenNumber)
        {
            bool hasNewCleanedMatchedTags = matchedTagsOfPattern.TryRemoveOverlaps(cleaningTokenNumber);
            if (ResultCallback != null && hasNewCleanedMatchedTags)
            {
                matchedTagsOfPattern.ForEachCleaned((MatchedTag matchedTag) =>
                {
                    if (!matchedTag.WasPassedToCallback)
                    {
                        ResultCallback(SearchEngine, matchedTag);
                        matchedTag.WasPassedToCallback = true;
                    }
                });
                matchedTagsOfPattern.RemoveCleanedExceptLast();
            }
        }

        private void TryPassFirstOfMatchedTagsToCallback(Dictionary<int, long> cleaningTokenNumberPerPattern)
        {
            for (int i = 0, n = fMatchedTagsPerPattern.Length; i < n; i++)
            {
                MatchedTagsOfPattern matchedTagsOfPattern = fMatchedTagsPerPattern[i];
                if (matchedTagsOfPattern != null)
                {
                    MatchedTag matchedTag = matchedTagsOfPattern.MatchedTags[0];
                    if (!matchedTag.WasPassedToCallback)
                    {
                        long cleaningTokenNumber = cleaningTokenNumberPerPattern.GetValueOrDefaultNullable(i, long.MaxValue);
                        if (matchedTag.Start.TokenNumber < cleaningTokenNumber)
                        {
                            ResultCallback(SearchEngine, matchedTag);
                            matchedTag.WasPassedToCallback = true;
                        }
                    }
                }
            }
        }
    }
}
