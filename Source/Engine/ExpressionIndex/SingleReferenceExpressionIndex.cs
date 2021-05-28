//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal class SingleReferenceExpressionIndex : IReferenceExpressionIndex, IEnumerable<PatternReferenceExpression>
    {
        public PatternReferenceExpression ReferenceExpression { get; protected set; }

        public PatternExpression ReferencedPattern => ReferenceExpression.ReferencedPattern;

        public SingleReferenceExpressionIndex(PatternReferenceExpression referenceExpression)
        {
            ReferenceExpression = referenceExpression;
        }

        public IReferenceExpressionIndex Clone()
        {
            var result = new SingleReferenceExpressionIndex(this.ReferenceExpression);
            return result;
        }

        public void SelectMatchingExpressions(int patternId, bool[] excludeFlagPerPattern,
            HashSet<PatternReferenceExpression> result)
        {
            if (excludeFlagPerPattern != null && excludeFlagPerPattern[ReferenceExpression.PatternId])
            {
                // не добавлять ReferenceExpression в результат
            }
            else
            {
                if (patternId == ReferencedPattern.Id)
                    result.Add(ReferenceExpression);
            }
        }

        public IEnumerator<PatternReferenceExpression> GetEnumerator()
        {
            yield return ReferenceExpression;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return ReferenceExpression;
        }

        public IReferenceExpressionIndex MergeFrom(IReferenceExpressionIndex anotherIndex)
        {
            IReferenceExpressionIndex result = null;
            switch (anotherIndex)
            {
                case DictionaryBasedReferenceExpressionIndex anotherDictionaryBasedIndex:
                    result = new DictionaryBasedReferenceExpressionIndex(anotherDictionaryBasedIndex);
                    break;
                case SingleReferenceExpressionIndex anotherSingleExpressionIndex:
                    result = new DictionaryBasedReferenceExpressionIndex();
                    result = result.MergeFrom(anotherSingleExpressionIndex);
                    break;
            }
            if (result != null)
                result = result.MergeFrom(this);
            return result;
        }
    }
}
