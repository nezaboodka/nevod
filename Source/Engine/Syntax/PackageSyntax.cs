//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    public class PackageSyntax : Syntax
    {
        public ReadOnlyCollection<RequiredPackageSyntax> RequiredPackages { get; }
        public ReadOnlyCollection<Syntax> SearchTargets { get; }
        public ReadOnlyCollection<Syntax> Patterns { get; }
        public List<Error> Errors { get; internal set; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                int rangeStart = TextRange.Start;
                if (RequiredPackages.Count != 0)
                {
                    int rangeEnd = RequiredPackages[0].TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.AddForElements(RequiredPackages);
                    rangeStart = RequiredPackages[RequiredPackages.Count - 1].TextRange.End;
                }
                ReadOnlyCollection<Syntax> mergedPatterns = MergePatternsAndSearchTargetsByTextRange();
                if (mergedPatterns.Count != 0)
                {
                    int rangeEnd = mergedPatterns[0].TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.AddForElements(mergedPatterns);
                    rangeStart = mergedPatterns[mergedPatterns.Count - 1].TextRange.End;
                }
                childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal PackageSyntax(IList<RequiredPackageSyntax> requiredPackages,
            IList<Syntax> searchTargets, IList<Syntax> patterns)
        {
            RequiredPackages = new ReadOnlyCollection<RequiredPackageSyntax>(requiredPackages);
            SearchTargets = new ReadOnlyCollection<Syntax>(searchTargets);
            Patterns = new ReadOnlyCollection<Syntax>(patterns);
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitPackage(this);
        }

        private ReadOnlyCollection<Syntax> MergePatternsAndSearchTargetsByTextRange()
        {
            var mergedList = new List<Syntax>(Patterns.Count + SearchTargets.Count);
            int firstIndex = 0;
            int secondIndex = 0;
            while (firstIndex < Patterns.Count && secondIndex < SearchTargets.Count)
            {
                if (Patterns[firstIndex].TextRange.Start < SearchTargets[secondIndex].TextRange.Start)
                    mergedList.Add(Patterns[firstIndex++]);
                else
                    mergedList.Add(SearchTargets[secondIndex++]);
            }
            if (firstIndex < Patterns.Count)
                for (int i = firstIndex; i < Patterns.Count; i++)
                    mergedList.Add(Patterns[i]);
            else if (secondIndex < SearchTargets.Count)
                for (int i = secondIndex; i < SearchTargets.Count; i++)
                    mergedList.Add(SearchTargets[i]);
            return mergedList.AsReadOnly();
        }
    }

    public class LinkedPackageSyntax : PackageSyntax
    {
        public bool HasOwnOrRequiredPackageErrors { get; internal set; }
        
        internal LinkedPackageSyntax(IList<RequiredPackageSyntax> requiredPackages,
            IList<Syntax> searchTargets, IList<Syntax> patterns)
            : base(requiredPackages, searchTargets, patterns)
        {
        }

        internal PackageSyntax Update(ReadOnlyCollection<RequiredPackageSyntax> requiredPackages,
            ReadOnlyCollection<Syntax> searchTargets, ReadOnlyCollection<Syntax> patterns)
        {
            LinkedPackageSyntax result = this;
            if (requiredPackages != RequiredPackages || searchTargets != SearchTargets || patterns != Patterns)
                result = LinkedPackage(requiredPackages, searchTargets, patterns);
            result.Errors = Errors;
            result.HasOwnOrRequiredPackageErrors = HasOwnOrRequiredPackageErrors;
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitLinkedPackage(this);
        }
    }

    public partial class Syntax
    {
        public static PackageSyntax Package(params Syntax[] patterns)
        {
            var result = new PackageSyntax(EmptyRequiredPackages(), EmptySearchTargets(), patterns);
            return result;
        }

        public static PackageSyntax Package(IList<Syntax> patterns)
        {
            var result = new PackageSyntax(EmptyRequiredPackages(), EmptySearchTargets(), patterns);
            return result;
        }

        public static PackageSyntax Package(IList<RequiredPackageSyntax> requiredPackages,
            IList<Syntax> patterns)
        {
            var result = new PackageSyntax(requiredPackages, EmptySearchTargets(), patterns);
            return result;
        }

        public static PackageSyntax Package(IList<RequiredPackageSyntax> requiredPackages,
            IList<Syntax> searchTargets, IList<Syntax> patterns)
        {
            var result = new PackageSyntax(requiredPackages, searchTargets, patterns);
            return result;
        }

        internal static LinkedPackageSyntax LinkedPackage(IList<RequiredPackageSyntax> requiredPackages,
            IList<Syntax> searchTargets, IList<Syntax> patterns)
        {
            var result = new LinkedPackageSyntax(requiredPackages, searchTargets, patterns);
            return result;
        }

        internal static IList<RequiredPackageSyntax> EmptyRequiredPackages()
        {
            return new RequiredPackageSyntax[0];
        }

        internal static IList<Syntax> EmptySearchTargets()
        {
            return new SearchTargetSyntax[0];
        }
    }
}
