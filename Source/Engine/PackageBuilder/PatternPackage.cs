//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class PatternPackage
    {
        public LinkedPackageSyntax Syntax;
        public ReadOnlyCollection<string> SearchTargets { get; }

        internal ReadOnlyCollection<PatternExpression> Patterns { get; }
        internal SearchExpression SearchQuery { get; private set; }

        public static PatternPackage FromFile(string filePath)
        {
            var builder = new PackageBuilder();
            PatternPackage result = builder.BuildPackageFromFile(filePath);
            return result;
        }

        public static PatternPackage FromFile(string filePath, PackageBuilderOptions options)
        {
            var builder = new PackageBuilder(options);
            PatternPackage result = builder.BuildPackageFromFile(filePath);
            return result;
        }

        public static PatternPackage FromText(string definition)
        {
            var builder = new PackageBuilder();
            PatternPackage result = builder.BuildPackageFromText(definition);
            return result;
        }

        public static PatternPackage FromText(string definition, PackageBuilderOptions options)
        {
            var builder = new PackageBuilder(options);
            PatternPackage result = builder.BuildPackageFromText(definition);
            return result;
        }

        public static PatternPackage FromSyntax(PackageSyntax parsedTree)
        {
            var builder = new PackageBuilder();
            PatternPackage result = builder.BuildPackageFromSyntax(parsedTree);
            return result;
        }

        public static PatternPackage FromSyntax(PackageSyntax parsedTree, PackageBuilderOptions options)
        {
            var builder = new PackageBuilder(options);
            PatternPackage result = builder.BuildPackageFromSyntax(parsedTree);
            return result;
        }

        public static PatternPackage FromExpressionText(string expression)
        {
            var builder = new PackageBuilder();
            PatternPackage result = builder.BuildPackageFromExpressionText(expression);
            return result;
        }

        public static PatternPackage FromExpressionText(string expression, PackageBuilderOptions options)
        {
            var builder = new PackageBuilder(options);
            PatternPackage result = builder.BuildPackageFromExpressionText(expression);
            return result;
        }

        internal PatternPackage(LinkedPackageSyntax syntax, IList<PatternExpression> patterns, 
            SearchExpression searchQuery)
        {
            Syntax = syntax;
            Patterns = new ReadOnlyCollection<PatternExpression>(patterns);
            SearchQuery = searchQuery;
            SearchTargets = new ReadOnlyCollection<string>(searchQuery.TargetPatterns.Select(x => x.Name).ToArray());
        }

        internal void BuildIndex()
        {
            if (SearchQuery != null && !SearchQuery.IsNestedIndexCreated)
            {
                var ownIndexBuilder = new OwnIndexBuilder();
                ownIndexBuilder.Build(this);
                var referenceNestedIndexBuilder = new ReferencesNestedIndexBuilder();
                referenceNestedIndexBuilder.Build(this);
                var nestedIndexBuilder = new NestedIndexBuilder();
                nestedIndexBuilder.Build(this);
            }
        }
    }
}
