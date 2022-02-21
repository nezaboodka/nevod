//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;

namespace Nezaboodka.Nevod
{
    public class PatternLinker : SyntaxVisitor
    {
        internal struct PatternReferenceInContext
        {
            public PatternReferenceSyntax PatternReference;
            public PatternSyntax PatternContext;
        }

        private readonly struct FileInfo
        {
            public string Path { get; }
            public string NormalizedPath { get; }

            public FileInfo(string path, string normalizedPath)
            {
                Path = path;
                NormalizedPath = normalizedPath;
            }
        }

        private static readonly HashSet<string> StandardPatternNames;
        private readonly Func<string, string> fFileContentProvider;
        private readonly PackageCache fPackageCache;
        private readonly Stack<FileInfo> fDependencyStack;
        private string fBaseDirectory;
        private Dictionary<string, PatternSyntax> fPatternByName;
        private Dictionary<string, RequiredPackageSyntax> fRequiredPackageByFilePath;
        private Dictionary<string, RequiredPackageSyntax> fRequiredPackageByPatternName;
        private Dictionary<RequiredPackageSyntax, Dictionary<string, List<string>>> fDuplicatePatternsByRequiredPackage;
        private List<PatternReferenceInContext> fPatternReferences;
        private PatternSyntax fCurrentPattern;
        private List<Error> fErrors;

        static PatternLinker()
        {
            StandardPatternNames = new HashSet<string>(Syntax.StandardPattern.StandardPatterns.Select(x => x.FullName));
        }

        public PatternLinker()
            : this(fileContentProvider: null, packageCache: null)
        {
        }

        public PatternLinker(Func<string, string> fileContentProvider)
            : this(fileContentProvider, packageCache: null)
        {
        }

        public PatternLinker(Func<string, string> fileContentProvider, PackageCache packageCache)
        {
            fFileContentProvider = fileContentProvider;
            fPackageCache = packageCache;
            fDependencyStack = new Stack<FileInfo>();
        }

        public virtual LinkedPackageSyntax Link(PackageSyntax syntaxTree, string baseDirectory, string filePath)
        {
            string saveBaseDirectory = fBaseDirectory;
            fBaseDirectory = baseDirectory;
            if (filePath != null)
                filePath = Path.GetFullPath(filePath);
            fDependencyStack.Push(new FileInfo(filePath, PathUtils.NormalizePathCase(filePath)));
            Dictionary<string, PatternSyntax> savePatternByName = fPatternByName;
            Dictionary<string, RequiredPackageSyntax> saveRequiredPackageByFilePath = fRequiredPackageByFilePath;
            Dictionary<string, RequiredPackageSyntax> saveRequiredPackageByPatternName = fRequiredPackageByPatternName;
            List<Error> saveErrors = fErrors;
            Dictionary<RequiredPackageSyntax, Dictionary<string, List<string>>> saveDuplicatePatternsByRequiredPackage = fDuplicatePatternsByRequiredPackage;
            LinkedPackageSyntax result;
            try
            {
                result = (LinkedPackageSyntax)Visit(syntaxTree);
            }
            finally
            {
                fBaseDirectory = saveBaseDirectory;
                fPatternByName = savePatternByName;
                fRequiredPackageByFilePath = saveRequiredPackageByFilePath;
                fRequiredPackageByPatternName = saveRequiredPackageByPatternName;
                fErrors = saveErrors;
                fDuplicatePatternsByRequiredPackage = saveDuplicatePatternsByRequiredPackage;
                fDependencyStack.Pop();
            }
            return result;
        }

