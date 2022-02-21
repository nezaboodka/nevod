//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    public abstract class SearchTargetSyntax : Syntax
    {
        public string SearchTarget { get; }
        public string Namespace { get; }

        internal SearchTargetSyntax(string searchTarget, string nameSpace)
        {
            SearchTarget = searchTarget;
            Namespace = nameSpace != null ? nameSpace : string.Empty;
        }
    }

    public class PatternSearchTargetSyntax : SearchTargetSyntax
    {
        public new PatternReferenceSyntax PatternReference { get; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            if (PatternReference != null)
            {
                SyntaxUtils.CreateChildrenForRange(TextRange.Start, PatternReference.TextRange.Start, children, scanner);
                children.Add(PatternReference);
                SyntaxUtils.CreateChildrenForRange(PatternReference.TextRange.End, TextRange.End, children, scanner);
            }
            else
                SyntaxUtils.CreateChildrenForRange(TextRange, children, scanner);
            Children = children.AsReadOnly();
        }

        internal PatternSearchTargetSyntax(string patternName, string nameSpace, PatternReferenceSyntax patternReference)
            : base(patternName, nameSpace)
        {
            PatternReference = patternReference;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitPatternSearchTarget(this);
        }
    }

    public class NamespaceSearchTargetSyntax : SearchTargetSyntax
    {
        public string PatternsNamespace { get; }
        public ReadOnlyCollection<Syntax> PatternReferences { get; internal set; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            SyntaxUtils.CreateChildrenForRange(TextRange, children, scanner);
            Children = children.AsReadOnly();
        }

        internal NamespaceSearchTargetSyntax(string patternsNameSpace, string nameSpace)
            : base(patternsNameSpace + ".*", nameSpace)
        {
            PatternsNamespace = patternsNameSpace;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitNamespaceSearchTarget(this);
        }
    }

    public partial class Syntax
    {
        public static PatternSearchTargetSyntax PatternSearchTarget(string fullName, string nameSpace, PatternReferenceSyntax patternReference)
        {
            var result = new PatternSearchTargetSyntax(fullName, nameSpace, patternReference);
            return result;
        }

        public static NamespaceSearchTargetSyntax NamespaceSearchTarget(string patternsNameSpace, string nameSpace)
        {
            var result = new NamespaceSearchTargetSyntax(patternsNameSpace, nameSpace);
            return result;
        }
    }
}
