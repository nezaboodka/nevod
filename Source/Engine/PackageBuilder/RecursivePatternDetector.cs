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
    internal class RecursivePatternDetector : SyntaxVisitor
    {
        private HashSet<PatternSyntax> fRecursivePatterns;
        private List<PatternSyntax> fPatternStack;
        private HashSet<PatternSyntax> fVisitedPatterns;

        internal RecursivePatternDetector()
        {
        }

        public HashSet<PatternSyntax> GetRecursivePatterns(LinkedPackageSyntax package)
        {
            fRecursivePatterns = new HashSet<PatternSyntax>();
            fPatternStack = new List<PatternSyntax>();
            fVisitedPatterns = new HashSet<PatternSyntax>();
            Visit(package);
            fPatternStack.Clear();
            fVisitedPatterns.Clear();
            return fRecursivePatterns;
        }

        protected internal override Syntax VisitLinkedPackage(LinkedPackageSyntax node)
        {
            Visit(node.RequiredPackages);
            Visit(node.Patterns);
            return node;            
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            Syntax result = node;
            if (fVisitedPatterns.Add(node))
            {
                fPatternStack.Add(node);
                result = base.VisitPattern(node);
                fPatternStack.RemoveAt(fPatternStack.Count - 1);
            }
            return result;
        }

        protected internal override Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            Syntax result = node;
            if (!fVisitedPatterns.Contains(node.ReferencedPattern))
                result = Visit(node.ReferencedPattern);
            if (!fRecursivePatterns.Contains(node.ReferencedPattern))
            {
                int recursivePatternIndex = fPatternStack.IndexOf(node.ReferencedPattern);
                if (recursivePatternIndex >= 0)
                    for (int i = recursivePatternIndex, n = fPatternStack.Count; i < n; i++)
                        fRecursivePatterns.Add(node.ReferencedPattern);
            }
            return result;
        }

        protected internal override Syntax VisitEmbeddedPatternReference(EmbeddedPatternReferenceSyntax node)
        {
            Syntax result = VisitPatternReference(node);
            return result;
        }
    }
}
