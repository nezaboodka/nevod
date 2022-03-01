//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

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
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                if (PatternReference != null)
                {
                    childrenBuilder.AddInsideRange(TextRange.Start, PatternReference.TextRange.Start);
                    childrenBuilder.Add(PatternReference);
                    childrenBuilder.AddInsideRange(PatternReference.TextRange.End, TextRange.End);
                }
                else
                    childrenBuilder.AddInsideRange(TextRange);
                Children = childrenBuilder.GetChildren();
            }
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
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                childrenBuilder.AddInsideRange(TextRange);
                Children = childrenBuilder.GetChildren();
            }
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
