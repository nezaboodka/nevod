//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class OwnIndexBuilder : ExpressionVisitor
    {
        private PatternPackage fPackage;
        private Stack<PatternExpression> fPatternStack;
        private Stack<RootExpression> fRootStack;
        private Stack<VariationExpression> fVariationWithExceptionsStack;
        private HashSet<PatternExpression> fVisitedPatterns;
        private List<PatternReferenceExpression> fVisitedReferences;

        public OwnIndexBuilder()
        {
        }

        public void Build(PatternPackage package)
        {
            fPackage = package;
            fPatternStack = new Stack<PatternExpression>();
            fRootStack = new Stack<RootExpression>();
            fVariationWithExceptionsStack = new Stack<VariationExpression>();
            fVisitedPatterns = new HashSet<PatternExpression>();
            fVisitedReferences = new List<PatternReferenceExpression>();
            Visit(fPackage.SearchQuery);
        }

        // Internal

        protected internal override void VisitSearch(SearchExpression node)
        {
            Visit(node.TargetPatterns);
            // SearchExpression.OwnIndex не используется и не инициализируется
            node.UsedPatterns = fVisitedPatterns;
            node.AllReferences = fVisitedReferences;
        }

        protected internal override void VisitPattern(PatternExpression node)
        {
            if (fVisitedPatterns.Add(node))
            {
                fPatternStack.Push(node);
                fRootStack.Push(node);
                Visit(node.Body);
                node.OwnIndex = node.Body.OwnIndex;
                fRootStack.Pop();
                fPatternStack.Pop();
            }
        }

        protected internal override void VisitSequence(SequenceExpression node)
        {
            // При построении индекса элементов цепочки необходимо учитывать их опциональность.
            // Если элемент цепочки в позиции i опциональный, то это означает, что, после совпадения элемента цепочки
            // в позиции i - 1, элемент в позиции i может быть пропущен, т.е. далее может совпадать как данный
            // элемент, так и элемент в позиции i + 1.
            // Но если опциональным элементом цепочки является повторитель, то для каждого нового повторения
            // необходимо использовать только индекс, который относится к самому повторителю.
            // Для реализации такого поведения для элемента цепочки вводится дополнительная часть индекса,
            // в которую добавляется индекс элемента в следующей за ним позиции.
            // Учитывая, что элемент цепочки в позиции i + 1 также может быть пропущен (после совпадения элемента
            // в позиции i - 1 может быть пропущено несколько следующих позиций цепочки), в дополнительную часть
            // индекса элемента в позиции i необходимо добавлять как основную, так и дополнительную части индекса
            // элемента в позиции i + 1.
            // При этом дополнительная часть индекса элемента в позиции i + 1 не должна содержать индекс элемента
            // в позиции i.
            // Также необходимо учитывать, что элемент в первой позиции цепочки может быть пропущен, и цепочка начинает
            // совпадать со второй позиции. Для обработки такой ситуации, основная часть индекса собственно цепочки
            // должна содержать как основную, так и дополнительную части индекса её первого элемента.
            int current = node.Elements.Length - 1;
            Expression currentElement = node.Elements[current];
            Visit(currentElement);
            ExpressionIndex succeedingElementOwnIndex = currentElement.OwnIndex;
            current--;
            while (current >= 0)
            {
                currentElement = node.Elements[current];
                Visit(currentElement);
                if (currentElement.IsOptional)
                {
                    currentElement.OwnIndex = currentElement.OwnIndex
                        .MergeIntoOptionalPart(succeedingElementOwnIndex);
                }
                succeedingElementOwnIndex = currentElement.OwnIndex;
                current--;
            }
            node.OwnIndex = new ExpressionIndex()
                .MergeIntoMainPart(currentElement.OwnIndex);
        }

        protected internal override void VisitConjunction(ConjunctionExpression node)
        {
            Visit(node.Elements);
            node.OwnIndex = new ExpressionIndex()
                .MergeWithOwnIndexOf(node.Elements);
        }

        protected internal override void VisitVariation(VariationExpression node)
        {
            if (node.HasExceptions)
            {
                fVariationWithExceptionsStack.Push(node);
                Visit(node.Elements);
                fVariationWithExceptionsStack.Pop();
            }
            else
            {
                Visit(node.Elements);
            }
            node.OwnIndex = new ExpressionIndex()
                .MergeWithOwnIndexOf(node.Elements);
            if (fVariationWithExceptionsStack.TryPeek(out VariationExpression variation))
                node.ParentVariationWithExceptions = variation;
        }

        protected internal override void VisitException(ExceptionExpression node)
        {
            fRootStack.Push(node);
            Visit(node.Body);
            node.OwnIndex = node.Body.OwnIndex;
            fRootStack.Pop();
            node.RootExpression = fRootStack.Peek();
        }

        protected internal override void VisitRepetition(RepetitionExpression node)
        {
            Visit(node.Body);
            node.OwnIndex = node.Body.OwnIndex;
        }

        protected internal override void VisitWordSpan(WordSpanExpression node)
        {
            Visit(node.Left);
            Visit(node.Span);
            Visit(node.Exclusion);
            Visit(node.Right);
            Visit(node.ExtractionOfSpan);
            if (node.SpanRangeInWords.LowBound == 0)
            {
                node.Span.OwnIndex = node.Span.OwnIndex
                    .MergeIntoOptionalPart(node.Right.OwnIndex);
            }
            node.OwnIndex = node.Left.OwnIndex;
        }

        protected internal override void VisitAnySpan(AnySpanExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);
            Visit(node.ExtractionOfSpan);
            node.OwnIndex = node.Left.OwnIndex;
        }

        protected internal override void VisitInside(InsideExpression node)
        {
            Visit(node.Body);
            Visit(node.OuterPattern);
            node.OwnIndex = node.Body.OwnIndex;
            var pattern = node.OuterPattern.ReferencedPattern;
            var patternAttributes = fPackage.SearchQuery.AcquirePatternAttributes(pattern);
            patternAttributes.IsOuterPatternOfInside = true;
        }

        protected internal override void VisitOutside(OutsideExpression node)
        {
            Visit(node.Body);
            Visit(node.OuterPattern);
            node.OwnIndex = node.Body.OwnIndex;
            var pattern = node.OuterPattern.ReferencedPattern;
            var patternAttributes = fPackage.SearchQuery.AcquirePatternAttributes(pattern);
            patternAttributes.IsOuterPatternOfOutside = true;
        }

        protected internal override void VisitHaving(HavingExpression node)
        {
            Visit(node.Body);
            Visit(node.InnerContent);
            node.OwnIndex = node.Body.OwnIndex;
            var pattern = node.InnerContent.ReferencedPattern;
            var patternAttributes = fPackage.SearchQuery.AcquirePatternAttributes(pattern);
            patternAttributes.IsInnerContentOfHaving = true;
        }

        protected internal override void VisitExtraction(ExtractionExpression node)
        {
            Visit(node.Body);
            node.OwnIndex = node.Body?.OwnIndex;
        }

        protected internal override void VisitFieldReference(FieldReferenceExpression node)
        {
            Visit(node.Body);
            node.OwnIndex = node.Body.OwnIndex;
        }

        protected internal override void VisitToken(TokenExpression node)
        {
            node.OwnIndex = ExpressionIndex.CreateFromToken(node);
            node.RootExpression = fRootStack.Peek();
            node.PatternId = fPatternStack.Peek().Id;
            if (fVariationWithExceptionsStack.TryPeek(out VariationExpression variation))
                node.ParentVariationWithExceptions = variation;
        }

        protected internal override void VisitPatternReference(PatternReferenceExpression node)
        {
            PatternExpression pattern = fPatternStack.Peek();
            pattern.HasReferences = true;
            node.PatternId = pattern.Id;
            node.OwnIndex = ExpressionIndex.CreateFromReference(node);
            if (!node.ReferencedPattern.IsOwnIndexCreated)
                Visit(node.ReferencedPattern);
            fVisitedReferences.Add(node);
        }
    }

    internal static partial class ExpressionIndexExtension
    {
        public static ExpressionIndex MergeWithOwnIndexOf<T>(this ExpressionIndex index, IList<T> expressions)
            where T : Expression
        {
            for (int i = 0, n = expressions.Count; i < n; i++)
            {
                Expression element = expressions[i];
                if (element is ExceptionExpression)
                    index = index.MergeIntoMainPartFromException(element.OwnIndex);
                else
                    index = index.MergeIntoMainPart(element.OwnIndex);
            }
            return index;
        }

        public static ExpressionIndex MergeWithOwnIndexOf<T>(this ExpressionIndex index, IEnumerable<T> expressions)
            where T : Expression
        {
            foreach (Expression element in expressions)
            {
                if (element is ExceptionExpression)
                    index = index.MergeIntoMainPartFromException(element.OwnIndex);
                else
                    index = index.MergeIntoMainPart(element.OwnIndex);
            }
            return index;
        }
    }
}