        protected internal override Syntax VisitPackage(PackageSyntax node)
        {
            fRequiredPackageByFilePath = new Dictionary<string, RequiredPackageSyntax>();
            fErrors = new List<Error>();
            fPatternByName = new Dictionary<string, PatternSyntax>();
            fRequiredPackageByPatternName = new Dictionary<string, RequiredPackageSyntax>();
            fDuplicatePatternsByRequiredPackage = new Dictionary<RequiredPackageSyntax, Dictionary<string, List<string>>>();
            ReadOnlyCollection<RequiredPackageSyntax> requiredPackages = Visit(node.RequiredPackages);
            AddDuplicatePatternsInRequiredPackagesErrors();
            foreach (PatternSyntax p in Syntax.StandardPattern.StandardPatterns)
                fPatternByName.Add(p.FullName, p);
            fPatternReferences = new List<PatternReferenceInContext>();
            ReadOnlyCollection<Syntax> rootPatterns = Visit(node.Patterns);
            ReadOnlyCollection<Syntax> searchTargets = Visit(node.SearchTargets);
            foreach (PatternReferenceInContext x in fPatternReferences)
                ResolvePatternReference(x);
            LinkedPackageSyntax result = Syntax.LinkedPackage(requiredPackages, searchTargets, rootPatterns);
            result.TextRange = node.TextRange;
            result.Errors = node.Errors.Concat(fErrors).ToList();
            result.HasOwnOrRequiredPackageErrors = result.Errors.Count != 0 ||
                                         result.RequiredPackages.Any(requiredPackage => requiredPackage.Package.HasOwnOrRequiredPackageErrors);
            fPatternByName.Clear();
            fRequiredPackageByFilePath.Clear();
            fRequiredPackageByPatternName.Clear();
            fDuplicatePatternsByRequiredPackage.Clear();
            fPatternReferences.Clear();
            fErrors.Clear();
            return result;
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            string filePath = Syntax.GetRequiredFilePath(fBaseDirectory, node.RelativePath);
            string normalizedFilePath = PathUtils.NormalizePathCase(filePath);
            if (ValidateRequiredPathAndAddErrors(filePath, normalizedFilePath, node))
            {
                LinkedPackageSyntax linkedPackage = TryLoadRequiredPackage(filePath, node);
                if (linkedPackage != null)
                {
                    node.SetRequiredPackage(linkedPackage, fBaseDirectory);
                    foreach (PatternSyntax p in node.Package.Patterns)
                        RegisterPatternWithNestedPatterns(p, node);
                }
            }
            return node;
        }

        protected internal override Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            // PatternSearchTarget with null pattern name is handled by parser. No need to add error here.
            if (node.SearchTarget != null)
            {
                if (fPatternByName.TryGetValue(node.SearchTarget, out PatternSyntax pattern))
                    node.PatternReference.ReferencedPattern = pattern;
                else
                    AddError(node, TextResource.SearchTargetIsUndefinedPattern, node.SearchTarget);
            }
            return node;
        }

        protected internal override Syntax VisitNamespaceSearchTarget(NamespaceSearchTargetSyntax node)
        {
            var targetReferences = new List<Syntax>();
            foreach (PatternSyntax p in fPatternByName.Values.Where(x => x.IsSearchTarget && x.Namespace == node.PatternsNamespace))
            {
                PatternReferenceSyntax r = Syntax.PatternReference(p.FullName);
                var rc = new PatternReferenceInContext()
                {
                    PatternReference = r,
                    PatternContext = null
                };
                fPatternReferences.Add(rc);
                targetReferences.Add(r);
            }
            node.PatternReferences = new ReadOnlyCollection<Syntax>(targetReferences);
            return node;
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            PatternSyntax result = node;
            fCurrentPattern = node;
            result = (PatternSyntax)base.VisitPattern(node);
            fCurrentPattern = null;
            if (result.Name != null)
                if (StandardPatternNames.Contains(result.FullName))
                    AddError(result, TextResource.DuplicatedStandardPatternName, result.FullName);
                else if (!fPatternByName.TryAdd(result.FullName, result))
                {
                    // If duplicate pattern has no associated RequiredPackageSyntax, original one is declared in current package.
                    if (fRequiredPackageByPatternName.TryGetValue(result.FullName, out RequiredPackageSyntax requiredPackage))
                        AddError(result, TextResource.DuplicatedPatternIsAlreadyDeclaredIn,
                            result.FullName, requiredPackage.RelativePath);
                    else
                        AddError(result, TextResource.DuplicatedPatternName, result.FullName);
                }
            return result;
        }

