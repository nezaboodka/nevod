//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal class DictionaryBasedReferenceExpressionIndex : IReferenceExpressionIndex
    {
        private static readonly Func<Dictionary<int, List<PatternReferenceExpression>>> IndexConstructor =
            () => new Dictionary<int, List<PatternReferenceExpression>>();

        public Dictionary<int, List<PatternReferenceExpression>> Index { get; }

        // Public

        public DictionaryBasedReferenceExpressionIndex()
        {
            Index = new Dictionary<int, List<PatternReferenceExpression>>();
        }

        public DictionaryBasedReferenceExpressionIndex(DictionaryBasedReferenceExpressionIndex anotherIndex)
        {
            Index = anotherIndex.Index.CloneWithValueListsNullable();
        }

        public IReferenceExpressionIndex Clone()
        {
            var result = new DictionaryBasedReferenceExpressionIndex(this);
            return result;
        }

        public void SelectMatchingExpressions(int patternId, bool[] excludeFlagPerPattern,
            HashSet<PatternReferenceExpression> result)
        {
            List<PatternReferenceExpression> list = Index.GetValueOrDefaultNullable(patternId);
            if (list != null)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    PatternReferenceExpression referenceExpression = list[i];
                    if (excludeFlagPerPattern != null && excludeFlagPerPattern[referenceExpression.PatternId])
                    {
                        // не добавлять referenceExpression в результат
                    }
                    else
                    {
                        result.Add(referenceExpression);
                    }
                }
            }
        }

        public IEnumerator<PatternReferenceExpression> GetEnumerator()
        {
            if (Index != null)
            {
                foreach (List<PatternReferenceExpression> references in Index.Values)
                {
                    for (int i = 0, n = references.Count; i < n; i++)
                    {
                        PatternReferenceExpression reference = references[i];
                        yield return reference;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IReferenceExpressionIndex MergeFrom(IReferenceExpressionIndex anotherIndex)
        {
            switch (anotherIndex)
            {
                case DictionaryBasedReferenceExpressionIndex anotherDictionaryBasedReferenceExpressionIndex:
                    Index.MergeFromNullable(anotherDictionaryBasedReferenceExpressionIndex.Index,
                        IndexConstructor);
                    break;
                case SingleReferenceExpressionIndex anotherSingleReferenceExpressionIndex:
                    Index.AddValueListItem(anotherSingleReferenceExpressionIndex.ReferencedPattern.Id,
                        anotherSingleReferenceExpressionIndex.ReferenceExpression);
                    break;
                default:
                    break;
            }
            return this;
        }
    }
}
