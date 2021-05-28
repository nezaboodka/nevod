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
    internal class SearchPatternAttributes
    {
        public static readonly SearchPatternAttributes Default = new SearchPatternAttributes();

        public bool IsTarget { get; set; }
        public bool IsInnerContentOfHaving { get; set; }
        public bool IsOuterPatternOfInside { get; set; }
        public bool IsOuterPatternOfOutside { get; set; }
    }

    internal sealed class SearchExpression : Expression
    {
        private ExpressionIndex fRootIndex;
        private ExpressionIndex fConditionalHavingIndex;

        public HashSet<PatternExpression> TargetPatterns { get; }
        public int PatternIndexLength { get; }
        public bool[] InitialExcludeFlagPerPattern { get; }
        public Dictionary<PatternExpression, SearchPatternAttributes> AttributesByPattern { get; private set; }
        public HashSet<PatternExpression> UsedPatterns { get; set; }
        public List<PatternReferenceExpression> AllReferences { get; set; }
        public bool IsRootIndexCreated { get; private set; }
        public bool IsConditionalHavingIndexCreated { get; private set; }

        public ExpressionIndex RootIndex
        {
            get
            {
                if (!IsRootIndexCreated)
                    throw new InvalidOperationException($"{nameof(RootIndex)} not initialized");
                return fRootIndex;
            }
            set
            {
                fRootIndex = value;
                IsRootIndexCreated = true;
            }
        }

        public ExpressionIndex ConditionalHavingIndex
        {
            get
            {
                if (!IsConditionalHavingIndexCreated)
                    throw new InvalidOperationException($"{nameof(ConditionalHavingIndex)} not initialized");
                return fConditionalHavingIndex;
            }
            set
            {
                fConditionalHavingIndex = value;
                IsConditionalHavingIndexCreated = true;
            }
        }

        public SearchExpression(Syntax syntax, HashSet<PatternExpression> targetPatterns, int patternIndexLength)
            : base(syntax)
        {
            TargetPatterns = targetPatterns;
            PatternIndexLength = patternIndexLength;
            InitialExcludeFlagPerPattern = new bool[patternIndexLength];
            for (int i = 0; i < InitialExcludeFlagPerPattern.Length; i++)
                InitialExcludeFlagPerPattern[i] = true;
            foreach (var targetPattern in targetPatterns)
            {
                var patternAttributes = AcquirePatternAttributes(targetPattern);
                patternAttributes.IsTarget = true;
            }
        }

        public bool IsTargetPattern(PatternExpression pattern)
        {
            return TargetPatterns.Contains(pattern);
        }

        public SearchPatternAttributes GetPatternAttributes(PatternExpression pattern)
        {
            SearchPatternAttributes result;
            if (AttributesByPattern != null)
            {
                if (!AttributesByPattern.TryGetValue(pattern, out result))
                    result = SearchPatternAttributes.Default;
            }
            else
                throw new InvalidOperationException("AttributesByPattern not initialized.");
            return result;
        }

        public SearchPatternAttributes AcquirePatternAttributes(PatternExpression pattern)
        {
            SearchPatternAttributes result = null;
            if (AttributesByPattern == null)
                AttributesByPattern = new Dictionary<PatternExpression, SearchPatternAttributes>();
            result = AttributesByPattern.GetOrCreate(pattern);
            return result;
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            throw new InvalidOperationException();
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitSearch(this);
        }
    }
}
