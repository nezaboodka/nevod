//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private Dictionary<string, PatternSyntax> fPatternByName;
        private List<PatternReferenceInContext> fPatternReferences;
        private PatternSyntax fCurrentPattern;
        private readonly string fBaseDirectory;
        private readonly IPackageLoader fRequiredPackageLoader;

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
            fPatternReferences = new List<PatternReferenceInContext>();
            LinkedPackageSyntax result = (LinkedPackageSyntax)Visit(syntaxTree);
            return result;
        }

        protected internal override Syntax VisitPackage(PackageSyntax node)
        {
            foreach (PatternSyntax p in Syntax.StandardPattern.StandardPatterns)
                fPatternByName.Add(p.FullName, p);
            ReadOnlyCollection<RequiredPackageSyntax> requiredPackages = Visit(node.RequiredPackages);
            CheckDuplicatedPatternNames(node);
            ReadOnlyCollection<Syntax> rootPatterns = Visit(node.Patterns);
            ReadOnlyCollection<Syntax> searchTargets = Visit(node.SearchTargets);
            foreach (PatternReferenceInContext x in fPatternReferences)
                ResolvePatternReference(x);
            LinkedPackageSyntax result = Syntax.LinkedPackage(requiredPackages, searchTargets, rootPatterns);
            result.TextRange = node.TextRange;
            fPatternByName.Clear();
            fPatternReferences.Clear();
            return result;
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            if (node.Package == null)
            {
                if (fRequiredPackageLoader == null)
                    throw SyntaxError(TextResource.RequireOperatorIsNotAllowedInSinglePackageMode);
                string filePath = Syntax.GetRequiredFilePath(fBaseDirectory, node.RelativePath);
                LinkedPackageSyntax linkedPackage = fRequiredPackageLoader.LoadPackage(filePath);
                node.SetRequiredPackage(linkedPackage, fBaseDirectory);
            }
            foreach (PatternSyntax p in node.Package.Patterns)
                RegisterPatternWithNestedPatterns(p, node);
            return node;
        }

        protected internal override Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            if (fPatternByName.TryGetValue(node.SearchTarget, out PatternSyntax pattern))
                node.PatternReference.ReferencedPattern = pattern;
            else
                throw SyntaxError(TextResource.SearchTargetIsUndefinedPattern, node.SearchTarget);
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
            fPatternByName[result.FullName] = result;
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
            if (!fPatternByName.TryAdd(pattern.FullName, pattern))
                throw SyntaxError(TextResource.DuplicatedPatternInRequiredPackage, pattern.FullName, node.RelativePath);
            foreach (PatternSyntax p in pattern.NestedPatterns)
                RegisterPatternWithNestedPatterns(p, node);
        }

        private void CheckDuplicatedPatternNames(PackageSyntax node)
        {
            foreach (PatternSyntax p in node.Patterns.Where(x => x is PatternSyntax))
            {
                // Добавляем в словарь имён значение null, потому что объект PatternSyntax может быть заменён на новый.
                // Значение null заменяется на существующую ссылку на объект PatternSyntax в методе VisitPattern.
                if (!fPatternByName.TryAdd(p.FullName, null))
                    throw SyntaxError(TextResource.DuplicatedPatternName, p.FullName);
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
                    throw SyntaxError(TextResource.ReferenceToUndefinedPattern, fullName);
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
                                    throw SyntaxError(TextResource.ReferenceToUndefinedPattern, name);
                            }
                        }
                    }
                }
                else if (fPatternByName.TryGetValue(name, out referencedPattern)) // полное имя шаблона?
                    reference.PatternReference.ReferencedPattern = referencedPattern;
                else
                    throw SyntaxError(TextResource.ReferenceToUndefinedPattern, name);
            }
            if (reference.PatternReference.ExtractionFromFields.Count > 0)
            {
                PatternSyntax referencedPattern = reference.PatternReference.ReferencedPattern;
                foreach (ExtractionFromFieldSyntax extraction in reference.PatternReference.ExtractionFromFields)
                {
                    if (referencedPattern.FindFieldByName(extraction.FromFieldName) == null)
                    {
                        throw SyntaxError(TextResource.UndefinedFieldInReferencedPattern, extraction.FromFieldName,
                            referencedPattern.FullName);
                    }
                }
            }
        }
    }

    internal static partial class TextResource
    {
        public const string DuplicatedPatternInRequiredPackage = "Duplicated pattern '{0}' in required package '{1}'";
        public const string ReferenceToUndefinedPattern = "Reference to undefined pattern '{0}'";
        public const string UndefinedFieldInReferencedPattern = "Undefined field {0} in referenced pattern '{1}'";
        public const string SearchTargetIsUndefinedPattern = "Search target is undefined pattern '{0}'";
        public const string RequireOperatorIsNotAllowedInSinglePackageMode = "@require operator is not allowed in single package mode";
    }
}
