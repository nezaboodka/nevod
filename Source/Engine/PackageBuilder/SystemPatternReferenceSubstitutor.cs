//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    internal class SystemPatternReferenceSubstitutor : SyntaxVisitor
    {
        private HashSet<PatternSyntax> fVisitedPatterns;
        private Dictionary<PatternSyntax, PatternSyntax> fSystemPatternSubstitutions;

        public LinkedPackageSyntax SubstituteSystemPatternReferences(LinkedPackageSyntax syntaxTree)
        {
            fVisitedPatterns = new HashSet<PatternSyntax>();
            fSystemPatternSubstitutions = new Dictionary<PatternSyntax, PatternSyntax>();
            LinkedPackageSyntax result = (LinkedPackageSyntax)Visit(syntaxTree);
            fSystemPatternSubstitutions.Clear();
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
                fSystemPatternSubstitutions[node] = result;
            }
            else
                result = fSystemPatternSubstitutions[node];
            return result;
        }

        protected internal override Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            Syntax result;
            if (node.ReferencedPattern.IsSystem)
            {
                PatternSyntax updatedSystemPattern = (PatternSyntax)Visit(node.ReferencedPattern);
                result = updatedSystemPattern.Body;
            }
            else
                result = node;
            return result;
        }
    }
}
