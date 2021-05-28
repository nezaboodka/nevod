//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal abstract class ExpressionVisitor
    {
        public virtual void Visit(Expression node)
        {
            if (node != null)
                node.Accept(this);
        }

        public void Visit<T>(IList<T> nodes) where T : Expression
        {
            for (int i = 0, n = nodes.Count; i < n; i++)
                Visit(nodes[i]);
        }

        public void Visit<T>(IEnumerable<T> nodes) where T : Expression
        {
            foreach (T node in nodes)
                Visit(node);
        }

        protected internal virtual void VisitExpression(Expression node)
        {
            throw new InvalidOperationException();
        }

        protected internal virtual void VisitSearch(SearchExpression node)
        {
            Visit(node.TargetPatterns);
        }

        protected internal virtual void VisitPattern(PatternExpression node)
        {
            Visit(node.Body);
            Visit(node.Fields);
        }

        protected internal virtual void VisitField(FieldExpression node)
        {
        }

        protected internal virtual void VisitSequence(SequenceExpression node)
        {
            Visit(node.Elements);
        }

        protected internal virtual void VisitConjunction(ConjunctionExpression node)
        {
            Visit(node.Elements);
        }

        protected internal virtual void VisitVariation(VariationExpression node)
        {
            Visit(node.Elements);
        }

        protected internal virtual void VisitException(ExceptionExpression node)
        {
            Visit(node.Body);
        }

        protected internal virtual void VisitRepetition(RepetitionExpression node)
        {
            Visit(node.Body);
        }

        protected internal virtual void VisitWordSpan(WordSpanExpression node)
        {
            Visit(node.Left);
            Visit(node.Span);
            Visit(node.Exclusion);
            Visit(node.Right);
            Visit(node.ExtractionOfSpan);
        }

        protected internal virtual void VisitAnySpan(AnySpanExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);
            Visit(node.ExtractionOfSpan);
        }

        protected internal virtual void VisitInside(InsideExpression node)
        {
            Visit(node.Body);
            Visit(node.OuterPattern);
        }

        protected internal virtual void VisitOutside(OutsideExpression node)
        {
            Visit(node.Body);
            Visit(node.OuterPattern);
        }

        protected internal virtual void VisitHaving(HavingExpression node)
        {
            Visit(node.Body);
            Visit(node.InnerContent);
        }

        protected internal virtual void VisitExtraction(ExtractionExpression node)
        {
            Visit(node.Body);
        }

        protected internal virtual void VisitFieldReference(FieldReferenceExpression node)
        {
        }

        protected internal virtual void VisitToken(TokenExpression node)
        {
        }

        protected internal virtual void VisitPatternReference(PatternReferenceExpression node)
        {
        }
    }
}