        protected internal override Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            // Нельзя создавать копии PatternReferenceSyntax с помощью node.Update.
            var reference = new PatternReferenceInContext()
            {
                PatternReference = node,
                PatternContext = fCurrentPattern
            };
            fPatternReferences.Add(reference);
            return node;
        }

        protected internal override Syntax VisitEmbeddedPatternReference(EmbeddedPatternReferenceSyntax node)
        {
            VisitPatternReference(node);
            return node;
        }

        protected virtual LinkedPackageSyntax LoadRequiredPackage(string filePath)
        {
            if (fPackageCache == null ||
                !fPackageCache.PackageSyntaxByFilePath.TryGetValue(filePath, out LinkedPackageSyntax linkedPackage))
            {
                if (fFileContentProvider == null)
                    throw PackageLoadError(TextResource.CannotLoadRequiredFile, filePath);
                string text = fFileContentProvider(filePath);
                var parser = new SyntaxParser();
                PackageSyntax package = parser.ParsePackageText(text);
                linkedPackage = Link(package, Path.GetDirectoryName(filePath), filePath);
                fPackageCache?.PackageSyntaxByFilePath.Add(filePath, linkedPackage);
            }
            return linkedPackage;
        }

        protected Exception PackageLoadError(string format, params object[] args)
        {
            return new NevodPackageLoadException(string.Format(System.Globalization.CultureInfo.CurrentCulture,
                format, args));
        }

        private bool ValidateRequiredPathAndAddErrors(string filePath, string normalizedFilePath, RequiredPackageSyntax requiredPackage)
        {
            if (!fRequiredPackageByFilePath.TryAdd(normalizedFilePath, requiredPackage))
            {
                AddError(requiredPackage, TextResource.DuplicatedRequiredPackage,
                    requiredPackage.RelativePath, fRequiredPackageByFilePath[normalizedFilePath].RelativePath);
                return false;
            }
            if (fDependencyStack.Any(x => x.NormalizedPath != null && x.NormalizedPath == normalizedFilePath))
            {
                string errorMessage = string.Join(" -> ",
                    fDependencyStack.Reverse().SkipWhile(x => x.NormalizedPath != normalizedFilePath)
                        .Select(x => x.Path)) + " -> " + filePath;
                AddError(requiredPackage, TextResource.RecursiveFileDependencyIsNotSupported, errorMessage);
                return false;
            }
            return true;
        }

        private LinkedPackageSyntax TryLoadRequiredPackage(string filePath, RequiredPackageSyntax node)
        {
            LinkedPackageSyntax package = null;
            try
            {
                package = LoadRequiredPackage(filePath);
            }
            catch (FileNotFoundException)
            {
                AddError(node, TextResource.FileNotFound, filePath);
            }
            catch (DirectoryNotFoundException)
            {
                AddError(node, TextResource.FileNotFound, filePath);
            }
            catch (UnauthorizedAccessException)
            {
                AddError(node, TextResource.AccessToFileDenied, filePath);
            }
            catch (SecurityException)
            {
                AddError(node, TextResource.AccessToFileDenied, filePath);
            }
            catch (PathTooLongException)
            {
                AddError(node, TextResource.PathIsTooLong, filePath);
            }
            catch (NotSupportedException)
            {
                AddError(node, TextResource.PathIsNotValid, filePath);
            }
            catch (IOException)
            {
                AddError(node, TextResource.PathIsNotValid, filePath);
            }
            return package;
        }

        private void RegisterPatternWithNestedPatterns(PatternSyntax pattern, RequiredPackageSyntax node)
        {
            // Duplicate patterns named as one of standard patterns are handled in VisitPattern. No need to add error here.
            // Erroneous patterns with no name should be ignored by linker.
            if (!StandardPatternNames.Contains(pattern.FullName) && pattern.Name != null)
            {
                if (fPatternByName.TryAdd(pattern.FullName, pattern))
                    fRequiredPackageByPatternName[pattern.FullName] = node;
                else
                {
                    RequiredPackageSyntax originalPatternPackage = fRequiredPackageByPatternName[pattern.FullName];
                    // No need to add error message if duplicate pattern is in the same package as original,
                    // as it has already been handled by linker in VisitPattern.
                    if (node != originalPatternPackage)
                        AddDuplicatePatternInfo(node, pattern.FullName, originalPatternPackage.RelativePath);
                }
            }
            foreach (PatternSyntax p in pattern.NestedPatterns)
                RegisterPatternWithNestedPatterns(p, node);
        }

        private void AddDuplicatePatternInfo(RequiredPackageSyntax requiredPackage, string patternName,
            string originalFile)
        {
            Dictionary<string, List<string>> duplicatePatternsByOriginalFile =
                fDuplicatePatternsByRequiredPackage.GetOrCreate(requiredPackage);
            List<string> duplicatePatternsFromFile = duplicatePatternsByOriginalFile.GetOrCreate(originalFile);
            // Ignore duplicate patterns declared in same required package.
            if (!duplicatePatternsFromFile.Contains(patternName))
                duplicatePatternsFromFile.Add(patternName);
        }

        private void AddDuplicatePatternsInRequiredPackagesErrors()
        {
            foreach ((RequiredPackageSyntax requiredPackage, Dictionary<string, List<string>> duplicatePatternsByOriginalFile) in
                fDuplicatePatternsByRequiredPackage)
            foreach ((string originalFile, List<string> duplicatePatterns) in duplicatePatternsByOriginalFile)
            {
                if (duplicatePatterns.Count == 1)
                    AddError(requiredPackage, TextResource.DuplicatedPatternInRequiredPackage,
                        requiredPackage.RelativePath, duplicatePatterns[0], originalFile);
                else if (duplicatePatterns.Count <= 3)
                {
                    var joinedDuplicatePatterns = string.Join(", ", duplicatePatterns.Select(name => $"'{name}'"));
                    AddError(requiredPackage, TextResource.DuplicatedPatternsInRequiredPackage,
                        requiredPackage.RelativePath, duplicatePatterns.Count, joinedDuplicatePatterns, originalFile);
                }
                else
                {
                    var joinedDuplicatePatterns = string.Join(", ", duplicatePatterns.Take(3).Select(name => $"'{name}'"));
                    AddError(requiredPackage, TextResource.DuplicatedPatternsAndMoreInRequiredPackage,
                        requiredPackage.RelativePath, duplicatePatterns.Count, joinedDuplicatePatterns,
                        duplicatePatterns.Count - 3,  originalFile);
                }
            }
        }

        private void ResolvePatternReference(PatternReferenceInContext reference)
        {
            if (reference.PatternReference.ReferencedPattern != null)
            {
                string fullName = reference.PatternReference.ReferencedPattern.FullName;
                if (fPatternByName.TryGetValue(fullName, out PatternSyntax p))
                    reference.PatternReference.ReferencedPattern = p;
                else
                {
                    AddError(reference.PatternReference, TextResource.ReferenceToUndefinedPattern, fullName);
                    reference.PatternReference.ReferencedPattern = null;
                }
            }
            else
            {
                PatternSyntax referencedPattern;
                string name = reference.PatternReference.PatternName;
                if (reference.PatternContext != null)
                {
                    string fullName = reference.PatternContext.FullName + '.' + name;
                    if (fPatternByName.TryGetValue(fullName, out referencedPattern)) // имя вложенного шаблона?
                        reference.PatternReference.ReferencedPattern = referencedPattern;
                    else
                    {
                        string contextNamespace = reference.PatternContext.Namespace;
                        string contextMasterPatternName = reference.PatternContext.MasterPatternName;
                        if (!string.IsNullOrEmpty(contextMasterPatternName))
                            fullName = contextMasterPatternName + '.' + name;
                        if (!string.IsNullOrEmpty(contextNamespace))
                            fullName = contextNamespace + '.' + fullName;
                        if (fPatternByName.TryGetValue(fullName, out referencedPattern)) // имя шаблона внутри того же блока @where и пространтсва имён?
                            reference.PatternReference.ReferencedPattern = referencedPattern;
                        else
                        {
                            fullName = contextNamespace + '.' + name;
                            if (fPatternByName.TryGetValue(fullName, out referencedPattern)) // короткое имя шаблона внутри того же пространтсва имён?
                                reference.PatternReference.ReferencedPattern = referencedPattern;
                            else
                            {
                                if (fPatternByName.TryGetValue(name, out referencedPattern)) // полное имя шаблона?
                                    reference.PatternReference.ReferencedPattern = referencedPattern;
                                else
                                    AddError(reference.PatternReference, TextResource.ReferenceToUndefinedPattern, name);
                            }
                        }
                    }
                }
                else if (fPatternByName.TryGetValue(name, out referencedPattern)) // полное имя шаблона?
                    reference.PatternReference.ReferencedPattern = referencedPattern;
                else
                    AddError(reference.PatternReference, TextResource.ReferenceToUndefinedPattern, name);
            }
            if (reference.PatternReference.ExtractionFromFields.Count > 0)
            {
                PatternSyntax referencedPattern = reference.PatternReference.ReferencedPattern;
                // Validate extractions only if pattern reference is resolved.
                if (referencedPattern != null)
                    foreach (ExtractionFromFieldSyntax extraction in reference.PatternReference.ExtractionFromFields)
                    {
                        // Skip erroneous ExtractionFromFieldSyntax with no FromFieldName.
                        if (extraction.FromFieldName != null && referencedPattern.FindFieldByName(extraction.FromFieldName) == null)
                        {
                            AddError(extraction, TextResource.UndefinedFieldInReferencedPattern, extraction.FromFieldName,
                                referencedPattern.FullName);
                        }
                    }
            }
        }

        private void AddError(Syntax syntax, string format, params object[] args)
        {
            Error error = GetError(syntax, format, args);
            fErrors.Add(error);
        }

        private Error GetError(Syntax syntax, string format, params object[] args) =>
            new Error(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), syntax.TextRange);
    }

    internal static partial class TextResource
    {
        public const string DuplicatedPatternInRequiredPackage = "Required package '{0}' contains duplicated pattern name '{1}'. Pattern is already declared in '{2}'. Try using namespaces to avoid name conflicts";
        public const string DuplicatedPatternsInRequiredPackage = "Required package '{0}' contains {1} duplicated patterns: {2}. Patterns are already declared in '{3}'. Try using namespaces to avoid name conflicts";
        public const string DuplicatedPatternsAndMoreInRequiredPackage = "Required package '{0}' contains {1} duplicated patterns: {2} and {3} more. Patterns are already declared in '{4}'. Try using namespaces to avoid name conflicts";
        public const string ReferenceToUndefinedPattern = "Reference to undefined pattern '{0}'";
        public const string DuplicatedPatternName = "Duplicated pattern name '{0}'";
        public const string DuplicatedStandardPatternName = "Duplicated pattern name. '{0}' is a standard pattern";
        public const string DuplicatedPatternIsAlreadyDeclaredIn = "Duplicated pattern name '{0}'. Pattern is already declared in '{1}'. Try using namespaces to avoid name conflicts";
        public const string DuplicatedRequiredPackage = "Duplicated required package '{0}' already imported as '{1}'";
        public const string UndefinedFieldInReferencedPattern = "Undefined field {0} in referenced pattern '{1}'";
        public const string SearchTargetIsUndefinedPattern = "Search target is undefined pattern '{0}'";
        public const string FileNotFound = "File '{0}' not found";
        public const string AccessToFileDenied = "Access to file '{0}' denied. If given path is a directory and you want to import all the files from it, import them separately";
        public const string PathIsTooLong = "Path '{0}' is too long";
        public const string PathIsNotValid = "Path '{0}' is not valid";
        public const string RecursiveFileDependencyIsNotSupported = "Recursive file dependency is not supported: {0}";
        public const string CannotLoadRequiredFile = "Cannot load required file '{0}' because fileContentProvider is null. Pass it to the linker's constructor";
    }
}
