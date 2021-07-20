//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Nezaboodka.Text;

namespace Nezaboodka.Nevod
{
    public interface IPackageLoader
    {
        LinkedPackageSyntax LoadPackage(string filePath);
    }

    // Operators from highest priority to lowest priority:
    // ?                - Optionality (repetition 0-1)
    // []               - Span
    // {}               - Variation
    // :                - Extraction
    // +                - Sequence
    // _                - Word sequence
    // ...  ..[m-n]..   - Word span, any span
    // &                - Conjunction
    // @inside, @outside, @having
    // ~                - Exception
    // m-n              - Repetition

    public class SyntaxParser
    {
        private string fBaseDirectory;
        private IPackageLoader fRequiredPackageLoader;
        private Slice fText;
        private int fTextPosition;
        private int fLineNumber;
        private int fLinePosition;
        private int fLineLength;
        private char fCharacter;
        private Token fToken;
        private Dictionary<string, TokenId> fTokenByKeyword;
        private NameScope fCurrentScope;
        private Stack<NameScope> fScopeStack;
        private List<RequiredPackageSyntax> fRequiredPackages;
        private List<Syntax> fPatterns;
        private List<Syntax> fSearchTargets;
        private Dictionary<string, PatternSyntax> fPatternByName;
        private Dictionary<string, PatternSyntax> fStandardPatterns;
        private Dictionary<string, FieldSyntax> fFieldByName;
        private HashSet<FieldSyntax> fExtractedFields;
        private HashSet<FieldSyntax> fAccessibleFields;
        private Stack<HashSet<FieldSyntax>> fAccessibleFieldsStack;

        public static readonly string EmptyNamespace = string.Empty;

        public SyntaxParser()
            : this(null, null)
        {
        }

        public SyntaxParser(string baseDirectory, IPackageLoader requiredPackageLoader)
        {
            fBaseDirectory = baseDirectory;
            fRequiredPackageLoader = requiredPackageLoader;
            fTokenByKeyword = new Dictionary<string, TokenId>();
            PrepareEnglishKeywordsDictionary();
            fStandardPatterns = new Dictionary<string, PatternSyntax>();
            fStandardPatterns.AddRange(Syntax.StandardPattern.StandardPatterns.Select(
                x => new KeyValuePair<string, PatternSyntax>(x.FullName, x)));
            fFieldByName = new Dictionary<string, FieldSyntax>();
            fExtractedFields = new HashSet<FieldSyntax>();
            fAccessibleFields = new HashSet<FieldSyntax>();
            fAccessibleFieldsStack = new Stack<HashSet<FieldSyntax>>();
        }

        public PackageSyntax ParsePackageText(string text)
        {
            fText = text.Slice();
            fTextPosition = -1;
            fLineNumber = 1;
            fLinePosition = 0;
            fLineLength = 0;
            NextCharacter();
            NextTokenOrComment();
            PackageSyntax result = ParsePackage();
            return result;
        }

        public PackageSyntax ParseExpressionText(string text)
        {
            fText = text.Slice();
            fTextPosition = -1;
            NextCharacter();
            NextToken();
            int startPosition = fToken.TextSlice.Position;
            Syntax patternBody = ParseInsideOrOutsideOrHaving();
            PatternSyntax pattern = SetTextRange(Syntax.Pattern(isSearchTarget: true, "Pattern", patternBody, null), startPosition);
            return SetTextRange(Syntax.Package(pattern), startPosition);
        }

        public Exception SyntaxError(string format)
        {
            return SyntaxError(fToken.TextSlice.Position, format, fToken);
        }

        public Exception SyntaxError(string format, params object[] args)
        {
            return SyntaxError(fToken.TextSlice.Position, format, args);
        }

