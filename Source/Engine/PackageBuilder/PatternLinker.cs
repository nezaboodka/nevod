//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public interface IPackageLoader
    {
        LinkedPackageSyntax LoadPackage(string filePath);
    }

    public class PatternLinker : SyntaxVisitor
    {
        internal struct PatternReferenceInContext
        {
            public PatternReferenceSyntax PatternReference;
            public PatternSyntax PatternContext;
        }

        private static readonly HashSet<string> StandardPatternNames;
        private Dictionary<string, PatternSyntax> fPatternByName;
        private Dictionary<string, RequiredPackageSyntax> fRequiredPackageByFilePath;
        private Dictionary<string, RequiredPackageSyntax> fRequiredPackageByPatternName;
        private Dictionary<RequiredPackageSyntax, Dictionary<string, List<string>>> fDuplicatePatternsByRequiredPackage;
        private List<PatternReferenceInContext> fPatternReferences;
        private PatternSyntax fCurrentPattern;
        private readonly string fBaseDirectory;
        private readonly IPackageLoader fRequiredPackageLoader;
        private List<Error> fErrors;

        static PatternLinker()
        {
            StandardPatternNames = new HashSet<string>(Syntax.StandardPattern.StandardPatterns.Select(x => x.FullName));
        }

        public PatternLinker()
            : this(null, null)
        {
        }

        public PatternLinker(string baseDirectory, IPackageLoader requiredPackageLoader)
        {
            fBaseDirectory = baseDirectory;
            fRequiredPackageLoader = requiredPackageLoader;
        }

        public virtual LinkedPackageSyntax Link(PackageSyntax syntaxTree)
        {
            fPatternByName = new Dictionary<string, PatternSyntax>();
            fRequiredPackageByFilePath = new Dictionary<string, RequiredPackageSyntax>();
            fRequiredPackageByPatternName = new Dictionary<string, RequiredPackageSyntax>();
            fDuplicatePatternsByRequiredPackage =
                new Dictionary<RequiredPackageSyntax, Dictionary<string, List<string>>>();
            fPatternReferences = new List<PatternReferenceInContext>();
            fErrors = new List<Error>();
            LinkedPackageSyntax result = (LinkedPackageSyntax)Visit(syntaxTree);
            return result;
        }

        protected internal override Syntax VisitPackage(PackageSyntax node)
        {
            ReadOnlyCollection<RequiredPackageSyntax> requiredPackages = Visit(node.RequiredPackages);
            RegisterPatternsFromRequiredPackages(node.RequiredPackages);
            AddDuplicatePatternsInRequiredPackagesErrors();
            foreach (PatternSyntax p in Syntax.StandardPattern.StandardPatterns)
                fPatternByName.Add(p.FullName, p);
            // CheckDuplicatedPatternNames(node);
            ReadOnlyCollection<Syntax> rootPatterns = Visit(node.Patterns);
            ReadOnlyCollection<Syntax> searchTargets = Visit(node.SearchTargets);
            foreach (PatternReferenceInContext x in fPatternReferences)
                ResolvePatternReference(x);
            LinkedPackageSyntax result = Syntax.LinkedPackage(requiredPackages, searchTargets, rootPatterns);
            result.TextRange = node.TextRange;
            result.Errors = node.Errors.Concat(fErrors).ToList();
            fPatternByName.Clear();
            fRequiredPackageByFilePath.Clear();
            fRequiredPackageByPatternName.Clear();
            fDuplicatePatternsByRequiredPackage.Clear();
            fPatternReferences.Clear();
            fErrors.Clear();
            return result;
        }

        private void RegisterPatternsFromRequiredPackages(ReadOnlyCollection<RequiredPackageSyntax> requiredPackages)
        {
            foreach (RequiredPackageSyntax requiredPackage in requiredPackages)
            {
                if (requiredPackage.Package != null)
                    foreach (PatternSyntax p in requiredPackage.Package.Patterns)
                        RegisterPatternWithNestedPatterns(p, requiredPackage);
            }
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            if (fRequiredPackageLoader == null)
                throw SyntaxError(TextResource.RequireOperatorIsNotAllowedInSinglePackageMode);
            string filePath = Syntax.GetRequiredFilePath(fBaseDirectory, node.RelativePath);
            if (fRequiredPackageByFilePath.TryAdd(filePath, node))
            {
                LinkedPackageSyntax linkedPackage = null;
                try
                {
                    linkedPackage = TryLoadRequiredPackage(filePath, node);
                }
                catch (Exception)
                {
                    AddError(node, "Cannot import file '{0}'", filePath);
                }
                if (linkedPackage != null)
                {
                    node.SetRequiredPackage(linkedPackage, fBaseDirectory);
                    foreach (PatternSyntax p in node.Package.Patterns)
                        RegisterPatternWithNestedPatterns(p, node);
                }
            }
            else
                AddError(node, TextResource.DuplicatedRequiredPackage, node.RelativePath, 
                    fRequiredPackageByFilePath[filePath].RelativePath);
            return node;
        }

        private LinkedPackageSyntax TryLoadRequiredPackage(string filePath, RequiredPackageSyntax node)
        {
            LinkedPackageSyntax package = null;
            try
            {
                package = fRequiredPackageLoader.LoadPackage(filePath);
            }
            catch (FileNotFoundException)
            {
                AddError(node, "File '{0}' not found", filePath);
            }
            catch (DirectoryNotFoundException)
            {
                AddError(node, "File '{0}' not found", filePath);
            }
            catch (UnauthorizedAccessException)
            {
                AddError(node, "Access to file '{0}' is denied. If given path is a directory and you want to import all the files from it, import them separately", filePath);
            }
            catch (Exception)
            {
                AddError(node, "Cannot import file '{0}'", filePath);
            }
            return package;
        }

        protected internal override Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            if (fPatternByName.TryGetValue(node.SearchTarget, out PatternSyntax pattern))
                node.PatternReference.ReferencedPattern = pattern;
            else
                AddError(node, TextResource.SearchTargetIsUndefinedPattern, node.SearchTarget);
            return node;
        }

        protected internal override Syntax VisitNamespaceSearchTarget(NamespaceSearchTargetSyntax node)
        {
            var targetReferences = new List<Syntax>();
            foreach (PatternSyntax p in fPatternByName.Values.Where(x => x.IsSearchTarget && x.Namespace == node.Namespace))
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
                    AddError(result, "Duplicate pattern name. '{0}' is a standard pattern", result.FullName);
                else if (!fPatternByName.TryAdd(result.FullName, result))
                {
                    // If duplicate pattern has no associated RequiredPackageSyntax, original one is declared in current package.
                    if (fRequiredPackageByPatternName.TryGetValue(result.FullName,
                        out RequiredPackageSyntax requiredPackage))
                        AddError(result, "Duplicate pattern '{0}'. Pattern is already declared in '{1}'",
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
                    // as it has already been handled by linker in CheckDuplicatedPatternNames.
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
            // Ignore duplicate patterns, declared in same required package
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
                        fullName = reference.PatternContext.MasterPatternName + '.' + name;
                        if (!string.IsNullOrEmpty(contextNamespace))
                            fullName = contextNamespace + '.' + fullName;
                        if (fPatternByName.TryGetValue(fullName, out referencedPattern)) // имя шаблона внутри того же блока @where?
                            reference.PatternReference.ReferencedPattern = referencedPattern;
                        else
                        {
                            if (fPatternByName.TryGetValue(name, out referencedPattern)) // полное имя шаблона?
                                reference.PatternReference.ReferencedPattern = referencedPattern;
                            else
                            {
                                fullName = contextNamespace + '.' + name;
                                if (fPatternByName.TryGetValue(fullName, out referencedPattern)) // короткое имя шаблона (без пространства имён)?
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
        public const string DuplicatedPatternInRequiredPackage = "Required package '{0}' contains duplicate pattern '{1}'. Pattern is already declared in '{2}'";
        public const string DuplicatedPatternsInRequiredPackage = "Required package '{0}' contains {1} duplicate patterns: {2}. Patterns are already declared in '{3}'";
        public const string DuplicatedPatternsAndMoreInRequiredPackage = "Required package '{0}' contains {1} duplicate patterns: {2} and {3} more. Patterns are already declared in '{4}'";
        public const string ReferenceToUndefinedPattern = "Reference to undefined pattern '{0}'";
        public const string DuplicatedRequiredPackage = "Duplicated required package '{0}' already imported as '{1}'";
        public const string UndefinedFieldInReferencedPattern = "Undefined field {0} in referenced pattern '{1}'";
        public const string SearchTargetIsUndefinedPattern = "Search target is undefined pattern '{0}'";
        public const string RequireOperatorIsNotAllowedInSinglePackageMode = "@require operator is not allowed in single package mode";
    }
}
