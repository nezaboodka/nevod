//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal class NestedIndexBuilder : ExpressionVisitor
    {
        private PatternPackage fPackage;
        private HashSet<PatternExpression> fContextPatterns;  // of Inside, Outside and Having
        private HashSet<PatternExpression> fConditionalHavingInnerContentPatterns;
        private Stack<HavingExpression> fConditionalHavingStack;

        public NestedIndexBuilder()
        {
        }

        public void Build(PatternPackage package)
        {
            fPackage = package;
            fContextPatterns = new HashSet<PatternExpression>();
            fConditionalHavingInnerContentPatterns = new HashSet<PatternExpression>();
            fConditionalHavingStack = new Stack<HavingExpression>();
            Visit(fPackage.SearchQuery);
        }

        // Internal

        protected internal override void VisitSearch(SearchExpression node)
        {
            Visit(node.UsedPatterns);
            // SearchExpression.NestedIndex не используется и не инициализируется
            node.RootIndex = new ExpressionIndex()
                .MergeWithOwnIndexOf(node.TargetPatterns)
                .MergeWithNestedIndexOf(node.TargetPatterns)
                .MergeWithOwnIndexOf(fContextPatterns)
                .MergeWithNestedIndexOf(fContextPatterns);
            if (fConditionalHavingInnerContentPatterns.Count > 0)
            {
                node.ConditionalHavingIndex = new ExpressionIndex()
                    .MergeWithOwnIndexOf(fConditionalHavingInnerContentPatterns)
                    .MergeWithNestedIndexOf(fConditionalHavingInnerContentPatterns);
            }
        }

        protected internal override void VisitPattern(PatternExpression node)
        {
            if (fPackage.SearchQuery.IsTargetPattern(node))
                fPackage.SearchQuery.InitialExcludeFlagPerPattern[node.Id] = false;
            if (node.HasReferences)
            {
                Visit(node.Body);
                node.NestedIndex = node.Body.NestedIndex;
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
            ExpressionIndex succeedingElementNestedIndex = currentElement.NestedIndex;
            current--;
            while (current >= 0)
            {
                currentElement = node.Elements[current];
                Visit(currentElement);
                if (currentElement.IsOptional)
                {
                    currentElement.NestedIndex = currentElement.NestedIndex
                        .MergeIntoOptionalPart(succeedingElementNestedIndex);
                }
                succeedingElementNestedIndex = currentElement.NestedIndex;
                current--;
            }
            node.NestedIndex = new ExpressionIndex()
                .MergeIntoMainPart(currentElement.NestedIndex);
        }

        protected internal override void VisitConjunction(ConjunctionExpression node)
        {
            Visit(node.Elements);
            node.NestedIndex = new ExpressionIndex()
                .MergeWithNestedIndexOf(node.Elements);
        }

        protected internal override void VisitVariation(VariationExpression node)
        {
            Visit(node.Elements);
            node.NestedIndex = new ExpressionIndex()
                .MergeWithNestedIndexOf(node.Elements);
        }

        protected internal override void VisitException(ExceptionExpression node)
        {
            Visit(node.Body);
            node.NestedIndex = node.Body.NestedIndex;
        }

        protected internal override void VisitRepetition(RepetitionExpression node)
        {
            Visit(node.Body);
            node.NestedIndex = node.Body.NestedIndex;
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
                node.Span.NestedIndex = node.Span.NestedIndex
                    .MergeIntoOptionalPart(node.Right.NestedIndex);
            }
            node.NestedIndex = node.Left.NestedIndex;
        }

        protected internal override void VisitAnySpan(AnySpanExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);
            Visit(node.ExtractionOfSpan);
            node.NestedIndex = node.Left.NestedIndex;
        }

        protected internal override void VisitInside(InsideExpression node)
        {
            Visit(node.Body);
            Visit(node.OuterPattern);
            node.NestedIndex = node.Body.NestedIndex;
            PatternExpression outerPattern = node.OuterPattern.ReferencedPattern;
            if (!fPackage.SearchQuery.IsTargetPattern(outerPattern))
            {
                fContextPatterns.Add(outerPattern);
                fPackage.SearchQuery.InitialExcludeFlagPerPattern[outerPattern.Id] = false;
            }
        }

        protected internal override void VisitOutside(OutsideExpression node)
        {
            Visit(node.Body);
            Visit(node.OuterPattern);
            node.NestedIndex = node.Body.NestedIndex;
            PatternExpression outerPattern = node.OuterPattern.ReferencedPattern;
            if (!fPackage.SearchQuery.IsTargetPattern(outerPattern))
            {
                fContextPatterns.Add(outerPattern);
                fPackage.SearchQuery.InitialExcludeFlagPerPattern[outerPattern.Id] = false;
            }
        }

        protected internal override void VisitHaving(HavingExpression node)
        {
            PatternExpression innerContentPattern = node.InnerContent.ReferencedPattern;
            var patternAttributes = fPackage.SearchQuery.GetPatternAttributes(innerContentPattern);
            bool isConditionalHaving = !patternAttributes.IsTarget
                && !patternAttributes.IsOuterPatternOfInside && !patternAttributes.IsOuterPatternOfOutside;
            if (isConditionalHaving)
            {
                fConditionalHavingInnerContentPatterns.Add(innerContentPattern);
                fConditionalHavingStack.Push(node);
                Visit(node.Body);
                fConditionalHavingStack.Pop();
            }
            else
            {
                Visit(node.Body);
            }
            Visit(node.InnerContent);
            node.NestedIndex = node.Body.NestedIndex;
            if (fConditionalHavingStack.TryPeek(out HavingExpression having))
                node.ParentConditionalHaving = having;
            if (!fPackage.SearchQuery.IsTargetPattern(innerContentPattern))
            {
                fContextPatterns.Add(innerContentPattern);
                if (!isConditionalHaving)
                    fPackage.SearchQuery.InitialExcludeFlagPerPattern[innerContentPattern.Id] = false;
            }
        }

        protected internal override void VisitExtraction(ExtractionExpression node)
        {
            Visit(node.Body);
            node.NestedIndex = node.Body?.NestedIndex;
        }

        protected internal override void VisitFieldReference(FieldReferenceExpression node)
        {
            node.NestedIndex = null;
        }

        protected internal override void VisitToken(TokenExpression node)
        {
            node.NestedIndex = null;
            if (fConditionalHavingStack.TryPeek(out HavingExpression having))
                node.ParentConditionalHaving = having;
        }

        protected internal override void VisitPatternReference(PatternReferenceExpression node)
        {
            // Не нужно идти вглубь шаблона по ссылке
        }
    }

    internal static partial class ExpressionIndexExtension
    {
        public static ExpressionIndex MergeWithNestedIndexOf<T>(this ExpressionIndex index, IList<T> expressions)
            where T : Expression
        {
            for (int i = 0, n = expressions.Count; i < n; i++)
            {
                Expression element = expressions[i];
                if (element is ExceptionExpression)
                    index = index.MergeIntoMainPartFromException(element.NestedIndex);
                else
                    index = index.MergeIntoMainPart(element.NestedIndex);
            }
            return index;
        }

        public static ExpressionIndex MergeWithNestedIndexOf<T>(this ExpressionIndex index, IEnumerable<T> expressions)
            where T : Expression
        {
            foreach (Expression element in expressions)
            {
                if (element.IsNestedIndexCreated)
                {
                    if (element is ExceptionExpression)
                        index = index.MergeIntoMainPartFromException(element.NestedIndex);
                    else
                        index = index.MergeIntoMainPart(element.NestedIndex);
                }
            }
            return index;
        }
    }
}
