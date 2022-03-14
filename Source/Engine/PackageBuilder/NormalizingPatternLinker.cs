using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class NormalizingPatternLinker : PatternLinker
    {
        private List<Syntax> fExtractedPatterns;
        private int fNextExtractedPatternNumber;
        private PatternSyntax fCurrentPattern;
        private List<Syntax> fAllPatterns;

        public NormalizingPatternLinker(Func<string, string> fileContentProvider, PackageCache packageCache)
            : base(fileContentProvider, packageCache, isFileSystemCaseSensitive: null)
        {
        }

        public NormalizingPatternLinker(Func<string, string> fileContentProvider, PackageCache packageCache, 
            bool? isFileSystemCaseSensitive)
            : base(fileContentProvider, packageCache, isFileSystemCaseSensitive)
        {
        }

        public override LinkedPackageSyntax Link(PackageSyntax syntaxTree, string baseDirectory, string filePath)
        {
            fExtractedPatterns = new List<Syntax>();
            fNextExtractedPatternNumber = 0;
            fAllPatterns = new List<Syntax>();
            return base.Link(syntaxTree, baseDirectory, filePath);
        }

        public override Syntax Visit(Syntax node)
        {
            Syntax result = base.Visit(node);
            if (result != null)
            {
                if (result.CanReduce)
                    result = result.Reduce();
                result.TextRange = node.TextRange;
            }
            return result;
        }

        protected internal override Syntax VisitPackage(PackageSyntax node)
        {
            var result = (LinkedPackageSyntax) base.VisitPackage(node);
            ReadOnlyCollection<Syntax> rootPatterns = result.Patterns;
            List<Syntax> searchTargetsFromPatterns = null;
            foreach (PatternSyntax p in fAllPatterns.Where(x => ((PatternSyntax)x).IsSearchTarget))
            {
                var t = new PatternSearchTargetSyntax(p.FullName, p.Namespace, Syntax.PatternReference(p));
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
            if (fExtractedPatterns.Count > 0)
                rootPatterns = new ReadOnlyCollection<Syntax>(result.Patterns.Union(fExtractedPatterns).ToArray());
            result = (LinkedPackageSyntax) result.Update(result.RequiredPackages, searchTargets, rootPatterns);
            fExtractedPatterns.Clear();
            fAllPatterns.Clear();
            return result;
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            fCurrentPattern = node;
            Syntax result = base.VisitPattern(node);
            fCurrentPattern = null;
            fAllPatterns.Add(result);
            return result;
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

        private string GetExtractedPatternName()
        {
            string result = $"{fCurrentPattern.Name}/{fNextExtractedPatternNumber}";
            fNextExtractedPatternNumber++;
            return result;
        }
    }
}
