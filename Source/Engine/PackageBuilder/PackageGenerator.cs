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
    public class PackageGenerator : SyntaxVisitor
    {
        private bool fBindToSyntaxTree;
        private PackageCache fPackageCache;

        private HashSet<string> fRequiredPackages;
        private Stack<RequiredPackageSyntax> fRequiredPackageStack;
        private Stack<Expression> fExpressionStack;
        private Stack<PatternSyntax> fPatternStack;

        private List<PatternExpression> fRootPackagePatterns;
        private Dictionary<string, PatternExpression> fRootPackagePatternByName;
        private int fLastRootPackageRootExpressionId;

        private Dictionary<string, List<PatternReferenceExpression>> fPatternReferencesByName;
        private List<FieldExpression> fFields;
        private List<List<ExtractionFromFieldSyntax>> fFieldSubstitutions;
        private int fNextVariationWithExceptionExpressionId;

        private const int VariationWithoutExceptionsId = 0;

        public PackageGenerator(bool bindToSyntaxTree, PackageCache packageCache)
        {
            fBindToSyntaxTree = bindToSyntaxTree;
            fPackageCache = packageCache;
        }

        public PatternPackage Generate(LinkedPackageSyntax packageSyntax)
        {
            fRequiredPackages = new HashSet<string>();
            fRequiredPackageStack = new Stack<RequiredPackageSyntax>();
            fExpressionStack = new Stack<Expression>();
            fPatternStack = new Stack<PatternSyntax>();
            fRootPackagePatterns = new List<PatternExpression>();
            fRootPackagePatternByName = new Dictionary<string, PatternExpression>();
            fLastRootPackageRootExpressionId = -1;
            fPatternReferencesByName = new Dictionary<string, List<PatternReferenceExpression>>();
            fFields = new List<FieldExpression>();
            fFieldSubstitutions = new List<List<ExtractionFromFieldSyntax>>();
            fNextVariationWithExceptionExpressionId = VariationWithoutExceptionsId + 1;
            Visit(packageSyntax);
            if (fExpressionStack.Count > 0 || fPatternStack.Count > 0 || fRequiredPackageStack.Count > 0)
                throw Error(TextResource.InternalCompilerError);
            SearchExpression searchQuery;
            if (packageSyntax.SearchTargets != null)
            {
                Visit(packageSyntax.SearchTargets);
                LinkReferences();
                RefreshIsOptional();
                HashSet<PatternExpression> targetPatterns = PopTargetPatterns();
                var patternIndexLength = Math.Max(fPackageCache.GetLastRootExpressionId(), fLastRootPackageRootExpressionId) + 1;
                searchQuery = new SearchExpression(GetSyntaxOrNull(packageSyntax), targetPatterns, patternIndexLength);
            }
            else // (packageSyntax.SearchTargets == null)
                searchQuery = null;
            var result = new PatternPackage(GetSyntaxOrNull(packageSyntax), fRootPackagePatterns, searchQuery);
            result.BuildIndex();
            return result;
        }

        // Internal

        private bool IsRootPackageProcessing => fRequiredPackageStack.Count == 0;

        private T GetSyntaxOrNull<T>(T syntax)
        {
            return fBindToSyntaxTree ? syntax : default(T);
        }

        private int GetNextRootExpressionId()
        {
            int result;
            if (IsRootPackageProcessing)
                result = ++fLastRootPackageRootExpressionId; // fLastRootPackageRootExpressionId++ is wrong!
            else
                result = fPackageCache.GetNextRootExpressionId();
            return result;
        }

        private void LinkReferences()
        {
            foreach (KeyValuePair<string, List<PatternReferenceExpression>> x in fPatternReferencesByName)
            {
                string patternName = x.Key;
                PatternExpression pattern;
                if (fPackageCache.PatternByName.TryGetValue(patternName, out pattern) ||
                    fRootPackagePatternByName.TryGetValue(patternName, out pattern))
                {
                    List<PatternReferenceExpression> references = x.Value;
                    for (int i = 0; i < references.Count; i++)
                        references[i].ReferencedPattern = pattern;
                }
                else
                    throw Error(TextResource.ReferenceToUndefinedPattern, patternName);
            }
        }

        private void RefreshIsOptional()
        {
            bool done = (fPatternReferencesByName.Count == 0);
            if (!done)
            {
                var processedPatternNames = new List<string>(fPatternReferencesByName.Count);
                while (!done)
                {
                    foreach (KeyValuePair<string, List<PatternReferenceExpression>> x in fPatternReferencesByName)
                    {
                        string patternName = x.Key;
                        PatternExpression pattern;
                        if (!fPackageCache.PatternByName.TryGetValue(patternName, out pattern))
                            if (!fRootPackagePatternByName.TryGetValue(patternName, out pattern))
                                throw Error(TextResource.InternalCompilerError);
                        if (pattern.IsOptional)
                        {
                            List<PatternReferenceExpression> references = x.Value;
                            for (int i = 0; i < references.Count; i++)
                                references[i].RefreshIsOptional();
                            processedPatternNames.Add(patternName);
                        }
                    }
                    done = (processedPatternNames.Count == 0);
                    for (int i = 0, n = processedPatternNames.Count; i < n; i++)
                        fPatternReferencesByName.Remove(processedPatternNames[i]);
                    processedPatternNames.Clear();
                }
            }
        }

        private HashSet<PatternExpression> PopTargetPatterns()
        {
            var searchTargets = new HashSet<PatternExpression>();
            while (fExpressionStack.TryPop(out Expression expression))
            {
                if (expression is PatternReferenceExpression patternReference)
                    searchTargets.Add(patternReference.ReferencedPattern);
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            return searchTargets;
        }

        protected internal int GetNextVariationWithExceptionExpressionId()
        {
            int result = fNextVariationWithExceptionExpressionId;
            fNextVariationWithExceptionExpressionId++;
            return result;
        }

        protected internal override Syntax VisitLinkedPackage(LinkedPackageSyntax node)
        {
            Visit(node.RequiredPackages);
            if (IsRootPackageProcessing)
                fLastRootPackageRootExpressionId = fPackageCache.GetLastRootExpressionId();
            Visit(node.Patterns);
            return node;
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            fRequiredPackageStack.Push(node);
            if (fPackageCache.GeneratedPackages.Add(node.FullPath))
                Visit(node.Package);
            fRequiredPackageStack.Pop();
            return node;
        }

        protected internal override Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            Visit(node.PatternReference);
            return node;
        }

        protected internal override Syntax VisitNamespaceSearchTarget(NamespaceSearchTargetSyntax node)
        {
            ReadOnlyCollection<Syntax> patternReferences = node.PatternReferences;
            for (int i = 0; i < patternReferences.Count; i++)
            {
                var reference = (PatternReferenceSyntax)patternReferences[i];
                Visit(reference);
            }
            return node;
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            fPatternStack.Push(node);
            Visit(node.Fields);
            fFields = PopArrayOf<FieldExpression>(node.Fields.Count).ToList();
            Visit(node.Body);
            if (fExpressionStack.TryPop(out Expression expression))
            {
                FieldExpression[] fields = fFields.ToArray();
                for (int i = 0; i < fields.Length; i++)
                    fields[i].FieldNumber = i;
                var pattern = new PatternExpression(GetSyntaxOrNull(node), GetNextRootExpressionId(), node.FullName, 
                    fields, expression);
                bool isUniquePatternName = false;
                if (IsRootPackageProcessing)
                {
                    isUniquePatternName = !fPackageCache.PatternByName.ContainsKey(node.FullName);
                    if (isUniquePatternName)
                    {
                        isUniquePatternName = fRootPackagePatternByName.TryAdd(node.FullName, pattern);
                        if (isUniquePatternName)
                            fRootPackagePatterns.Add(pattern);
                    }
                }
                else
                    isUniquePatternName = fPackageCache.PatternByName.TryAdd(node.FullName, pattern);
                if (!isUniquePatternName)
                    throw Error(TextResource.DuplicatedPatternName, node.FullName);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            fPatternStack.Pop();
            Visit(node.NestedPatterns);
            return node;
        }

        protected internal override Syntax VisitField(FieldSyntax node)
        {
            var field = new FieldExpression(GetSyntaxOrNull(node), node.Name, node.IsInternal);
            fExpressionStack.Push(field);
            return node;
        }

        protected internal override Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            var reference = new PatternReferenceExpression(GetSyntaxOrNull(node));
            string patternFullName = node.ReferencedPattern.FullName;
            List<PatternReferenceExpression> patternReferences = fPatternReferencesByName.GetOrCreate(patternFullName);
            patternReferences.Add(reference);
            fExpressionStack.Push(reference);
            return node;
        }

        protected internal override Syntax VisitEmbeddedPatternReference(EmbeddedPatternReferenceSyntax node)
        {
            fPatternStack.Push(node.ReferencedPattern);
            if (node.ReferencedPattern.Fields.Count > 0)
            {
                List<ExtractionFromFieldSyntax> extractionFromFields =
                    node.ExtractionFromFields.Cast<ExtractionFromFieldSyntax>().ToList();
                for (int i = 0; i < node.ReferencedPattern.Fields.Count; i++)
                {
                    FieldSyntax field = node.ReferencedPattern.Fields[i];
                    if (!extractionFromFields.Exists(x => x.FromFieldName == field.Name))
                    {
                        string newFieldName = fFields.Count.ToString();
                        var fieldExpression = new FieldExpression(GetSyntaxOrNull(field), newFieldName, isInternal: true);
                        fFields.Add(fieldExpression);
                        var extractionFromField = Syntax.ExtractionFromField(newFieldName, field.Name);
                        extractionFromFields.Add(extractionFromField);
                    }
                }
                fFieldSubstitutions.Add(extractionFromFields);
            }
            Visit(node.ReferencedPattern.Body);
            if (node.ReferencedPattern.Fields.Count > 0)
                fFieldSubstitutions.RemoveAt(fFieldSubstitutions.Count - 1);
            fPatternStack.Pop();
            return node;
        }

        protected internal override Syntax VisitFieldReference(FieldReferenceSyntax node)
        {
            int fieldNumber = FindFieldNumberByName(node.FieldName);
            if (fieldNumber >= 0)
            {
                Expression bodyExpression = CreateAnyTokenVariationExpression();
                var extraction = new FieldReferenceExpression(GetSyntaxOrNull(node), fieldNumber, bodyExpression);
                fExpressionStack.Push(extraction);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitExtraction(ExtractionSyntax node)
        {
            int fieldNumber = FindFieldNumberByName(node.FieldName);
            if (fieldNumber >= 0)
            {
                Expression bodyExpression = null;
                if (node.Body != null)
                {
                    Visit(node.Body);
                    if (!fExpressionStack.TryPop(out bodyExpression))
                        throw Error(TextResource.InternalCompilerError);
                }
                var extraction = new ExtractionExpression(GetSyntaxOrNull(node), fieldNumber, bodyExpression);
                fExpressionStack.Push(extraction);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitSequence(SequenceSyntax node)
        {
            Visit(node.Elements);
            var elements = new Expression[node.Elements.Count];
            for (int i = node.Elements.Count - 1; i >= 0; i--)
            {
                if (fExpressionStack.TryPop(out Expression expression))
                    elements[i] = expression;
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            var sequence = new SequenceExpression(GetSyntaxOrNull(node), elements);
            fExpressionStack.Push(sequence);
            return node;
        }

        protected internal override Syntax VisitConjunction(ConjunctionSyntax node)
        {
            Visit(node.Elements);
            var elements = new Expression[node.Elements.Count];
            for (int i = node.Elements.Count - 1; i >= 0; i--)
            {
                if (fExpressionStack.TryPop(out Expression expression))
                    elements[i] = expression;
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            var conjunction = new ConjunctionExpression(GetSyntaxOrNull(node), elements);
            fExpressionStack.Push(conjunction);
            return node;
        }

        protected internal override Syntax VisitVariation(VariationSyntax node)
        {
            Visit(node.Elements);
            var elements = new Expression[node.Elements.Count];
            for (int i = node.Elements.Count - 1; i >= 0; i--)
            {
                if (fExpressionStack.TryPop(out Expression expression))
                    elements[i] = expression;
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            var variation = new VariationExpression(GetSyntaxOrNull(node), elements);
            if (variation.HasExceptions)
                variation.VariationWithExceptionsId = GetNextVariationWithExceptionExpressionId();
            else
                variation.VariationWithExceptionsId = VariationWithoutExceptionsId;
            fExpressionStack.Push(variation);
            return node;
        }

        protected internal override Syntax VisitSpan(SpanSyntax node)
        {
            if (node.Elements.Count == 1 && node.Elements[0] is RepetitionSyntax elementRepetition)
            {
                Visit(elementRepetition.Body);
                if (fExpressionStack.TryPop(out Expression expression))
                {
                    var repetition = new RepetitionExpression(GetSyntaxOrNull(node), elementRepetition.RepetitionRange, expression);
                    fExpressionStack.Push(repetition);
                }
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            else
                throw Error(TextResource.SpanWithMultipleRepetitionsIsNotSupported);
            return node;
        }

        protected internal override Syntax VisitException(ExceptionSyntax node)
        {
            Visit(node.Body);
            if (fExpressionStack.TryPop(out Expression expression))
            {
                var exception = new ExceptionExpression(GetSyntaxOrNull(node), GetNextRootExpressionId(), expression);
                fExpressionStack.Push(exception);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitRepetition(RepetitionSyntax node)
        {
            Visit(node.Body);
            if (fExpressionStack.TryPop(out Expression expression))
            {
                var repetition = new RepetitionExpression(GetSyntaxOrNull(node), node.RepetitionRange, expression);
                fExpressionStack.Push(repetition);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitOptionality(OptionalitySyntax node)
        {
            Visit(node.Body);
            if (fExpressionStack.TryPop(out Expression expression))
            {
                var repetition = new RepetitionExpression(GetSyntaxOrNull(node), Range.ZeroToOne(), expression);
                fExpressionStack.Push(repetition);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitWordSpan(WordSpanSyntax node)
        {
            Visit(node.Left);
            if (fExpressionStack.TryPop(out Expression leftExpression))
            {
                Visit(node.Right);
                if (fExpressionStack.TryPop(out Expression rightExpression))
                {
                    Expression extractionOfSpanExpression = null;
                    if (node.ExtractionOfSpan != null)
                    {
                        Visit(node.ExtractionOfSpan);
                        if (!fExpressionStack.TryPop(out extractionOfSpanExpression))
                            throw Error(TextResource.InternalCompilerError);
                    }
                    Expression exclusionExpression = null;
                    if (node.Exclusion != null)
                    {
                        Visit(node.Exclusion);
                        if (!fExpressionStack.TryPop(out exclusionExpression))
                            throw Error(TextResource.InternalCompilerError);
                    }
                    Expression wordSpanExpression;
                    if (node.SpanRange.IsZeroPlusOrOnePlus() && node.Exclusion == null)
                        wordSpanExpression = new AnySpanExpression(GetSyntaxOrNull(node), leftExpression, rightExpression,
                            node.SpanRange, extractionOfSpanExpression);
                    else
                    {
                        var spanExpression = new TokenExpression(GetSyntaxOrNull(Syntax.Word), TokenKind.Word, string.Empty,
                            isCaseSensitive: false, textIsPrefix: false, tokenAttributes: null);
                        wordSpanExpression = new WordSpanExpression(GetSyntaxOrNull(node), leftExpression, rightExpression,
                            node.SpanRange, spanExpression, exclusionExpression, extractionOfSpanExpression);
                    }
                    fExpressionStack.Push(wordSpanExpression);
                }
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitAnySpan(AnySpanSyntax node)
        {
            Visit(node.Left);
            if (fExpressionStack.TryPop(out Expression leftExpression))
            {
                Visit(node.Right);
                if (fExpressionStack.TryPop(out Expression rightExpression))
                {
                    Expression extractionOfSpanExpression = null;
                    if (node.ExtractionOfSpan != null)
                    {
                        Visit(node.ExtractionOfSpan);
                        if (!fExpressionStack.TryPop(out extractionOfSpanExpression))
                            throw Error(TextResource.InternalCompilerError);
                    }
                    var anySpanExpression = new AnySpanExpression(GetSyntaxOrNull(node), leftExpression, rightExpression,
                        spanRangeInWords: Range.ZeroPlus(), extractionOfSpanExpression);
                    fExpressionStack.Push(anySpanExpression);
                }
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitInside(InsideSyntax node)
        {
            Visit(node.Inner);
            if (fExpressionStack.TryPop(out Expression innerExpression))
            {
                Visit(node.Outer);
                if (fExpressionStack.TryPop(out Expression outerExpression))
                {
                    PatternReferenceExpression outerPatternReference;
                    if (outerExpression is PatternReferenceExpression reference)
                        outerPatternReference = reference;
                    else
                        throw Error(TextResource.InternalCompilerError);
                    var insideExpression = new InsideExpression(GetSyntaxOrNull(node), innerExpression, outerPatternReference);
                    fExpressionStack.Push(insideExpression);
                }
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitOutside(OutsideSyntax node)
        {
            Visit(node.Body);
            if (fExpressionStack.TryPop(out Expression bodyExpression))
            {
                Visit(node.Exception);
                if (fExpressionStack.TryPop(out Expression exceptionExpression))
                {
                    PatternReferenceExpression exceptionPatternReference;
                    if (exceptionExpression is PatternReferenceExpression reference)
                        exceptionPatternReference = reference;
                    else
                        throw Error(TextResource.InternalCompilerError);
                    var outsideExpression = new OutsideExpression(GetSyntaxOrNull(node), bodyExpression, exceptionPatternReference);
                    fExpressionStack.Push(outsideExpression);
                }
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitHaving(HavingSyntax node)
        {
            Visit(node.Outer);
            if (fExpressionStack.TryPop(out Expression outerExpression))
            {
                Visit(node.Inner);
                if (fExpressionStack.TryPop(out Expression innerExpression))
                {
                    PatternReferenceExpression innerPatternReference;
                    if (innerExpression is PatternReferenceExpression reference)
                        innerPatternReference = reference;
                    else
                        throw Error(TextResource.InternalCompilerError);
                    var havingExpression = new HavingExpression(GetSyntaxOrNull(node), outerExpression, innerPatternReference);
                    fExpressionStack.Push(havingExpression);
                }
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            else
                throw Error(TextResource.InternalCompilerError);
            return node;
        }

        protected internal override Syntax VisitText(TextSyntax node)
        {
            throw Error(TextResource.InternalCompilerError);
        }

        protected internal override Syntax VisitToken(TokenSyntax node)
        {
            var token = new TokenExpression(GetSyntaxOrNull(node), node.TokenKind, node.Text, node.IsCaseSensitive,
                node.TextIsPrefix, node.TokenAttributes);
            fExpressionStack.Push(token);
            return node;
        }

        protected internal override Syntax VisitDefault(DefaultSyntax node)
        {
            throw Error(TextResource.InternalCompilerError);
        }

        protected Exception Error(string format, params object[] args)
        {
            string currentPatternName = string.Empty;
            if (fPatternStack.TryPeek(out PatternSyntax currentPattern))
                currentPatternName = currentPattern.FullName;
            return new SyntaxException(string.Format(TextResource.CompilationError,
                string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args),
                currentPatternName), 0, 0, string.Empty);
        }

        private T[] PopArrayOf<T>(int count) where T : Expression
        {
            var elements = new T[count];
            for (int i = count - 1; i >= 0; i--)
            {
                if (fExpressionStack.TryPop(out Expression element))
                    elements[i] = (T)element;
                else
                    throw Error(TextResource.InternalCompilerError);
            }
            return elements;
        }

        private int FindFieldNumberByName(string fieldName)
        {
            string substitutedFieldName = fieldName;
            for (int i = fFieldSubstitutions.Count - 1; i >= 0; i--)
            {
                bool substituted = false;
                List<ExtractionFromFieldSyntax> substitution = fFieldSubstitutions[i];
                for (int j = 0; j < substitution.Count; j++)
                {
                    ExtractionFromFieldSyntax extractionFromField = substitution[j];
                    if (extractionFromField.FromFieldName == substitutedFieldName)
                    {
                        substitutedFieldName = extractionFromField.FieldName;
                        substituted = true;
                        break;
                    }
                }
                if (!substituted)
                    break;
            }
            int result = fFields.FindIndex(x => x.Name == substitutedFieldName);
            return result;
        }

        private static Expression CreateAnyTokenVariationExpression()
        {
            return new VariationExpression(syntax: null, new[]{
                new TokenExpression(TokenKind.Word),
                new TokenExpression(TokenKind.Symbol),
                new TokenExpression(TokenKind.Space),
                new TokenExpression(TokenKind.Punctuation),
                new TokenExpression(TokenKind.LineBreak),
            });
        }
    }

    internal static partial class TextResource
    {
        public const string CompilationError = "Compilation error: {0}, pattern '{1}'";
        public const string InternalCompilerError = "Internal pattern compiler error";
        public const string SpanWithMultipleRepetitionsIsNotSupported = "Span operator with multiple repetitions is not supported in this version of the search engine";
    }
}
