//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class PackageBuilder
    {
        private PackageBuilderOptions fOptions;
        private Func<string, string> fFileContentProvider;
        private PackageCache fPackageCache;

        public PackageBuilder()
            : this(PackageBuilderOptions.Default, new PackageCache(), LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options)
            : this(options, new PackageCache(), LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options, PackageCache packageCache)
            : this(options, packageCache, LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options, PackageCache packageCache,
            Func<string, string> fileContentProvider)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (packageCache == null)
                throw new ArgumentNullException(nameof(packageCache));
            if (fileContentProvider == null)
                throw new ArgumentNullException(nameof(fileContentProvider));

            fOptions = options;
            fFileContentProvider = fileContentProvider;
            fPackageCache = packageCache;
        }

        public PatternPackage BuildPackageFromText(string definition) =>
            BuildPackageFromText(definition, Environment.CurrentDirectory);

        public PatternPackage BuildPackageFromText(string definition, string baseDirectory)
        {
            var parser = new SyntaxParser();
            PackageSyntax parsedTree = parser.ParsePackageText(definition);
            PatternPackage result = BuildPackageFromSyntax(parsedTree, baseDirectory);
            return result;
        }

        public PatternPackage BuildPackageFromFile(string filePath)
        {
            string normalizedFilePath = Path.GetFullPath(filePath);
            PackageSyntax parsedTree = ParseFile(normalizedFilePath);
            PatternPackage result = BuildPackageFromSyntax(parsedTree, Path.GetDirectoryName(filePath), normalizedFilePath);
            return result;
        }

        public PatternPackage BuildPackageFromExpressionText(string expression) =>
            BuildPackageFromExpressionText(expression, Environment.CurrentDirectory);

        public PatternPackage BuildPackageFromExpressionText(string expression, string baseDirectory)
        {
            var parser = new SyntaxParser();
            PackageSyntax parsedTree = parser.ParseExpressionText(expression);
            PatternPackage result = BuildPackageFromSyntax(parsedTree, baseDirectory);
            return result;
        }

        public PatternPackage BuildPackageFromSyntax(PackageSyntax parsedTree) => 
            BuildPackageFromSyntax(parsedTree, Environment.CurrentDirectory);

        public PatternPackage BuildPackageFromSyntax(PackageSyntax parsedTree, string baseDirectory) =>
            BuildPackageFromSyntax(parsedTree, baseDirectory, filePath: null);

        private PatternPackage BuildPackageFromSyntax(PackageSyntax parsedTree, string baseDirectory, string filePath)
        {
            lock (fPackageCache)
            {
                LinkedPackageSyntax linkedTree = LinkPackage(parsedTree, baseDirectory, filePath);
                if (linkedTree.HasOwnOrRequiredPackageErrors)
                    throw PackageBuildError(linkedTree, filePath);
                linkedTree = SubstituteReferences(linkedTree);
                var generator = new PackageGenerator(fOptions.SyntaxInformationBinding, fPackageCache);
                PatternPackage result = generator.Generate(linkedTree);
                return result;
            }
        }

        private PackageSyntax ParseFile(string filePath)
        {
            PackageSyntax result;
            string text = fFileContentProvider(filePath);
            var parser = new SyntaxParser();
            result = parser.ParsePackageText(text);
            return result;
        }

        private LinkedPackageSyntax LinkPackage(PackageSyntax parsedTree, string baseDirectory, string filePath)
        {
            var linker = new NormalizingPatternLinker(fFileContentProvider, fPackageCache);
            LinkedPackageSyntax result = linker.Link(parsedTree, baseDirectory, filePath);
            return result;
        }

        private LinkedPackageSyntax SubstituteReferences(LinkedPackageSyntax linkedTree)
        {
            if (fOptions.PatternReferencesInlined)
            {
                // Выполнить подстановку всех ссылок на шаблоны, кроме рекурсивных.
                var recursionDetector = new RecursivePatternDetector();
                HashSet<PatternSyntax> recursivePatterns = recursionDetector.GetRecursivePatterns(linkedTree);
                var substitutor = new PatternReferenceSubstitutor();
                linkedTree = substitutor.SubstitutePatternReferences(linkedTree, excludedPatterns: recursivePatterns);
            }
            else
            {
                // Выполнить подстановку только ссылок на системные шаблоны.
                var substitutor = new SystemPatternReferenceSubstitutor();
                linkedTree = substitutor.SubstituteSystemPatternReferences(linkedTree);
            }
            return linkedTree;
        }

        private static string LoadFileContent(string filePath)
        {
            string result = File.ReadAllText(filePath, Encoding.UTF8);
            return result;
        }

        private InvalidPackageException PackageBuildError(LinkedPackageSyntax linkedTree, string filePath)
        {
            return ErrorsCollector.AggregateErrorsException(linkedTree, filePath,
                TextResource.LinkedPackageContainsErrors);
        }
    }

    internal static partial class TextResource
    {
        public const string LinkedPackageContainsErrors = "Linked package contains errors. Pattern package cannot be built";
        public const string ReferencesCannotBeSubstituted = "Linked package contains errors. References cannot be substituted";
    }
}
