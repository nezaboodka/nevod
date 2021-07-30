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
    public abstract class SearchTargetSyntax : Syntax
    {
        public string SearchTarget { get; }

        internal SearchTargetSyntax(string searchTarget)
        {
            SearchTarget = searchTarget;
        }
    }

    public class PatternSearchTargetSyntax : SearchTargetSyntax
    {
        public new PatternReferenceSyntax PatternReference { get; }

        internal PatternSearchTargetSyntax(string patternName, PatternReferenceSyntax patternReference)
            : base(patternName)
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
        public string Namespace { get; }
        public ReadOnlyCollection<Syntax> PatternReferences { get; internal set; }

        internal NamespaceSearchTargetSyntax(string nameSpace)
            : base(nameSpace + ".*")
        {
            Namespace = nameSpace;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitNamespaceSearchTarget(this);
        }
    }

    public partial class Syntax
    {
        public static PatternSearchTargetSyntax PatternSearchTarget(string fullName, PatternReferenceSyntax patternReference)
        {
            var result = new PatternSearchTargetSyntax(fullName, patternReference);
            return result;
        }

        public static NamespaceSearchTargetSyntax NamespaceSearchTarget(string nameSpace)
        {
            var result = new NamespaceSearchTargetSyntax(nameSpace);
            return result;
        }
    }
}
