//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class PatternReferenceSubstitutor : SyntaxVisitor
    {
        private HashSet<PatternSyntax> fExcludedPatterns;
        private HashSet<PatternSyntax> fVisitedPatterns;
        private Dictionary<PatternSyntax, PatternSyntax> fPatternSubstitutions;

        public PatternReferenceSubstitutor()
        {
        }

        public LinkedPackageSyntax SubstitutePatternReferences(LinkedPackageSyntax syntaxTree,
            HashSet<PatternSyntax> excludedPatterns)
        {
            fExcludedPatterns = excludedPatterns;
            fVisitedPatterns = new HashSet<PatternSyntax>();
            fPatternSubstitutions = new Dictionary<PatternSyntax, PatternSyntax>();
            LinkedPackageSyntax substitutedTree = (LinkedPackageSyntax)Visit(syntaxTree);
            fPatternSubstitutions.Clear();
            var linker = new NormalizingPatternLinker(linkRequiredPackages: true);
            LinkedPackageSyntax result = linker.Link(substitutedTree);
            return result;
        }

        public override Syntax Visit(Syntax node)
        {
            Syntax result = base.Visit(node);
            if (result is not null)
            {
                if (result.CanReduce)
                    result = result.Reduce();
                result.TextRange = node.TextRange;
            }
            return result;
        }

        protected internal override Syntax VisitLinkedPackage(LinkedPackageSyntax node)
        {
            ReadOnlyCollection<RequiredPackageSyntax> requiredPackages = Visit(node.RequiredPackages);
            ReadOnlyCollection<Syntax> patterns = Visit(node.Patterns);
            Syntax result = node.Update(requiredPackages, node.SearchTargets, patterns);
            return result;
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            PatternSyntax result = node;
            if (fVisitedPatterns.Add(node))
            {
                result = (PatternSyntax)base.VisitPattern(node);
                fPatternSubstitutions[node] = result;
            }
            else
                result = fPatternSubstitutions[node];
            return result;
        }

        protected internal override Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            Syntax result = node;
            if (!fExcludedPatterns.Contains(node.ReferencedPattern))
            {
                PatternSyntax updatedPattern = (PatternSyntax)Visit(node.ReferencedPattern);
                if (node.ReferencedPattern.Fields.Count > 0)
                    result = Syntax.EmbeddedPatternReference(updatedPattern, node.ExtractionFromFields);
                else
                    result = updatedPattern.Body;
            }
            return result;
        }

        protected internal override Syntax VisitInside(InsideSyntax node)
        {
            Syntax newInner = Visit(node.Inner);
            Syntax newOuter = node.Outer;
            // Не выполнять подстановку ссылок, если внешний контекст это ссылка на шаблон
            if (!(node.Outer is PatternReferenceSyntax))
                newOuter = Visit(node.Outer);
            Syntax result = node.Update(newInner, newOuter);
            return result;
        }

        protected internal override Syntax VisitOutside(OutsideSyntax node)
        {
            Syntax newBody = Visit(node.Body);
            Syntax newException = node.Exception;
            // Не выполнять подстановку ссылок, если внешний контекст это ссылка на шаблон
            if (!(node.Exception is PatternReferenceSyntax))
                newException = Visit(node.Exception);
            Syntax result = node.Update(newBody, newException);
            return result;
        }

        protected internal override Syntax VisitHaving(HavingSyntax node)
        {
            Syntax newOuter = Visit(node.Outer);
            Syntax newInner = node.Inner;
            // Не выполнять подстановку ссылок, если внутренний контекст это ссылка на шаблон
            if (!(node.Inner is PatternReferenceSyntax))
                newInner = Visit(node.Inner);
            Syntax result = node.Update(newOuter, newInner);
            return result;
        }
    }
}
