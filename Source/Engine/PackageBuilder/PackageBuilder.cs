//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class PackageBuilder : IPackageLoader
    {
        internal class FileLinker
        {
            public string FilePath;
            public PatternLinker Linker;
        }

        private PackageBuilderOptions fOptions;
        private string fBaseDirectory;
        private Func<string, string> fFileContentProvider;
        private PackageCache fPackageCache;
        private Stack<FileLinker> fDependencyStack;

        public PackageBuilder()
            : this(PackageBuilderOptions.Default, Environment.CurrentDirectory, new PackageCache(), LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options)
            : this(options, Environment.CurrentDirectory, new PackageCache(), LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options, string baseDirectory)
            : this(options, baseDirectory, new PackageCache(), LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options, string baseDirectory, PackageCache packageCache)
            : this(options, baseDirectory, packageCache, LoadFileContent)
        {
        }

        public PackageBuilder(PackageBuilderOptions options, string baseDirectory, PackageCache packageCache,
            Func<string, string> fileContentProvider)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (packageCache == null)
                throw new ArgumentNullException(nameof(packageCache));
            if (fileContentProvider == null)
                throw new ArgumentNullException(nameof(fileContentProvider));

            fOptions = options;
            fBaseDirectory = baseDirectory;
            fFileContentProvider = fileContentProvider;
            fPackageCache = packageCache;
            fDependencyStack = new Stack<FileLinker>();
        }

        public PatternPackage BuildPackageFromText(string definition)
        {
            var parser = new SyntaxParser();
            PackageSyntax parsedTree = parser.ParsePackageText(definition);
            PatternPackage result = BuildPackageFromSyntax(parsedTree);
            return result;
        }

        public PatternPackage BuildPackageFromFile(string filePath)
        {
            string normalizedFilePath = Path.GetFullPath(filePath);
            PackageSyntax parsedTree = ParseFile(normalizedFilePath);
            PatternPackage result = BuildPackageFromSyntax(parsedTree, normalizedFilePath);
            return result;
        }

        public PatternPackage BuildPackageFromExpressionText(string expression)
        {
            var parser = new SyntaxParser();
            PackageSyntax parsedTree = parser.ParseExpressionText(expression);
            PatternPackage result = BuildPackageFromSyntax(parsedTree);
            return result;
        }

        public PatternPackage BuildPackageFromSyntax(PackageSyntax parsedTree) => BuildPackageFromSyntax(parsedTree, null);

        // Internal

        LinkedPackageSyntax IPackageLoader.LoadPackage(string filePath)
        {
            CheckRecursiveDependency(filePath);
            LinkedPackageSyntax result;
            if (!fPackageCache.PackageSyntaxByFilePath.TryGetValue(filePath, out result))
            {
                PackageSyntax package = ParseFile(filePath);
                string baseDirectory = Path.GetDirectoryName(filePath);
                try
                {
                    result = LinkPackage(package, baseDirectory, filePath);
                }
                catch (Exception ex)
                {
                    throw RequireError(filePath, ex);
                }
                result = SubstituteReferences(result);
                fPackageCache.PackageSyntaxByFilePath.TryAdd(filePath, result);
            }
            return result;
        }
        
        private PatternPackage BuildPackageFromSyntax(PackageSyntax parsedTree, string filePath)
        {
            lock (fPackageCache)
            {
                LinkedPackageSyntax linkedTree;
                try
                {
                    linkedTree = LinkPackage(parsedTree, fBaseDirectory, filePath);
                }
                catch (Exception ex)
                {
                    throw PackageBuildError(ex);
                }
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
            LinkedPackageSyntax result;
            var linker = new NormalizingPatternLinker(baseDirectory, requiredPackageLoader: this);
            fDependencyStack.Push(new FileLinker()
            {
                FilePath = filePath,
                Linker = linker
            });
            result = linker.Link(parsedTree);
            fDependencyStack.Pop();
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

        private void CheckRecursiveDependency(string filePath)
        {
            if (fDependencyStack.Any(x => x.FilePath == filePath))
            {
                fDependencyStack.TryPeek(out FileLinker lastLinker);
                throw lastLinker.Linker.SyntaxError(TextResource.RecursiveFileDependencyIsNotSupported,
                    string.Join(" -> ", fDependencyStack.Reverse().Select(x => x.FilePath)) + " -> " + filePath);
            }
        }

        private static string LoadFileContent(string filePath)
        {
            string result = File.ReadAllText(filePath, Encoding.UTF8);
            return result;
        }

        private Exception RequireError(string filePath, Exception innerException)
        {
            var result = new SyntaxException(string.Format(System.Globalization.CultureInfo.CurrentCulture,
                TextResource.RequiredPackageExceptionFormat, filePath, innerException.Message),
                innerException);
            return result;
        }
        
        private Exception PackageBuildError(Exception innerException)
        {
            var result = new SyntaxException(string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    TextResource.PackageBuildExceptionFormat, innerException.Message), innerException);
            return result;
        }
    }

    internal static partial class TextResource
    {
        public const string RequiredPackageExceptionFormat = "Error in required package '{0}': {1}";
        public const string PackageBuildExceptionFormat = "Error building package: {0}";
        public const string RecursiveFileDependencyIsNotSupported = "Recursive file dependency is not supported: {0}";
    }
}
