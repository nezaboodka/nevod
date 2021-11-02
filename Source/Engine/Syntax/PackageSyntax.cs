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
    }

    public class LinkedPackageSyntax : PackageSyntax
    {
        public bool HasOwnOrChildErrors { get; internal set; }
        
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
            result.HasOwnOrChildErrors = HasOwnOrChildErrors;
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
