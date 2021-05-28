//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class CandidateFactory
    {
        public int TotalCandidateCount { get; private set; }
        public int[] CandidateCountPerPattern { get; private set; }
        public int NewWaitingTokensCount { get; private set; }

        public CandidateFactory(int patternIndexLength)
        {
            CandidateCountPerPattern = new int[patternIndexLength];
        }

        public void ResetCandidateCount()
        {
            TotalCandidateCount = 0;
            Array.Clear(CandidateCountPerPattern, 0, CandidateCountPerPattern.Length);
            NewWaitingTokensCount = 0;
        }

        public void RegisterRootCandidate(RootCandidate root)
        {
            if (!root.IsRegistered)
            {
                CandidateCountPerPattern[root.PatternId]++;
                TotalCandidateCount++;
                root.IsRegistered = true;
            }
        }

        public void UnregisterRootCandidate(RootCandidate root)
        {
            if (root.IsRegistered)
            {
                if (CandidateCountPerPattern[root.PatternId] == 0 || TotalCandidateCount == 0)
                    throw new InvalidOperationException();
                CandidateCountPerPattern[root.PatternId]--;
                TotalCandidateCount--;
                root.IsRegistered = false;
            }
        }

        public WaitingToken CreateWaitingToken(TokenExpression tokenExpression, AnySpanCandidate candidate,
            bool isException)
        {
            var result = new WaitingToken(tokenExpression, candidate, isException);
            NewWaitingTokensCount++;
            return result;
        }

        public void ResetNewWaitingTokensCount()
        {
            NewWaitingTokensCount = 0;
        }

        public PatternCandidate CreatePatternCandidate(PatternExpression expression)
        {
            var result = new PatternCandidate(expression);
            return result;
        }

        public ConjunctionCandidate CreateConjunctionCandidate(ConjunctionExpression expression)
        {
            var result = new ConjunctionCandidate(expression);
            return result;
        }

        public InsideCandidate CreateInsideCandidate(InsideExpression expression)
        {
            var result = new InsideCandidate(expression);
            return result;
        }

        public OutsideCandidate CreateOutsideCandidate(OutsideExpression expression)
        {
            var result = new OutsideCandidate(expression);
            return result;
        }

        public ExceptionCandidate CreateExceptionCandidate(ExceptionExpression expression)
        {
            var result = new ExceptionCandidate(expression);
            return result;
        }

        public ExceptionStubCandidate CreateExceptionStubCandidate(RootCandidate rootCandidate)
        {
            var result = new ExceptionStubCandidate((CompoundExpression)rootCandidate.Expression);
            result.SearchContext = rootCandidate.SearchContext;
            // для связывания исключения с целевыми кандидатами при восстановлении из ожидания
            result.PatternId = rootCandidate.PatternId;
            result.Start = rootCandidate.Start;
            return result;
        }

        public HavingCandidate CreateHavingCandidate(HavingExpression expression)
        {
            var result = new HavingCandidate(expression);
            return result;
        }

        public PatternReferenceCandidate CreatePatternReferenceCandidate(PatternReferenceExpression expression)
        {
            var result = new PatternReferenceCandidate(expression);
            return result;
        }

        public RepetitionCandidate CreateRepetitionCandidate(RepetitionExpression expression)
        {
            var result = new RepetitionCandidate(expression);
            return result;
        }

        public SequenceCandidate CreateSequenceCandidate(SequenceExpression expression)
        {
            var result = new SequenceCandidate(expression);
            return result;
        }

        public WordSpanCandidate CreateWordSpanCandidate(WordSpanExpression expression)
        {
            var result = new WordSpanCandidate(expression);
            return result;
        }

        public AnySpanCandidate CreateAnySpanCandidate(AnySpanExpression expression)
        {
            var result = new AnySpanCandidate(expression);
            return result;
        }

        public TokenCandidate CreateTokenCandidate(TokenExpression expression)
        {
            var result = new TokenCandidate(expression);
            return result;
        }

        public VariationCandidate CreateVariationCandidate(VariationExpression expression)
        {
            var result = new VariationCandidate(expression);
            return result;
        }

        public ExtractionCandidate CreateExtractionCandidate(ExtractionExpression expression)
        {
            var result = new ExtractionCandidate(expression);
            return result;
        }

        public FieldReferenceCandidate CreateFieldReferenceCandidate(FieldReferenceExpression expression)
        {
            var result = new FieldReferenceCandidate(expression);
            return result;
        }
    }
}
