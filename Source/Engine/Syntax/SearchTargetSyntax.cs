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
    public class SearchTargetSyntax : Syntax
    {
        public new string SearchTarget { get; }

        public bool IsNamespaceWithWildcard() => SearchTarget.EndsWith(".*");

        internal SearchTargetSyntax(string searchTarget)
        {
            SearchTarget = searchTarget;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitSearchTarget(this);
        }
    }

    public class PatternSearchTargetSyntax : SearchTargetSyntax
    {
        public new Syntax PatternReference { get; }

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
        public ReadOnlyCollection<Syntax> PatternReferences { get; }

        internal NamespaceSearchTargetSyntax(string nameSpace, IList<Syntax> patternReferences)
            : base(nameSpace + ".*")
        {
            Namespace = nameSpace;
            PatternReferences = new ReadOnlyCollection<Syntax>(patternReferences);
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitNamespaceSearchTarget(this);
        }
    }

    public partial class Syntax
    {
        public static SearchTargetSyntax SearchTarget(string searchTarget)
        {
            var result = new SearchTargetSyntax(searchTarget);
            return result;
        }
    }
}
