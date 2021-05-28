//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal class ReferencesNestedIndexBuilder : ExpressionVisitor
    {
        private PatternPackage fPackage;
        private HashSet<PatternExpression> fVisitedPatterns;
        private ExpressionIndex fCurrentNestedIndex;

        public ReferencesNestedIndexBuilder()
        {
        }

        public void Build(PatternPackage package)
        {
            fPackage = package;
            fCurrentNestedIndex = null;
            fVisitedPatterns = new HashSet<PatternExpression>();
            Visit(fPackage.SearchQuery);
        }

        // Internal

        protected internal override void VisitSearch(SearchExpression node)
        {
            List<PatternReferenceExpression> references = node.AllReferences;
            for (int i = 0, n = references.Count; i < n; i++)
            {
                PatternReferenceExpression reference = references[i];
                Visit(reference);
                reference.NestedIndex = fCurrentNestedIndex;
                fCurrentNestedIndex = null;
                fVisitedPatterns.Clear();
            }
        }

        protected internal override void VisitPattern(PatternExpression node)
        {
            if (fVisitedPatterns.Add(node))
            {
                if (node.OwnIndex.ReferenceIndex != null)
                {
                    foreach (PatternReferenceExpression reference in node.OwnIndex.ReferenceIndex)
                        Visit(reference);
                }
            }
        }

        protected internal override void VisitPatternReference(PatternReferenceExpression node)
        {
            Visit(node.ReferencedPattern);
            fCurrentNestedIndex = fCurrentNestedIndex
                .MergeIntoMainPart(node.ReferencedPattern.OwnIndex);
        }
    }
}
