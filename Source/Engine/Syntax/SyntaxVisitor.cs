//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Nezaboodka.Nevod
{
    public abstract class SyntaxVisitor
    {
        public virtual Syntax Visit(Syntax node)
        {
            Syntax result = null;
            if (node != null)
                result = node.Accept(this);
            return result;
        }

        public ReadOnlyCollection<T> Visit<T>(ReadOnlyCollection<T> nodes) where T : Syntax
        {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                T node = (T)Visit(nodes[i]);
                if (newNodes != null)
                    newNodes[i] = node;
                else if (!object.ReferenceEquals(node, nodes[i]))
                {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++)
                        newNodes[j] = nodes[j];
                    newNodes[i] = node;
                }
            }
            ReadOnlyCollection<T> result;
            if (newNodes == null)
                result = nodes;
            else
                result = new ReadOnlyCollection<T>(newNodes);
            return result;
        }

        public static ReadOnlyCollection<T> Visit<T>(ReadOnlyCollection<T> nodes, Func<T, T> elementVisitor)
        {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                T node = elementVisitor(nodes[i]);
                if (newNodes != null)
                    newNodes[i] = node;
                else if (!object.ReferenceEquals(node, nodes[i]))
                {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++)
                        newNodes[j] = nodes[j];
                    newNodes[i] = node;
                }
            }
            ReadOnlyCollection<T> result;
            if (newNodes == null)
                result = nodes;
            else
                result = new ReadOnlyCollection<T>(newNodes);
            return result;
        }

        protected internal virtual Syntax VisitSyntax(Syntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitPackage(PackageSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitLinkedPackage(LinkedPackageSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            LinkedPackageSyntax package = (LinkedPackageSyntax)Visit(node.Package);
            Syntax result = node.Update(package);
            return result;
        }

        protected internal virtual Syntax VisitSearchTarget(SearchTargetSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitNamespaceSearchTarget(NamespaceSearchTargetSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitPattern(PatternSyntax node)
        {
            Syntax newBody = Visit(node.Body);
            ReadOnlyCollection<FieldSyntax> newFields = Visit(node.Fields);
            ReadOnlyCollection<PatternSyntax> newNestedPatterns = Visit(node.NestedPatterns);
            Syntax result = node.Update(newBody, newFields, newNestedPatterns);
            return result;
        }

        protected internal virtual Syntax VisitField(FieldSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitEmbeddedPatternReference(EmbeddedPatternReferenceSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitFieldReference(FieldReferenceSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitExtraction(ExtractionSyntax node)
        {
            Syntax body = Visit(node.Body);
            Syntax result = node.Update(node.FieldName, body);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitExtractionFromField(ExtractionFromFieldSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitSequence(SequenceSyntax node)
        {
            ReadOnlyCollection<Syntax> newElements = Visit(node.Elements);
            Syntax result;
            if (newElements.Count == 1)
                result = newElements[0];
            else
                result = node.Update(newElements);
            return result;
        }

        protected internal virtual Syntax VisitWordSequence(WordSequenceSyntax node)
        {
            ReadOnlyCollection<Syntax> newElements = Visit(node.Elements);
            Syntax result;
            if (newElements.Count == 1)
                result = newElements[0];
            else
                result = node.Update(newElements);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitConjunction(ConjunctionSyntax node)
        {
            ReadOnlyCollection<Syntax> newElements = Visit(node.Elements);
            Syntax result;
            if (newElements.Count == 1)
                result = newElements[0];
            else
                result = node.Update(newElements);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitVariation(VariationSyntax node)
        {
            ReadOnlyCollection<Syntax> newElements = Visit(node.Elements);
            Syntax result;
            if (newElements.Count == 1)
                result = newElements[0];
            else
                result = node.Update(newElements);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitSpan(SpanSyntax node)
        {
            ReadOnlyCollection<Syntax> newElements = Visit(node.Elements);
            Syntax result = node.Update(newElements);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitRepetition(RepetitionSyntax node)
        {
            Syntax newBody = Visit(node.Body);
            Range range = node.RepetitionRange;
            Syntax result = node.Update(newBody, range);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitOptionality(OptionalitySyntax node)
        {
            Syntax body = Visit(node.Body);
            Syntax result = node.Update(body);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitException(ExceptionSyntax node)
        {
            Syntax body = Visit(node.Body);
            Syntax result = node.Update(body);
            return result;
        }

        protected internal virtual Syntax VisitWordSpan(WordSpanSyntax node)
        {
            Syntax newLeft = Visit(node.Left);
            Syntax newRight = Visit(node.Right);
            Syntax newExclusion = Visit(node.Exclusion);
            Syntax newExtractionOfSpan = Visit(node.ExtractionOfSpan);
            Syntax result = node.Update(newLeft, newRight, newExclusion, newExtractionOfSpan);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitAnySpan(AnySpanSyntax node)
        {
            Syntax newLeft = Visit(node.Left);
            Syntax newRight = Visit(node.Right);
            Syntax newExtractionOfSpan = Visit(node.ExtractionOfSpan);
            Syntax result = node.Update(newLeft, newRight, newExtractionOfSpan);
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitInside(InsideSyntax node)
        {
            Syntax newInner = Visit(node.Inner);
            Syntax newOuter = Visit(node.Outer);
            Syntax result = node.Update(newInner, newOuter);
            return result;
        }

        protected internal virtual Syntax VisitOutside(OutsideSyntax node)
        {
            Syntax newBody = Visit(node.Body);
            Syntax newException = Visit(node.Exception);
            Syntax result = node.Update(newBody, newException);
            return result;
        }

        protected internal virtual Syntax VisitHaving(HavingSyntax node)
        {
            Syntax newOuter = Visit(node.Outer);
            Syntax newInner = Visit(node.Inner);
            Syntax result = node.Update(newOuter, newInner);
            return result;
        }

        protected internal virtual Syntax VisitText(TextSyntax node)
        {
            Syntax result = node;
            if (result.CanReduce)
                result = result.Reduce();
            return result;
        }

        protected internal virtual Syntax VisitToken(TokenSyntax node)
        {
            return node;
        }

        protected internal virtual Syntax VisitDefault(DefaultSyntax node)
        {
            return node;
        }

        protected Exception SyntaxError(string format, params object[] args)
        {
            return new SyntaxException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), 0,
                0, string.Empty);
        }
    }
}
