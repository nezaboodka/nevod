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
    internal class PatternLinker : SyntaxVisitor
    {
        internal struct PatternReferenceInContext
        {
            public PatternReferenceSyntax PatternReference;
            public PatternSyntax PatternContext;
        }

        private bool fLinkRequiredPackages;
        private List<Syntax> fAllPatterns;
        private Dictionary<string, PatternSyntax> fPatternByName;
        private List<PatternReferenceInContext> fPatternReferences;
        private PatternSyntax fCurrentPattern;
        private List<Syntax> fExtractedPatterns;
        private int fNextExtractedPatternNumber;

        internal PatternLinker(bool linkRequiredPackages)
        {
            fLinkRequiredPackages = linkRequiredPackages;
        }

        public LinkedPackageSyntax Link(PackageSyntax syntaxTree)
        {
            fAllPatterns = new List<Syntax>();
            fPatternByName = new Dictionary<string, PatternSyntax>();
            fPatternReferences = new List<PatternReferenceInContext>();
            fExtractedPatterns = new List<Syntax>();
            fNextExtractedPatternNumber = 0;
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
            List<Syntax> searchTargetsFromPatterns = null;
            foreach (PatternSyntax p in fAllPatterns.Where(x => ((PatternSyntax)x).IsSearchTarget))
            {
                var t = Syntax.SearchTarget(p.FullName);
                if (searchTargetsFromPatterns == null)
                    searchTargetsFromPatterns = new List<Syntax>();
                searchTargetsFromPatterns.Add(t);
            }
            ReadOnlyCollection<Syntax> searchTargets;
            if (searchTargetsFromPatterns != null)
            {
                var newSearchTargets = new List<Syntax>(node.SearchTargets);
                newSearchTargets.AddRange(searchTargetsFromPatterns);
                searchTargets = new ReadOnlyCollection<Syntax>(newSearchTargets);
            }
            else
                searchTargets = node.SearchTargets;
            searchTargets = Visit(searchTargets);
            foreach (PatternReferenceInContext x in fPatternReferences)
                ResolvePatternReference(x);
            if (fExtractedPatterns.Count > 0)
                rootPatterns = new ReadOnlyCollection<Syntax>(rootPatterns.Union(fExtractedPatterns).ToArray());
            LinkedPackageSyntax result = Syntax.LinkedPackage(requiredPackages, searchTargets, rootPatterns);
            fAllPatterns.Clear();
            fPatternByName.Clear();
            fPatternReferences.Clear();
            fExtractedPatterns.Clear();
            return result;
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            RequiredPackageSyntax result = node;
            if (fLinkRequiredPackages)
                result = (RequiredPackageSyntax)base.VisitRequiredPackage(node);
            foreach (PatternSyntax p in result.Package.Patterns)
                RegisterPatternWithNestedPatterns(p, node);
            return result;
        }

        protected internal override Syntax VisitSearchTarget(SearchTargetSyntax node)
        {
            SearchTargetSyntax result = node;
            if (node.IsNamespaceWithWildcard())
            {
                string ns = node.SearchTarget.TrimEnd('*', '.');
                var targetReferences = new List<Syntax>();
                foreach (PatternSyntax p in fPatternByName.Values.Where(x => x.IsSearchTarget && x.Namespace == ns))
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
                result = new NamespaceSearchTargetSyntax(ns, targetReferences);
            }
            else
            {
                if (fPatternByName.TryGetValue(node.SearchTarget, out PatternSyntax pattern))
                {
                    PatternReferenceSyntax targetReference = Syntax.PatternReference(pattern);
                    result = new PatternSearchTargetSyntax(pattern.FullName, targetReference);
                }
                else
                    throw SyntaxError(TextResource.SearchTargetIsUndefinedPattern, node.SearchTarget);
            }
            return result;
        }

        protected internal override Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            var rc = new PatternReferenceInContext()
            {
                PatternReference = (PatternReferenceSyntax)node.PatternReference,
                PatternContext = null
            };
            fPatternReferences.Add(rc);
            return node;
        }

        protected internal override Syntax VisitNamespaceSearchTarget(NamespaceSearchTargetSyntax node)
        {
            foreach (PatternReferenceSyntax r in node.PatternReferences)
            {
                var rc = new PatternReferenceInContext()
                {
                    PatternReference = r,
                    PatternContext = null
                };
                fPatternReferences.Add(rc);
            }
            return node;
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            PatternSyntax result = node;
            fCurrentPattern = node;
            result = (PatternSyntax)base.VisitPattern(node);
            fCurrentPattern = null;
            fPatternByName[result.FullName] = result;
            fAllPatterns.Add(result);
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

        protected internal override Syntax VisitInside(InsideSyntax node)
        {
            Syntax newInner = Visit(node.Inner);
            Syntax newOuter = Visit(node.Outer);
            if (!(newOuter is PatternReferenceSyntax))
            {
                var extractionFromFields = new List<Syntax>();
                foreach (FieldSyntax field in fCurrentPattern.Fields)
                    extractionFromFields.Add(Syntax.ExtractionFromField(field, field.Name));
                var pattern = Syntax.Pattern(fCurrentPattern.Namespace, isSearchTarget: false,
                    GetExtractedPatternName(), fCurrentPattern.Fields, newOuter);
                newOuter = Syntax.PatternReference(pattern, extractionFromFields);
                fExtractedPatterns.Add(pattern);
            }
            Syntax result = node.Update(newInner, newOuter);
            return result;
        }

        protected internal override Syntax VisitOutside(OutsideSyntax node)
        {
            Syntax newBody = Visit(node.Body);
            Syntax newException = Visit(node.Exception);
            if (!(newException is PatternReferenceSyntax))
            {
                var pattern = Syntax.Pattern(fCurrentPattern.Namespace, isSearchTarget: false,
                    GetExtractedPatternName(), fCurrentPattern.Fields, newException);
                newException = Syntax.PatternReference(pattern, Syntax.EmptyExtractionList());
                fExtractedPatterns.Add(pattern);
            }
            Syntax result = node.Update(newBody, newException);
            return result;
        }

        protected internal override Syntax VisitHaving(HavingSyntax node)
        {
            Syntax newOuter = Visit(node.Outer);
            Syntax newInner = Visit(node.Inner);
            if (!(newInner is PatternReferenceSyntax))
            {
                var extractionFromFields = new List<Syntax>();
                foreach (FieldSyntax field in fCurrentPattern.Fields)
                    extractionFromFields.Add(Syntax.ExtractionFromField(field, field.Name));
                var pattern = Syntax.Pattern(fCurrentPattern.Namespace, isSearchTarget: false,
                    GetExtractedPatternName(), fCurrentPattern.Fields, newInner);
                newInner = Syntax.PatternReference(pattern, extractionFromFields);
                fExtractedPatterns.Add(pattern);
            }
            Syntax result = node.Update(newOuter, newInner);
            return result;
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

        private string GetExtractedPatternName()
        {
            string result = string.Format("{0}/{1}", fCurrentPattern.Name, fNextExtractedPatternNumber);
            fNextExtractedPatternNumber++;
            return result;
        }
    }

    internal static partial class TextResource
    {
        public const string DuplicatedPatternInRequiredPackage = "Duplicated pattern '{0}' in required package '{1}'";
        public const string ReferenceToUndefinedPattern = "Reference to undefined pattern '{0}'";
        public const string UndefinedFieldInReferencedPattern = "Undefined field {0} in referenced pattern '{1}'";
        public const string SearchTargetIsUndefinedPattern = "Search target is undefined pattern '{0}'";
    }
}