        public Exception SyntaxError(int pos, string format, params object[] args)
        {
            var result = new SyntaxException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args),
                pos, fLineNumber, fText.SubSlice(fLinePosition, fLineLength).ToString());
            return result;
        }

        // Internal

        private void PrepareEnglishKeywordsDictionary()
        {
            fTokenByKeyword.Clear();
            fTokenByKeyword.Add("@require", TokenId.RequireKeyword);
            fTokenByKeyword.Add("@namespace", TokenId.NamespaceKeyword);
            fTokenByKeyword.Add("@pattern", TokenId.PatternKeyword);
            fTokenByKeyword.Add("@search", TokenId.SearchKeyword);
            fTokenByKeyword.Add("@where", TokenId.WhereKeyword);
            fTokenByKeyword.Add("@inside", TokenId.InsideKeyword);
            fTokenByKeyword.Add("@outside", TokenId.OutsideKeyword);
            fTokenByKeyword.Add("@having", TokenId.HavingKeyword);
        }

        private void PrepareRussianKeywordsDictionary()
        {
            fTokenByKeyword.Clear();
            fTokenByKeyword.Add("@требуется", TokenId.RequireKeyword);
            fTokenByKeyword.Add("@пространство", TokenId.NamespaceKeyword);
            fTokenByKeyword.Add("@шаблон", TokenId.PatternKeyword);
            fTokenByKeyword.Add("@искать", TokenId.SearchKeyword);
            fTokenByKeyword.Add("@где", TokenId.WhereKeyword);
            fTokenByKeyword.Add("@внутри", TokenId.InsideKeyword);
            fTokenByKeyword.Add("@вне", TokenId.OutsideKeyword);
            fTokenByKeyword.Add("@имеющий", TokenId.HavingKeyword);
        }

        private PackageSyntax ParsePackage()
        {
            fCurrentScope = new NameScope();
            fScopeStack = new Stack<NameScope>();
            fRequiredPackages = new List<RequiredPackageSyntax>();
            fPatterns = new List<Syntax>();
            fSearchTargets = new List<Syntax>();
            fPatternByName = new Dictionary<string, PatternSyntax>();
            int startPosition = fToken.TextSlice.Position;
            foreach (PatternSyntax p in Syntax.StandardPattern.StandardPatterns)
                fPatternByName.Add(p.FullName, p);
            // 1. Metadata
            ParseMetadata();
            // 2. Required packages
            ParseRequires();
            var searchTargets = new List<Syntax>();
            var patterns = new List<Syntax>();
            // 3. Pattern definitions within namespaces
            while (fToken.Id != TokenId.End)
                ParseNamespacesAndPatterns();
            PackageSyntax result = SetTextRange(Syntax.Package(fRequiredPackages, fSearchTargets, fPatterns), startPosition);
            fCurrentScope = null;
            fScopeStack = null;
            fRequiredPackages = null;
            fPatterns = null;
            fSearchTargets = null;
            fPatternByName = null;
            return result;
        }

        private void ParseMetadata()
        {
            while (fToken.Id == TokenId.Comment)
            {
                char commentType = fToken.TextSlice[1];
                int cutNum = 0;
                if (commentType == '*')
                    cutNum = 2;
                else
                {
                    int lastPos = fToken.TextSlice.Length - 1;
                    while (fToken.TextSlice[lastPos] == '\u000A' || fToken.TextSlice[lastPos] == '\u000D')
                    {
                        lastPos--;
                        cutNum++;
                    }
                }
                string comment = fToken.TextSlice.SubSlice(2, fToken.TextSlice.Length - 3 - cutNum).ToString();
                string metadata = comment.Trim();
                ParseMetadataValue(metadata);
                NextTokenOrComment();
            }
        }

        private void ParseMetadataValue(string metadata)
        {
            switch (metadata)
            {
                case "en":
                    PrepareEnglishKeywordsDictionary();
                    break;
                case "ru":
                    PrepareRussianKeywordsDictionary();
                    break;
            }
        }

        private void ParseRequires()
        {
            while (fToken.Id == TokenId.RequireKeyword)
            {
                RequiredPackageSyntax requiredPackage = ParseRequire();
                if (!fRequiredPackages.Any(x => x.Package == requiredPackage.Package))
                    fRequiredPackages.Add(requiredPackage);
                else
                    throw SyntaxError(TextResource.DuplicatedRequiredPackage, requiredPackage.RelativePath);
            }
        }

        private RequiredPackageSyntax ParseRequire()
        {
            int startPosition = fToken.TextSlice.Position;
            ValidateToken(TokenId.RequireKeyword, TextResource.RequireKeywordExpected);
            NextToken();
            ValidateToken(TokenId.StringLiteral, TextResource.FilePathAsStringLiteralExpected);
            string relativePath = ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix);
            if (isCaseSensitive || textIsPrefix)
                throw SyntaxError(TextResource.InvalidSpecifierAfterStringLiteral);
            RequiredPackageSyntax result;
            if (fRequiredPackageLoader != null)
            {
                string filePath = Syntax.GetRequiredFilePath(fBaseDirectory, relativePath);
                LinkedPackageSyntax linkedPackage = fRequiredPackageLoader.LoadPackage(filePath);
                result = new RequiredPackageSyntax(fBaseDirectory, relativePath, linkedPackage);
            }
            else
                throw SyntaxError(TextResource.RequireOperatorIsNotAllowedInSinglePackageMode);
            ValidateToken(TokenId.Semicolon, TextResource.RequireDefinitionShouldEndWithSemicolon);
            NextToken();
            return SetTextRange(result, startPosition);
        }

        private void ParseNamespacesAndPatterns()
        {
            switch (fToken.Id)
            {
                case TokenId.NamespaceKeyword:
                    NextToken();
                    ParseNamespace();
                    break;
                case TokenId.PatternKeyword:
                    int startPosition1 = fToken.TextSlice.Position;
                    NextToken();
                    PatternSyntax pattern1 = SetTextRange(ParsePattern(isSearchTarget: false), startPosition1);
                    fPatterns.Add(pattern1);
                    break;
                case TokenId.SearchKeyword:
                    int startPosition2 = fToken.TextSlice.Position;
                    NextToken();
                    if (fToken.Id == TokenId.PatternKeyword)
                    {
                        NextToken();
                        PatternSyntax pattern2 = SetTextRange(ParsePattern(isSearchTarget: true), startPosition2);
                        fPatterns.Add(pattern2);
                    }
                    else
                    {
                        SearchTargetSyntax searchTarget = ParseSearchTarget();
                        fSearchTargets.Add(searchTarget);
                    }
                    break;
                default:
                    PatternSyntax pattern3 = ParsePattern(isSearchTarget: false);
                    fPatterns.Add(pattern3);
                    break;
            }
        }

        private SearchTargetSyntax ParseSearchTarget()
        {
            int startPosition = fToken.TextSlice.Position;
            string name = ParseMultipartIdentifier(shouldStartFromIdentifier: true, canEndWithWildcard: true);
            string fullName = Syntax.GetFullName(fCurrentScope.Namespace, name);
            ValidateToken(TokenId.Semicolon, TextResource.SearchTargetDefinitionShouldEndWithSemicolon);
            NextToken();
            SearchTargetSyntax result;
            if (fullName.EndsWith(".*"))
            {
                string ns = fullName.TrimEnd('*', '.');
                var targetReferences = new List<Syntax>();
                result = new NamespaceSearchTargetSyntax(ns, targetReferences);
            }
            else
            {
                PatternReferenceSyntax patternReference = SetTextRange(Syntax.PatternReference(fullName), startPosition);
                result = new PatternSearchTargetSyntax(fullName, patternReference);
            }
            return SetTextRange(result, startPosition);
        }

        private void ParseNamespace()
        {
            string name = ParseMultipartIdentifier(shouldStartFromIdentifier: true, canEndWithWildcard: false);
            ValidateToken(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected);
            NextToken();
            fScopeStack.Push(fCurrentScope);
            string nameSpace = Syntax.GetFullName(fCurrentScope.Namespace, name);
            fCurrentScope = new NameScope(nameSpace, fCurrentScope.MasterPatternName);
            try
            {
                while (fToken.Id != TokenId.CloseCurlyBrace)
                    ParseNamespacesAndPatterns();
                NextToken();
            }
            finally
            {
                fCurrentScope = fScopeStack.Pop();
            }
        }

        private string ParseMultipartIdentifier(bool shouldStartFromIdentifier, bool canEndWithWildcard)
        {
            var result = new StringBuilder();
            if (fToken.Id == TokenId.Identifier)
            {
                result.Append(fToken.TextSlice.ToString());
                NextToken();
            }
            else if (shouldStartFromIdentifier)
                throw SyntaxError(TextResource.IdentifierExpected);
            while (fToken.Id == TokenId.Period)
            {
                result.Append('.');
                NextToken();
                if (fToken.Id == TokenId.Identifier)
                {
                    result.Append(fToken.TextSlice.ToString());
                    NextToken();
                }
                else if (fToken.Id == TokenId.Asterisk && canEndWithWildcard)
                {
                    result.Append(fToken.TextSlice.ToString());
                    NextToken();
                    break;
                }
                else if (canEndWithWildcard)
                    throw SyntaxError(TextResource.IdentifierOrAsteriskExpected);
                else
                    throw SyntaxError(TextResource.IdentifierExpected);
            }
            return result.ToString();
        }

        private PatternSyntax ParsePattern(bool isSearchTarget)
        {
            int startPosition = fToken.TextSlice.Position;
            PatternSyntax pattern;
            if (fToken.Id == TokenId.HashSign)
            {
                isSearchTarget = true;
                NextToken();
            }
            if (fToken.Id == TokenId.Identifier)
            {
                string name = ParseMultipartIdentifier(shouldStartFromIdentifier: true, canEndWithWildcard: false);
                string fullName = Syntax.GetFullName(fCurrentScope.GetFullName(), name);
                if (!fPatternByName.ContainsKey(fullName))
                {
                    fFieldByName.Clear();
                    fExtractedFields.Clear();
                    fAccessibleFields.Clear();
                    fAccessibleFieldsStack.Clear();
                    FieldSyntax[] fields = null;
                    if (fToken.Id == TokenId.OpenParenthesis)
                        fields = ParseFields();
                    ValidateToken(TokenId.Equal, TextResource.EqualSignExpectedInPatternDefinition);
                    NextToken();
                    Syntax body = ParsePatternBody();
                    IList<PatternSyntax> nestedPatterns;
                    if (fToken.Id == TokenId.WhereKeyword)
                    {
                        NextToken();
                        string masterPatternName = Syntax.GetFullName(fCurrentScope.MasterPatternName, name);
                        fScopeStack.Push(fCurrentScope);
                        fCurrentScope = new NameScope(fCurrentScope.Namespace, masterPatternName);
                        try
                        {
                            nestedPatterns = ParseNestedPatterns();
                        }
                        finally
                        {
                            fCurrentScope = fScopeStack.Pop();
                        }
                    }
                    else
                        nestedPatterns = Syntax.EmptyPatternList();
                    ValidateToken(TokenId.Semicolon, TextResource.PatternShouldEndWithSemicolon);
                    NextToken();
                    pattern = new PatternSyntax(fCurrentScope.Namespace, fCurrentScope.MasterPatternName,
                        isSearchTarget, name, fields, body, nestedPatterns);
                    SetTextRange(pattern, startPosition);
                    fPatternByName.Add(fullName, pattern);
                }
                else
                    throw SyntaxError(TextResource.DuplicatedPatternName, name);
            }
            else
                throw SyntaxError(TextResource.PatternDefinitionExpected, fToken);
            return pattern;
        }

        private PatternSyntax[] ParseNestedPatterns()
        {
            ValidateToken(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected);
            NextToken();
            var nestedPatterns = new List<PatternSyntax>();
            while (fToken.Id != TokenId.CloseCurlyBrace)
            {
                switch (fToken.Id)
                {
                    case TokenId.PatternKeyword:
                        int startPosition1 = fToken.TextSlice.Position;
                        NextToken();
                        PatternSyntax pattern1 = SetTextRange(ParsePattern(isSearchTarget: false), startPosition1);
                        nestedPatterns.Add(pattern1);
                        break;
                    case TokenId.SearchKeyword:
                        int startPosition2 = fToken.TextSlice.Position;
                        NextToken();
                        if (fToken.Id == TokenId.PatternKeyword)
                        {
                            PatternSyntax pattern2 = SetTextRange(ParsePattern(isSearchTarget: true), startPosition2);
                            nestedPatterns.Add(pattern2);
                        }
                        else
                            throw SyntaxError(TextResource.PatternDefinitionExpected, fToken);
                        break;
                    default:
                        PatternSyntax pattern3 = ParsePattern(isSearchTarget: false);
                        nestedPatterns.Add(pattern3);
                        break;
                }
            }
            NextToken();
            return nestedPatterns.ToArray();
        }

        private FieldSyntax[] ParseFields()
        {
            ValidateToken(TokenId.OpenParenthesis, TextResource.ListOfFieldNamesExpected);
            var result = new List<FieldSyntax>();
            do
            {
                NextToken();
                int startPosition = fToken.TextSlice.Position;
                if (fToken.Id != TokenId.CloseParenthesis)
                {
                    bool isInternal = false;
                    if (fToken.Id == TokenId.Tilde)
                    {
                        isInternal = true;
                        NextToken();
                    }
                    ValidateToken(TokenId.Identifier, TextResource.FieldNameExpected);
                    string name = fToken.TextSlice.ToString();
                    if (!fFieldByName.ContainsKey(name))
                    {
                        var field = Syntax.Field(name, isInternal);
                        fFieldByName.Add(name, field);
                        result.Add(field);
                        NextToken();
                        SetTextRange(field, startPosition);
                    }
                    else
                        throw SyntaxError(TextResource.DuplicatedField, name);
                }
            }
            while (fToken.Id == TokenId.Comma);
            ValidateToken(TokenId.CloseParenthesis, TextResource.CloseParenthesisOrCommaExpected);
            NextToken();
            return result.ToArray();
        }

        private Syntax ParsePatternBody()
        {
            Syntax result = ParseInsideOrOutsideOrHaving();
            return result;
        }

        private Syntax ParseInsideOrOutsideOrHaving()
        {
            int startPosition = fToken.TextSlice.Position;
            Syntax result = ParseConjunction();
            if (fToken.Id == TokenId.InsideKeyword || fToken.Id == TokenId.OutsideKeyword
                || fToken.Id == TokenId.HavingKeyword)
            {
                while (fToken.Id == TokenId.InsideKeyword || fToken.Id == TokenId.OutsideKeyword
                    || fToken.Id == TokenId.HavingKeyword)
                {
                    fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
                    TokenId operation = fToken.Id;
                    NextToken();
                    Syntax right = ParseConjunction();
                    switch (operation)
                    {
                        case TokenId.InsideKeyword:
                            result = Syntax.Inside(result, right);
                            break;
                        case TokenId.OutsideKeyword:
                            result = Syntax.Outside(result, right);
                            break;
                        case TokenId.HavingKeyword:
                            result = Syntax.Having(result, right);
                            break;
                    }
                    fAccessibleFields = fAccessibleFieldsStack.Pop();
                }
                result = SetTextRange(result, startPosition);
            }
            return result;
        }

        private Syntax ParseConjunction()
        {
            int startPosition = fToken.TextSlice.Position;
            Syntax result = ParseAnySpanOrWordSpan();
            if (fToken.Id == TokenId.Amphersand)
            {
                var elements = new List<Syntax> { result };
                while (fToken.Id == TokenId.Amphersand)
                {
                    NextToken();
                    Syntax element = ParseAnySpanOrWordSpan();
                    elements.Add(element);
                }
                result = SetTextRange(Syntax.Conjunction(elements), startPosition);
            }
            return result;
        }

        private Syntax ParseAnySpanOrWordSpan()
        {
            int startPosition = fToken.TextSlice.Position;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Syntax result = ParseWordSequence();
            if (fToken.Id == TokenId.DoublePeriod || fToken.Id == TokenId.Ellipsis)
            {
                while (fToken.Id == TokenId.DoublePeriod || fToken.Id == TokenId.Ellipsis)
                {
                    bool isWordSpan = false;
                    Range spanRange = new Range(0, Range.Max);
                    Syntax exclusion = null;
                    Syntax extractionOfSpan = null;
                    switch (fToken.Id)
                    {
                        case TokenId.DoublePeriod:
                            NextToken();
                            if (fToken.Id == TokenId.Identifier)
                            {
                                extractionOfSpan = ParseSpanExtraction();
                                if (fToken.Id == TokenId.Colon)
                                {
                                    NextToken();
                                    ValidateToken(TokenId.OpenSquareBracket, TextResource.OpenSquareBracketExpected);
                                }
                            }
                            if (fToken.Id == TokenId.OpenSquareBracket)
                            {
                                isWordSpan = true;
                                NextToken();
                                spanRange = ParseNumericRange();
                                ValidateToken(TokenId.CloseSquareBracket, TextResource.CloseSquareBracketExpected);
                                NextToken();
                                if (fToken.Id == TokenId.Tilde)
                                {
                                    NextToken();
                                    exclusion = ParsePrimaryExpression();
                                }
                            }
                            ValidateToken(TokenId.DoublePeriod, TextResource.DoublePeriodExpected);
                            NextToken();
                            break;
                        case TokenId.Ellipsis:
                            NextToken();
                            break;
                    }
                    Syntax next = ParseWordSequence();
                    if (isWordSpan)
                        result = Syntax.WordSpan(result, spanRange, next, exclusion, extractionOfSpan);
                    else
                        result = Syntax.AnySpan(result, next, extractionOfSpan);
                }
                result = SetTextRange(result, startPosition);
            }
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            return result;
        }

        private Syntax ParseSpanExtraction()
        {
            ValidateToken(TokenId.Identifier, TextResource.IdentifierExpected);
            string fieldName = fToken.TextSlice.ToString();
            int startPosition = fToken.TextSlice.Position;
            NextToken();
            Syntax result;
            if (fFieldByName.TryGetValue(fieldName, out FieldSyntax field))
            {
                if (fExtractedFields.Add(field))
                {
                    result = SetTextRange(Syntax.Extraction(field), startPosition);
                    fAccessibleFields.Add(field);
                }
                else
                    throw SyntaxError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name);
            }
            else
                throw SyntaxError(TextResource.UnknownField, fieldName);
            return result;
        }

        private Syntax ParseWordSequence()
        {
            int startPosition = fToken.TextSlice.Position;
            Syntax result = ParseSequence();
            if (fToken.Id == TokenId.Underscore)
            {
                var elements = new List<Syntax> { result };
                while (fToken.Id == TokenId.Underscore)
                {
                    NextToken();
                    Syntax element = ParseSequence();
                    elements.Add(element);
                }
                result = SetTextRange(Syntax.WordSequence(elements), startPosition);
            }
            return result;
        }

        private Syntax ParseSequence()
        {
            int startPosition = fToken.TextSlice.Position;
            Syntax result = ParsePrimaryExpression();
            if (fToken.Id == TokenId.Plus)
            {
                var elements = new List<Syntax> { result };
                while (fToken.Id == TokenId.Plus)
                {
                    NextToken();
                    Syntax element = ParsePrimaryExpression();
                    elements.Add(element);
                }
                result = SetTextRange(Syntax.Sequence(elements), startPosition);
            }
            return result;
        }

        private Syntax ParsePrimaryExpression()
        {
            Syntax result;
            switch (fToken.Id)
            {
                case TokenId.OpenParenthesis:
                    NextToken();
                    result = ParseInsideOrOutsideOrHaving();
                    ValidateToken(TokenId.CloseParenthesis, TextResource.CloseParenthesisOrOperatorExpected);
                    NextToken();
                    break;
                case TokenId.OpenCurlyBrace:
                    result = ParseVariation();
                    break;
                case TokenId.OpenSquareBracket:
                    result = ParseSpan();
                    break;
                case TokenId.Question:
                    int startPosition = fToken.TextSlice.Position;
                    NextToken();
                    Syntax body = ParsePrimaryExpression();
                    result = SetTextRange(Syntax.Optionality(body), startPosition);
                    break;
                case TokenId.Identifier:
                    result = ParseExtractionOrReference();
                    break;
                case TokenId.StringLiteral:
                    result = ParseText();
                    break;
                default:
                    throw SyntaxError(TextResource.IdentifierOrStringLiteralExpected);
            }
            return result;
        }

        private VariationSyntax ParseVariation()
        {
            ValidateToken(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected);
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            int startPosition = fToken.TextSlice.Position;
            var elements = new List<Syntax>();
            do
            {
                NextToken();
                Syntax element;
                if (fToken.Id == TokenId.Tilde)
                    element = ParseException();
                else
                    element = ParseInsideOrOutsideOrHaving();
                elements.Add(element);
            }
            while (fToken.Id == TokenId.Comma);
            ValidateToken(TokenId.CloseCurlyBrace, TextResource.CloseCurlyBraceOrCommaExpected);
            NextToken();
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            return SetTextRange(Syntax.Variation(elements), startPosition);
        }

        private ExceptionSyntax ParseException()
        {
            ValidateToken(TokenId.Tilde, TextResource.TildeSignExpected);
            int startPosition = fToken.TextSlice.Position;
            NextToken();
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Syntax body = ParseInsideOrOutsideOrHaving();
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            return SetTextRange(Syntax.Exception(body), startPosition);
        }

        private SpanSyntax ParseSpan()
        {
            ValidateToken(TokenId.OpenSquareBracket, TextResource.OpenSquareBracketExpected);
            int startPosition = fToken.TextSlice.Position;
            var elements = new List<Syntax>();
            do
            {
                NextToken();
                Syntax element;
                if (fToken.Id == TokenId.Tilde)
                    element = ParseException();
                else
                    element = ParseRepetition();
                elements.Add(element);
            }
            while (fToken.Id == TokenId.Comma);
            ValidateToken(TokenId.CloseSquareBracket, TextResource.CloseSquareBracketExpected);
            NextToken();
            return SetTextRange(Syntax.Span(elements), startPosition);
        }

        private RepetitionSyntax ParseRepetition()
        {
            int startPosition = fToken.TextSlice.Position;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Range repetitionRange = ParseNumericRange();
            Syntax body = ParseInsideOrOutsideOrHaving();
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            var result = Syntax.Repetition(repetitionRange.LowBound, repetitionRange.HighBound, body);
            return SetTextRange(result, startPosition);
        }

        private Range ParseNumericRange()
        {
            Range result;
            switch (fToken.Id)
            {
                case TokenId.Question:
                    NextToken();
                    result = new Range(0, 1);
                    break;
                case TokenId.IntegerLiteral:
                    result = new Range();
                    result.LowBound = ParseNumericRangeBound();
                    result.HighBound = result.LowBound;
                    switch (fToken.Id)
                    {
                        case TokenId.Plus:
                            NextToken();
                            result.HighBound = Range.Max;
                            break;
                        case TokenId.Minus:
                            NextToken();
                            result.HighBound = ParseNumericRangeBound();
                            if (result.LowBound > result.HighBound)
                                throw SyntaxError(TextResource.NumericRangeLowBoundCannotBeGreaterThanHighBound);
                            break;
                    }
                    break;
                default:
                    throw SyntaxError(TextResource.IntegerLiteralExpected);
            }
            return result;
        }

        private int ParseNumericRangeBound()
        {
            int result;
            if (fToken.Id == TokenId.IntegerLiteral)
            {
                if (int.TryParse(fToken.TextSlice.ToString(), out result))
                {
                    if (result >= 0 && result != int.MaxValue)
                        NextToken();
                    else
                        throw SyntaxError(TextResource.InvalidValueOfNumericRangeBound, result);
                }
                else
                    throw SyntaxError(TextResource.StringLiteralCannotBeConvertedToIntegerValue, fToken);
            }
            else
                throw SyntaxError(TextResource.IntegerLiteralExpected);
            return result;
        }

        private Syntax ParseExtractionOrReference()
        {
            int startPosition = fToken.TextSlice.Position;
            Syntax result;
            string name = ParseMultipartIdentifier(shouldStartFromIdentifier: false, canEndWithWildcard: false);
            if (fFieldByName.TryGetValue(name, out FieldSyntax field))
            {
                if (fToken.Id == TokenId.Colon)
                {
                    NextToken();
                    if (fExtractedFields.Add(field))
                    {
                        Syntax body = ParsePrimaryExpression();
                        result = Syntax.Extraction(field, body);
                        fAccessibleFields.Add(field);
                    }
                    else
                        throw SyntaxError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name);
                }
                else
                {
                    if (fAccessibleFields.Contains(field))
                        result = Syntax.FieldReference(field);
                    else
                        throw SyntaxError(TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse, field.Name);
                }
            }
            else
                result = ParsePatternReference(name);
            return SetTextRange(result, startPosition);
        }

        private Syntax ParsePatternReference(string patternName)
        {
            Syntax result;
            if (fStandardPatterns.TryGetValue(patternName, out PatternSyntax pattern))
            {
                if (fToken.Id == TokenId.OpenParenthesis)
                {
                    if (pattern.Body is TokenSyntax tokenSyntax)
                    {
                        switch (tokenSyntax.TokenKind)
                        {
                            case TokenKind.Word:
                                WordAttributes wordAttributes = ParseWordAttributes(
                                    tokenSyntax.TokenAttributes as WordAttributes);
                                if (wordAttributes != null)
                                    result = Syntax.Token(wordAttributes.WordClass, wordAttributes.LengthRange,
                                        wordAttributes.CharCase);
                                else
                                    result = Syntax.Token(TokenKind.Word);
                                break;
                            case TokenKind.Space:
                            case TokenKind.LineBreak:
                                NextToken();
                                Range lengthRange = ParseNumericRange();
                                result = Syntax.Token(tokenSyntax.TokenKind, lengthRange);
                                ValidateToken(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
                                NextToken();
                                break;
                            default:
                                throw SyntaxError(TextResource.AttributesAreNotAllowedForStandardPattern, patternName);
                        }
                    }
                    else
                        throw SyntaxError(TextResource.AttributesAreNotAllowedForStandardPattern, patternName);
                }
                else
                    result = pattern.Body switch
                    {
                        TokenSyntax token => new TokenSyntax(token.TokenKind, token.Text, token.IsCaseSensitive, token.TextIsPrefix, token.TokenAttributes),
                        VariationSyntax variation => new VariationSyntax(variation.Elements, checkCanReduce: false),
                        _ => throw SyntaxError(TextResource.InternalCompilerError)
                    };
            }
            else
            {
                var extractionFromFields = new List<Syntax>();
                if (fToken.Id == TokenId.OpenParenthesis)
                {
                    do
                    {
                        NextToken();
                        if (fToken.Id != TokenId.CloseParenthesis)
                        {
                            Syntax extractionFromField = ParseExtractionFromField();
                            extractionFromFields.Add(extractionFromField);
                        }
                    }
                    while (fToken.Id == TokenId.Comma);
                    ValidateToken(TokenId.CloseParenthesis, TextResource.CloseParenthesisOrCommaExpected);
                    NextToken();
                }
                result = Syntax.PatternReference(patternName, extractionFromFields);
            }
            return result;
        }

        private Syntax ParseExtractionFromField()
        {
            Syntax result;
            ValidateToken(TokenId.Identifier, TextResource.FieldNameExpected);
            int startPosition = fToken.TextSlice.Position;
            string fieldName = fToken.TextSlice.ToString();
            if (fFieldByName.TryGetValue(fieldName, out FieldSyntax field))
            {
                if (!fExtractedFields.Contains(field))
                {
                    NextToken();
                    ValidateToken(TokenId.Colon, TextResource.ColonExpected);
                    NextToken();
                    ValidateToken(TokenId.Identifier, TextResource.FieldNameExpected);
                    string fromFieldName = fToken.TextSlice.ToString();
                    NextToken();
                    result = SetTextRange(Syntax.ExtractionFromField(field, fromFieldName), startPosition);
                    fExtractedFields.Add(field);
                    fAccessibleFields.Add(field);
                }
                else
                    throw SyntaxError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name);
            }
            else
                throw SyntaxError(TextResource.UnknownField, fieldName);
            return result;
        }

        private TextSyntax ParseText()
        {
            int startPosition = fToken.TextSlice.Position;
            TextSyntax result;
            bool isCaseSensitive;
            bool textIsPrefix;
            string text = ParseStringLiteral(out isCaseSensitive, out textIsPrefix);
            if (textIsPrefix && fToken.Id == TokenId.OpenParenthesis)
            {
                WordAttributes attributes = ParseTextAttributes(allowWordClass: true);
                result = Syntax.Text(text, isCaseSensitive, attributes);
            }
            else
                result = Syntax.Text(text, isCaseSensitive, textIsPrefix);
            return SetTextRange(result, startPosition);
        }

        private string ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix)
        {
            ValidateToken(TokenId.StringLiteral, TextResource.StringLiteralExpected);
            char quote = fToken.TextSlice[0];
            isCaseSensitive = false;
            textIsPrefix = false;
            int cutNum = 0;
            if (fToken.TextSlice[fToken.TextSlice.Length - 1] == '*')
            {
                textIsPrefix = true;
                cutNum++;
            }
            if (fToken.TextSlice[fToken.TextSlice.Length - 1 - cutNum] == '!')
            {
                isCaseSensitive = true;
                cutNum++;
            }
            string text = fToken.TextSlice.SubSlice(1, fToken.TextSlice.Length - 2 - cutNum).ToString();
            if (!string.IsNullOrEmpty(text))
            {
                switch (quote)
                {
                    case '\'':
                        text = text.Replace("''", "'");
                        break;
                    case '"':
                        text = text.Replace("\"\"", "\"");
                        break;
                }
                NextToken();
            }
            else
                throw SyntaxError(TextResource.NonEmptyStringLiteralExpected, fToken);
            return text;
        }

        private WordAttributes ParseTextAttributes(bool allowWordClass)
        {
            ValidateToken(TokenId.OpenParenthesis, TextResource.OpenParenthesisExpected);
            NextToken();
            WordAttributes result = null;
            WordClass wordClass = WordClass.Any;
            Range lengthRange = Range.ZeroPlus();
            CharCase charCase = CharCase.Undefined;
            if (fToken.Id != TokenId.CloseParenthesis)
            {
                if (allowWordClass && fToken.Id == TokenId.Identifier)
                {
                    string value = fToken.TextSlice.ToString();
                    if (IsWordClass(value, out wordClass))
                        NextToken();
                    if (fToken.Id == TokenId.Comma)
                        NextToken();
                }
                if (fToken.Id == TokenId.IntegerLiteral)
                {
                    lengthRange = ParseNumericRange();
                    if (fToken.Id == TokenId.Comma)
                        NextToken();
                }
                if (fToken.Id == TokenId.Identifier)
                {
                    string value = fToken.TextSlice.ToString();
                    if (IsCharCase(value, out charCase))
                        NextToken();
                    else
                        throw SyntaxError(TextResource.UnknownWordAttribute, value);
                }
                ValidateToken(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
                if (wordClass != WordClass.Any || !lengthRange.IsZeroPlus() || charCase != CharCase.Undefined)
                    result = new WordAttributes(wordClass, lengthRange, charCase);
            }
            NextToken();
            return result;
        }

        private WordAttributes ParseWordAttributes(WordAttributes attributes)
        {
            WordAttributes result = ParseTextAttributes(allowWordClass: false);
            if (result != null)
            {
                WordClass wordClass = attributes != null ? attributes.WordClass : WordClass.Any;
                result = new WordAttributes(wordClass, result.LengthRange, result.CharCase);
            }
            return result;
        }

        private bool IsWordClass(string value, out WordClass wordClass)
        {
            bool result = true;
            switch (value)
            {
                case "Alpha":
                    wordClass = WordClass.Alpha;
                    break;
                case "Num":
                    wordClass = WordClass.Num;
                    break;
                case "AlphaNum":
                    wordClass = WordClass.AlphaNum;
                    break;
                case "NumAlpha":
                    wordClass = WordClass.NumAlpha;
                    break;
                default:
                    wordClass = WordClass.Any;
                    result = false;
                    break;
            }
            return result;
        }

        private bool IsCharCase(string value, out CharCase charCase)
        {
            bool result = true;
            switch (value)
            {
                case "Lowercase":
                    charCase = CharCase.Lowercase;
                    break;
                case "Uppercase":
                    charCase = CharCase.Uppercase;
                    break;
                case "TitleCase":
                    charCase = CharCase.TitleCase;
                    break;
                default:
                    charCase = CharCase.Undefined;
                    result = false;
                    break;
            }
            return result;
        }

        private void NextToken()
        {
            do
            {
                NextTokenOrComment();
            } while (fToken.Id == TokenId.Comment);
        }

        private void NextTokenOrComment()
        {
            while (char.IsWhiteSpace(fCharacter))
                NextCharacter();
            TokenId tokenId = TokenId.Unknown;
            int tokenPosition = fTextPosition;
            switch (fCharacter)
            {
                case '(':
                    NextCharacter();
                    tokenId = TokenId.OpenParenthesis;
                    break;
                case ')':
                    NextCharacter();
                    tokenId = TokenId.CloseParenthesis;
                    break;
                case '{':
                    NextCharacter();
                    tokenId = TokenId.OpenCurlyBrace;
                    break;
                case '}':
                    NextCharacter();
                    tokenId = TokenId.CloseCurlyBrace;
                    break;
                case '[':
                    NextCharacter();
                    tokenId = TokenId.OpenSquareBracket;
                    break;
                case ']':
                    NextCharacter();
                    tokenId = TokenId.CloseSquareBracket;
                    break;
                case '.':
                    NextCharacter();
                    if (fCharacter == '.')
                    {
                        NextCharacter();
                        if (fCharacter == '.')
                        {
                            NextCharacter();
                            tokenId = TokenId.Ellipsis;
                        }
                        else
                            tokenId = TokenId.DoublePeriod;
                    }
                    else
                        tokenId = TokenId.Period;
                    break;
                case ',':
                    NextCharacter();
                    tokenId = TokenId.Comma;
                    break;
                case ':':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.Assignment;
                    }
                    else if (fCharacter == ':')
                    {
                        NextCharacter();
                        tokenId = TokenId.DoubleColon;
                    }
                    else
                        tokenId = TokenId.Colon;
                    break;
                case ';':
                    NextCharacter();
                    tokenId = TokenId.Semicolon;
                    break;
                case '#':
                    NextCharacter();
                    tokenId = TokenId.HashSign;
                    break;
                case '~':
                    NextCharacter();
                    tokenId = TokenId.Tilde;
                    break;
                case '@':
                    NextCharacter();
                    if (char.IsLetter(fCharacter))
                    {
                        NextCharacter();
                        while (char.IsLetterOrDigit(fCharacter) || fCharacter == '-')
                            NextCharacter();
                        Slice keywordSlice = fText.SubSlice(tokenPosition, fTextPosition - tokenPosition);
                        string keyword = keywordSlice.ToString();
                        if (!fTokenByKeyword.TryGetValue(keyword, out tokenId))
                            throw SyntaxError(fTextPosition, TextResource.UnknownKeyword, keyword);
                    }
                    else
                        tokenId = TokenId.CommercialAt;
                    break;
                case '+':
                    NextCharacter();
                    tokenId = TokenId.Plus;
                    break;
                case '-':
                    NextCharacter();
                    tokenId = TokenId.Minus;
                    break;
                case '*':
                    NextCharacter();
                    tokenId = TokenId.Asterisk;
                    break;
                case '/':
                    NextCharacter();
                    if (fCharacter == '/')
                    {
                        NextCharacter();
                        while (fTextPosition < fText.Length && fCharacter != '\n')
                            NextCharacter();
                        if (fTextPosition < fText.Length)
                            NextCharacter();
                        tokenId = TokenId.Comment;
                    }
                    else if (fCharacter == '*')
                    {
                        NextCharacter();
                        char previousCharacter = '\0';
                        while (fTextPosition < fText.Length && !(previousCharacter == '*' && fCharacter == '/'))
                        {
                            previousCharacter = fCharacter;
                            NextCharacter();
                        }
                        if (fTextPosition < fText.Length)
                            NextCharacter();
                        else
                            throw SyntaxError(fTextPosition, TextResource.UnterminatedComment);
                        tokenId = TokenId.Comment;
                    }
                    else
                        tokenId = TokenId.Slash;
                    break;
                case '&':
                    NextCharacter();
                    tokenId = TokenId.Amphersand;
                    break;
                case '?':
                    NextCharacter();
                    tokenId = TokenId.Question;
                    break;
                case '=':
                    NextCharacter();
                    tokenId = TokenId.Equal;
                    break;
                case '_':
                    NextCharacter();
                    tokenId = TokenId.Underscore;
                    break;
                case '!':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.ExclamationEqual;
                    }
                    else
                        tokenId = TokenId.Exclamation;
                    break;
                case '<':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.LessThanEqual;
                    }
                    else
                        tokenId = TokenId.LessThan;
                    break;
                case '>':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.GreaterThanEqual;
                    }
                    else
                        tokenId = TokenId.GreaterThan;
                    break;
                case '"':
                case '\'':
                    char quote = fCharacter;
                    do
                    {
                        NextCharacter();
                        while (fTextPosition < fText.Length && fCharacter != quote)
                            NextCharacter();
                        if (fTextPosition < fText.Length)
                            NextCharacter();
                        else
                            throw SyntaxError(fTextPosition, TextResource.UnterminatedStringLiteral);
                    } while (fCharacter == quote);
                    if (fCharacter == '!')
                        NextCharacter();
                    if (fCharacter == '*')
                        NextCharacter();
                    tokenId = TokenId.StringLiteral;
                    break;
                default:
                    if (char.IsLetter(fCharacter))
                    {
                        NextCharacter();
                        while (char.IsLetterOrDigit(fCharacter) || fCharacter == '-')
                            NextCharacter();
                        tokenId = TokenId.Identifier;
                        break;
                    }
                    if (char.IsDigit(fCharacter))
                    {
                        tokenId = TokenId.IntegerLiteral;
                        NextCharacter();
                        while (char.IsDigit(fCharacter))
                            NextCharacter();
                        if (fCharacter == '.')
                        {
                            tokenId = TokenId.RealLiteral;
                            NextCharacter();
                            NextDigits();
                        }
                        if (fCharacter == 'E' || fCharacter == 'e')
                        {
                            tokenId = TokenId.RealLiteral;
                            NextCharacter();
                            if (fCharacter == '+' || fCharacter == '-')
                                NextCharacter();
                            NextDigits();
                        }
                        if (fCharacter == 'F' || fCharacter == 'f')
                            NextCharacter();
                        break;
                    }
                    // Take next character to allow using fToken to format error message with the unknown character.
                    NextCharacter();
                    break;
            }
            if (tokenId == TokenId.Unknown && fTextPosition == fText.Length)
                tokenId = TokenId.End;
            fToken.Id = tokenId;
            fToken.TextSlice = fText.SubSlice(tokenPosition, fTextPosition - tokenPosition);
        }

        private void NextCharacter()
        {
            if (fTextPosition < fText.Length)
                fTextPosition++;
            if (fTextPosition < fText.Length)
            {
                fCharacter = fText.Source[fTextPosition];
                if (fCharacter == '\u000A')
                {
                    fLineNumber++;
                    fLinePosition = fTextPosition + 1;
                    fLineLength = 0;
                }
                else
                    fLineLength++;
            }
            else
                fCharacter = '\0';
        }

        private void NextDigits()
        {
            ValidateDigit();
            NextCharacter();
            while (char.IsDigit(fCharacter))
                NextCharacter();
        }

        private void ValidateDigit()
        {
            if (!char.IsDigit(fCharacter))
                throw SyntaxError(fTextPosition, TextResource.DigitExpected);
        }

        private void ValidateToken(TokenId id, string error)
        {
            if (fToken.Id != id)
                throw SyntaxError(fToken.TextSlice.Position, error, args: fToken);
        }

        private T SetTextRange<T>(T syntax, int start) where T : Syntax
        {
            int end = fToken.TextSlice.Position;
            syntax.TextRange = new TextRange(start, end);
            return syntax;
        }

        private struct Token
        {
            public TokenId Id;
            public Slice TextSlice;

            public override string ToString()
            {
                string result;
                if (Id != TokenId.End)
                    result = TextSlice.ToString();
                else
                    result = "<end>";
                return result;
            }
        }

        private enum TokenId
        {
            Unknown,
            End,
            Comment,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            OpenParenthesis,
            CloseParenthesis,
            OpenCurlyBrace,
            CloseCurlyBrace,
            OpenSquareBracket,
            CloseSquareBracket,
            Period,
            Comma,
            Colon,
            Semicolon,
            HashSign,
            Tilde,
            CommercialAt,
            Plus,
            Minus,
            Asterisk,
            Slash,
            Amphersand,
            Question,
            Exclamation,
            Equal,
            Underscore,
            LessThan,
            GreaterThan,
            DoublePeriod,
            Ellipsis,
            ExclamationEqual,
            LessThanEqual,
            GreaterThanEqual,
            Assignment,
            DoubleColon,
            RequireKeyword,
            NamespaceKeyword,
            PatternKeyword,
            SearchKeyword,
            WhereKeyword,
            InsideKeyword,
            OutsideKeyword,
            HavingKeyword,
        }
    }

    public class NameScope
    {
        public string Namespace { get; }
        public string MasterPatternName { get; }

        public NameScope()
            : this(string.Empty, string.Empty)
        {
        }

        public NameScope(string nameSpace, string masterPatternName)
        {
            Namespace = nameSpace;
            MasterPatternName = masterPatternName;
        }

        public string GetFullName()
        {
            return Syntax.GetFullName(Namespace, MasterPatternName);
        }
    }

    public sealed class SyntaxException : Exception
    {
        private readonly int fPosition;
        private readonly int fLineNumber;
        private readonly string fLine;

        public SyntaxException(string message, int position, int lineNumber, string line)
            : base(string.Format(TextResource.SyntaxExceptionFormat, message, position, lineNumber, line))
        {
            fPosition = position;
            fLineNumber = lineNumber;
            fLine = line;
        }

        public SyntaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public int Position
        {
            get { return fPosition; }
        }

        public int LineNumber
        {
            get { return fLineNumber; }
        }

        public string Line
        {
            get { return fLine; }
        }
    }

    internal static partial class TextResource
    {
        public const string SyntaxExceptionFormat = "{0} (at position {1}, line {2}: \"{3}\")";
        public const string RequireOperatorIsNotAllowedInSinglePackageMode = "@require operator is not allowed in single package mode";
        public const string RequireKeywordExpected = "@require keyword expected, but '{0}' found";
        public const string RequiredFilePathExpected = "Required file path expected, but '{0}' found";
        public const string RequireDefinitionShouldEndWithSemicolon = "@require definition should end with semicolon";
        public const string DuplicatedRequiredPackage = "Duplicated required package: '{0}'";
        public const string FilePathAsStringLiteralExpected = "File path as string literal expected, but '{0}' found";
        public const string InvalidSpecifierAfterStringLiteral = "Invalid specifier after string literal";
        public const string NamespaceKeywordExpected = "@namespace keyword expected, but '{0}' found";
        public const string NamespaceIdentifierExpected = "Namespace identifier expected, but '{0}' found";
        public const string NamespaceDefinitionShouldEndWithColon = "@namespace definition should end with colon";
        public const string DigitExpected = "Digit expected";
        public const string UnknownKeyword = "Unknown keyword '{0}'";
        public const string UnterminatedComment = "Unterminated comment";
        public const string UnterminatedStringLiteral = "Unterminated string literal";
        public const string IdentifierExpected = "Identifier expected, but '{0}' found";
        public const string IdentifierOrPeriodExpected = "Identifier or period expected, but '{0}' found";
        public const string IdentifierOrAsteriskExpected = "Identifier or asterisk expected, but '{0}' found";
        public const string DuplicatedPatternName = "Duplicated pattern name '{0}'";
        public const string DuplicatedField = "Duplicated field '{0}'";
        public const string PatternIdentifierExpected = "Pattern identifier expected, but '{0}' found";
        public const string MultipleSearchQueriesNotAllowedInPackage = "Multiple search queries not allowed in a package";
        public const string SearchTargetDefinitionExpected = "Search target definition expected, but '{0}' found";
        public const string SearchTargetDefinitionShouldEndWithSemicolon = "Search target definition should end with semicolon";
        public const string PatternDefinitionExpected = "Pattern definition expected, but '{0}' found";
        public const string PatternShouldEndWithSemicolon = "Pattern should end with semicolon";
        public const string EqualSignExpectedInPatternDefinition = "Equal sign expected in pattern definition, but '{0}' found";
        public const string FieldNameExpected = "Field name expected, but '{0}' found";
        public const string ListOfFieldNamesExpected = "List of field names expected, but '{0}' found";
        public const string UnknownField = "Unknown field '{0}'";
        public const string ValueOfFieldShouldBeExtractedFromTextBeforeUse = "Value of field '{0}' should be extracted from text before use";
        public const string FieldAlreadyUsedForTextExtraction = "Field '{0}' already used for text extraction";
        public const string OpenParenthesisExpected = "Open parenthesis expected, but '{0}' found";
        public const string CloseParenthesisExpected = "Close parenthesis expected, but '{0}' found";
        public const string CloseParenthesisOrCommaExpected = "Close parenthesis or comma expected, but '{0}' found";
        public const string CloseParenthesisOrOperatorExpected = "Close parenthesis or operator expected, but '{0}' found";
        public const string DoublePeriodExpected = "Double period expected, but '{0}' found";
        public const string IdentifierOrStringLiteralExpected = "Identifier or string literal expected, but '{0}' found";
        public const string StringLiteralExpected = "String literal expected, but '{0}' found";
        public const string NonEmptyStringLiteralExpected = "Expected non-empty string literal";
        public const string OpenCurlyBraceExpected = "Open curly brace expected, but '{0}' found";
        public const string CloseCurlyBraceExpected = "Close curly brace expected, but '{0}' found";
        public const string CloseCurlyBraceOrCommaExpected = "Close curly brace or comma expected, but '{0}' found";
        public const string OpenSquareBracketExpected = "Open square bracket expected, but '{0}' found";
        public const string CloseSquareBracketExpected = "Close square bracket expected, but '{0}' found";
        public const string TildeSignExpected = "Tilde sign expected, but '{0}' found";
        public const string IntegerLiteralExpected = "Integer literal expected, but '{0}' found";
        public const string StringLiteralCannotBeConvertedToIntegerValue = "String literal '{0}' cannot be converted to integer value";
        public const string InvalidValueOfNumericRangeBound = "Invalid value of numeric range bound: '{0}', the value should be greater or equal to 0 and less than maximum 32-bit integer";
        public const string NumericRangeLowBoundCannotBeGreaterThanHighBound = "Numeric range low bound cannot be greater than high bound";
        public const string ColonExpected = "Colon expected, but '{0}' found";
        public const string AssignmentExpected = "Assignment (:=) expected, but '{0}' found";
        public const string AttributesAreNotAllowedForStandardPattern = "Attributes are not allowed for standard pattern '{0}'";
        public const string WordAttributeExpected = "Word token attribute expected, but '{0}' found";
        public const string WordClassWasAlreadyDefined = "Word class was already defined";
        public const string CharacterCaseWasAlreadyDefined = "Character case was already defined";
        public const string LengthRangeWasAlreadyDefined = "Length range was already defined";
        public const string UnknownWordAttribute = "Unknown Word attribute: '{0}'";
    }
}
