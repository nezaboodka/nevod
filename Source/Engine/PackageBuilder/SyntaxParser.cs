//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nezaboodka.Text;

namespace Nezaboodka.Nevod
{
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
        private Scanner fScanner;
        private int fTextLength;
        private bool fIsErrorRecovery;
        private TextRange fPreviousTokenRange;
        private NameScope fCurrentScope;
        private Stack<NameScope> fScopeStack;
        private List<RequiredPackageSyntax> fRequiredPackages;
        private List<Syntax> fPatterns;
        private List<Syntax> fSearchTargets;
        private readonly Dictionary<string, PatternSyntax> fStandardPatterns;
        private readonly Dictionary<string, FieldSyntax> fFieldByName;
        private readonly HashSet<FieldSyntax> fExtractedFields;
        private HashSet<FieldSyntax> fAccessibleFields;
        private readonly Stack<HashSet<FieldSyntax>> fAccessibleFieldsStack;
        private List<Error> fErrors;
        private EndSign fEndSign;
        private NestingContext fNestingContext;
        private bool fIsAbortingDueToPatternDefinition;

        public SyntaxParser()
        {
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
            fScanner = new Scanner(text);
            fTextLength = text.Length;
            fErrors = new List<Error>();
            fNestingContext = NestingContext.None;
            fIsAbortingDueToPatternDefinition = false;
            NextToken();
            PackageSyntax result = ParsePackage();
            result.Errors = fErrors;
            return result;
        }

        public PackageSyntax ParseExpressionText(string text)
        {
            fScanner = new Scanner(text);
            fTextLength = text.Length;
            fErrors = new List<Error>();
            fNestingContext = NestingContext.None;
            fIsAbortingDueToPatternDefinition = false;
            NextToken();
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax patternBody = ParseInsideOrOutsideOrHaving();
            if (fScanner.CurrentToken.Id != TokenId.End)
                AddExpressionModeError(isExpressionParsed: patternBody != null);
            PatternSyntax pattern = SetTextRange(Syntax.Pattern(isSearchTarget: true, "Pattern", patternBody, null), startPosition);
            PackageSyntax result = SetTextRange(Syntax.Package(pattern), startPosition);
            result.Errors = fErrors;
            return result;
        }

        // Internal

        private void AddExpressionModeError(bool isExpressionParsed)
        {
            if (fIsAbortingDueToPatternDefinition
            || fScanner.CurrentToken.Id == TokenId.SearchKeyword
            || fScanner.CurrentToken.Id == TokenId.PatternKeyword)
                AddErrorWithoutCheck(CreateError(TextResource.PatternDefinitionsAreNotAllowedInExpressionMode));
            else if (fScanner.CurrentToken.Id == TokenId.NamespaceKeyword)
                AddErrorWithoutCheck(CreateError(TextResource.NamespacesAreNotAllowedInExpressionMode));
            else if (fScanner.CurrentToken.Id == TokenId.RequireKeyword)
                AddErrorWithoutCheck(CreateError(TextResource.RequireKeywordsAreNotAllowedInExpressionMode));
            else if (isExpressionParsed)
                AddErrorWithoutCheck(CreateError(TextResource.EndOfExpressionExpectedInExpressionMode));
        }

        private PackageSyntax ParsePackage()
        {
            fCurrentScope = new NameScope();
            fScopeStack = new Stack<NameScope>();
            fRequiredPackages = new List<RequiredPackageSyntax>();
            fPatterns = new List<Syntax>();
            fSearchTargets = new List<Syntax>();
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            // 1. Required packages
            ParseRequires();
            // 2. Pattern definitions within namespaces
            while (fScanner.CurrentToken.Id != TokenId.End)
                ParseNamespacesAndPatterns();
            PackageSyntax result = SetTextRange(Syntax.Package(fRequiredPackages, fSearchTargets, fPatterns), startPosition);
            fCurrentScope = null;
            fScopeStack = null;
            fRequiredPackages = null;
            fPatterns = null;
            fSearchTargets = null;
            return result;
        }

        private void ParseRequires()
        {
            while (fScanner.CurrentToken.Id == TokenId.RequireKeyword)
            {
                RequiredPackageSyntax requiredPackage = ParseRequire();
                if (requiredPackage != null)
                    fRequiredPackages.Add(requiredPackage);
            }
        }

        private RequiredPackageSyntax ParseRequire()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.RequireKeyword, TextResource.RequireKeywordExpected));
            RequiredPackageSyntax result = null;
            if (ValidateToken(TokenId.StringLiteral, TextResource.FilePathAsStringLiteralExpected))
            {
                string relativePath = ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix);
                if (isCaseSensitive || textIsPrefix)
                    AddError(CreateError(fPreviousTokenRange, TextResource.InvalidSpecifierAfterStringLiteral));
                ValidateTokenAndAdvance(TokenId.Semicolon, TextResource.RequireDefinitionShouldEndWithSemicolon);
                result = new RequiredPackageSyntax(relativePath);
                SetTextRange(result, startPosition);
            }
            return result;
        }

        private void ParseNamespacesAndPatterns()
        {
            switch (fScanner.CurrentToken.Id)
            {
                case TokenId.NamespaceKeyword:
                    NextToken();
                    ParseNamespace();
                    break;
                case TokenId.PatternKeyword:
                    int startPosition1 = fScanner.CurrentToken.TextSlice.Position;
                    NextToken();
                    PatternSyntax pattern1 = ParsePattern(isSearchTarget: false);
                    if (pattern1 != null)
                    {
                        SetTextRange(pattern1, startPosition1);
                        fPatterns.Add(pattern1);
                    }
                    break;
                case TokenId.SearchKeyword:
                    int startPosition2 = fScanner.CurrentToken.TextSlice.Position;
                    NextToken();
                    if (fScanner.CurrentToken.Id == TokenId.PatternKeyword)
                    {
                        NextToken();
                        PatternSyntax pattern2 = ParsePattern(isSearchTarget: true);
                        if (pattern2 != null)
                        {
                            SetTextRange(pattern2, startPosition2);
                            fPatterns.Add(pattern2);
                        }
                    }
                    else
                    {
                        SearchTargetSyntax searchTarget = ParseSearchTarget();
                        if (searchTarget != null)
                        {
                            SetTextRange(searchTarget, startPosition2);
                            fSearchTargets.Add(searchTarget);
                        }
                    }
                    break;
                case TokenId.RequireKeyword:
                    ParseRequireInWrongPlace();
                    break;
                default:
                    PatternSyntax pattern3 = ParsePattern(isSearchTarget: false);
                    if (pattern3 != null)
                        fPatterns.Add(pattern3);
                    break;
            }
        }

        private void ParseRequireInWrongPlace()
        {
            bool saveIsErrorRecovery = fIsErrorRecovery;
            fIsErrorRecovery = true;
            int errorStart = fScanner.CurrentToken.TextSlice.Position;
            ParseRequire();
            int errorEnd = fPreviousTokenRange.End;
            fIsErrorRecovery = saveIsErrorRecovery;
            var errorRange = new TextRange(errorStart, errorEnd);
            AddError(CreateError(errorRange, TextResource.RequireKeywordsAreOnlyAllowedInTheBeginning));
        }

        private SearchTargetSyntax ParseSearchTarget()
        {
            SearchTargetSyntax result;
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            if (fScanner.CurrentToken.Id == TokenId.Identifier)
            {
                string name = ParseMultipartIdentifier(canEndWithWildcard: true);
                string fullName = Syntax.GetFullName(fCurrentScope.Namespace, name);
                if (fullName.EndsWith(".*"))
                {
                    string ns = fullName.TrimEnd('*', '.');
                    result = Syntax.NamespaceSearchTarget(ns, fCurrentScope.Namespace);
                }
                else
                {
                    PatternReferenceSyntax patternReference = SetTextRange(Syntax.PatternReference(name), startPosition);
                    result = Syntax.PatternSearchTarget(fullName, fCurrentScope.Namespace, patternReference);
                }
                ValidateTokenAndAdvance(TokenId.Semicolon, TextResource.SearchTargetDefinitionShouldEndWithSemicolon);
            }
            else
            {
                AddError(CreateError(TextResource.IdentifierExpected));
                result = Syntax.PatternSearchTarget(fullName: null, fCurrentScope.Namespace, patternReference: null);
            }
            return SetTextRange(result, startPosition);
        }

        private void ParseNamespace()
        {
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfNamespaceBody;
            string name = ParseMultipartIdentifier(canEndWithWildcard: false);
            ValidateTokenAndAdvance(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected);
            fScopeStack.Push(fCurrentScope);
            string nameSpace = Syntax.GetFullName(fCurrentScope.Namespace, name);
            fCurrentScope = new NameScope(nameSpace, fCurrentScope.MasterPatternName);
            try
            {
                while (fScanner.CurrentToken.Id != TokenId.CloseCurlyBrace && fScanner.CurrentToken.Id != TokenId.End)
                    ParseNamespacesAndPatterns();
                ValidateTokenAndAdvance(TokenId.CloseCurlyBrace, TextResource.CloseCurlyBraceExpected);
            }
            finally
            {
                fCurrentScope = fScopeStack.Pop();
            }
            fEndSign = saveEndSign;
        }

        private string ParseMultipartIdentifier(bool canEndWithWildcard)
        {
            var result = new StringBuilder();
            if (fScanner.CurrentToken.Id == TokenId.Identifier)
            {
                result.Append(fScanner.CurrentToken.TextSlice.ToString());
                NextToken();
            }
            else
            {
                AddError(CreateError(TextResource.IdentifierExpected));
                return "";
            }
            while (fScanner.CurrentToken.Id == TokenId.Period)
            {
                result.Append('.');
                NextToken();
                if (fScanner.CurrentToken.Id == TokenId.Identifier)
                {
                    result.Append(fScanner.CurrentToken.TextSlice.ToString());
                    NextToken();
                }
                else if (fScanner.CurrentToken.Id == TokenId.Asterisk && canEndWithWildcard)
                {
                    result.Append(fScanner.CurrentToken.TextSlice.ToString());
                    NextToken();
                    break;
                }
                else if (canEndWithWildcard)
                {
                    AddError(CreateError(TextResource.IdentifierOrAsteriskExpected));
                    return result.ToString();
                }
                else
                {
                    AddError(CreateError(TextResource.IdentifierExpected));
                    return result.ToString();
                }
            }
            return result.ToString();
        }

        private PatternSyntax ParsePattern(bool isSearchTarget)
        {
            fIsAbortingDueToPatternDefinition = false;
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfPattern;
            PatternSyntax pattern = null;
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            if (fScanner.CurrentToken.Id == TokenId.HashSign)
            {
                isSearchTarget = true;
                NextToken();
            }
            string name = null;
            if (fScanner.CurrentToken.Id == TokenId.Identifier)
                name = ParseMultipartIdentifier(canEndWithWildcard: false);
            else
                AddError(CreateError(TextResource.PatternNameExpected));
            fFieldByName.Clear();
            fExtractedFields.Clear();
            fAccessibleFields.Clear();
            fAccessibleFieldsStack.Clear();
            FieldSyntax[] fields = null;
            if (fScanner.CurrentToken.Id == TokenId.OpenParenthesis)
                fields = ParseFields();
            ValidateTokenAndAdvance(TokenId.Equal, TextResource.EqualSignExpectedInPatternDefinition);
            if (fScanner.CurrentToken.TextSlice.Position == startPosition && !IsStartOfPrimaryExpression())
                NextToken();
            else
            {
                Syntax body = ParsePatternBody();
                if (body == null)
                    AddError(CreateError(TextResource.PatternBodyExpected));
                IList<PatternSyntax> nestedPatterns;
                if (fScanner.CurrentToken.Id == TokenId.WhereKeyword)
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
                if (fScanner.CurrentToken.Id == TokenId.Semicolon) 
                    NextToken();
                else
                    AddError(CreateErrorAfterRange(fPreviousTokenRange, TextResource.PatternShouldEndWithSemicolon));
                pattern = new PatternSyntax(fCurrentScope.Namespace, fCurrentScope.MasterPatternName,
                    isSearchTarget, name, fields, body, nestedPatterns);
                SetTextRange(pattern, startPosition);
            }
            fEndSign = saveEndSign;
            return pattern;
        }

        private PatternSyntax[] ParseNestedPatterns()
        {
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfNestedPatterns;
            ValidateTokenAndAdvance(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected);
            var nestedPatterns = new List<PatternSyntax>();
            while (fScanner.CurrentToken.Id != TokenId.CloseCurlyBrace && fScanner.CurrentToken.Id != TokenId.End)
            {
                switch (fScanner.CurrentToken.Id)
                {
                    case TokenId.PatternKeyword:
                        int startPosition1 = fScanner.CurrentToken.TextSlice.Position;
                        NextToken();
                        PatternSyntax pattern1 = ParsePattern(isSearchTarget: false);
                        if (pattern1 != null)
                        {
                            SetTextRange(pattern1, startPosition1);
                            nestedPatterns.Add(pattern1);
                        }
                        break;
                    case TokenId.SearchKeyword:
                        int startPosition2 = fScanner.CurrentToken.TextSlice.Position;
                        NextToken();
                        ValidateTokenAndAdvance(TokenId.PatternKeyword, TextResource.PatternDefinitionExpected);
                        PatternSyntax pattern2 = ParsePattern(isSearchTarget: true);
                        if (pattern2 != null)
                        {
                            SetTextRange(pattern2, startPosition2);
                            nestedPatterns.Add(pattern2);
                        }
                        break;
                    case TokenId.NamespaceKeyword:
                        AddError(CreateError(TextResource.NamespacesAreNotAllowedInNestedPatterns));
                        NextToken();
                        break;
                    case TokenId.RequireKeyword:
                        ParseRequireInWrongPlace();
                        break;
                    default:
                        PatternSyntax pattern3 = ParsePattern(isSearchTarget: false);
                        if (pattern3 != null)
                            nestedPatterns.Add(pattern3);
                        break;
                }
            }
            ValidateTokenAndAdvance(TokenId.CloseCurlyBrace, TextResource.CloseCurlyBraceExpected);
            fEndSign = saveEndSign;
            return nestedPatterns.ToArray();
        }

        private FieldSyntax[] ParseFields()
        {
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenParenthesis, TextResource.ListOfFieldNamesExpected));
            List<FieldSyntax> result;
            if (fScanner.CurrentToken.Id == TokenId.CloseParenthesis)
                result = new List<FieldSyntax>();
            else
            {
                EndSign saveEndSign = fEndSign;
                fEndSign |= EndSign.EndOfFields;
                result = ParseCommaSeparatedList(ParseField, TokenId.CloseParenthesis, IsFieldStart);
                fEndSign = saveEndSign;
            }
            ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
            return result.ToArray();
        }

        private bool IsFieldStart() => fScanner.CurrentToken.Id == TokenId.Identifier || fScanner.CurrentToken.Id == TokenId.Tilde;

        private FieldSyntax ParseField()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            var isInternal = false;
            if (fScanner.CurrentToken.Id == TokenId.Tilde)
            {
                isInternal = true;
                NextToken();
            }
            FieldSyntax result = null;
            if (fScanner.CurrentToken.Id == TokenId.Identifier)
            {
                string name = fScanner.CurrentToken.TextSlice.ToString();
                result = Syntax.Field(name, isInternal);
                if (!fFieldByName.ContainsKey(name))
                {
                    if (!fIsErrorRecovery)
                        fFieldByName.Add(name, result);
                }
                else
                    AddError(CreateError(TextResource.DuplicatedField, name));
                NextToken();
                SetTextRange(result, startPosition);
            }
            else
                AddError(CreateError(TextResource.FieldNameExpected));
            return result;
        }

        private Syntax ParsePatternBody()
        {
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.StartOfNestedPatterns;
            Syntax result = ParseInsideOrOutsideOrHaving();
            fEndSign = saveEndSign;
            return result;
        }

        private Syntax ParseInsideOrOutsideOrHaving()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax result = ParseConjunction();
            if (fScanner.CurrentToken.Id == TokenId.InsideKeyword || fScanner.CurrentToken.Id == TokenId.OutsideKeyword
                || fScanner.CurrentToken.Id == TokenId.HavingKeyword)
            {
                while (fScanner.CurrentToken.Id == TokenId.InsideKeyword || fScanner.CurrentToken.Id == TokenId.OutsideKeyword
                                                                         || fScanner.CurrentToken.Id == TokenId.HavingKeyword)
                {
                    fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
                    TokenId operation = fScanner.CurrentToken.Id;
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
                    result = SetTextRange(result, startPosition);
                }
            }
            return result;
        }

        private Syntax ParseConjunction()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax result = ParseAnySpanOrWordSpan();
            if (fScanner.CurrentToken.Id == TokenId.Amphersand)
            {
                var elements = new List<Syntax> { result };
                while (fScanner.CurrentToken.Id == TokenId.Amphersand)
                {
                    NextToken();
                    Syntax element = ParseAnySpanOrWordSpan();
                    if (element != null)
                        elements.Add(element);
                }
                result = SetTextRange(Syntax.Conjunction(elements), startPosition);
            }
            return result;
        }

        private Syntax ParseAnySpanOrWordSpan()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Syntax result = ParseWordSequence();
            if (fScanner.CurrentToken.Id == TokenId.DoublePeriod || fScanner.CurrentToken.Id == TokenId.Ellipsis)
            {
                while (fScanner.CurrentToken.Id == TokenId.DoublePeriod || fScanner.CurrentToken.Id == TokenId.Ellipsis)
                {
                    bool isWordSpan = false;
                    Range spanRange = new Range(0, Range.Max);
                    Syntax exclusion = null;
                    Syntax extractionOfSpan = null;
                    switch (fScanner.CurrentToken.Id)
                    {
                        case TokenId.DoublePeriod:
                            NextToken();
                            bool isNumericRangeRequired = false;
                            if (fScanner.CurrentToken.Id == TokenId.Identifier)
                            {
                                extractionOfSpan = ParseSpanExtraction();
                                if (fScanner.CurrentToken.Id == TokenId.Colon)
                                {
                                    NextToken();
                                    ValidateTokenAndAdvance(TokenId.OpenSquareBracket, TextResource.OpenSquareBracketExpected);
                                    isNumericRangeRequired = true;
                                }
                            }
                            if (fScanner.CurrentToken.Id == TokenId.OpenSquareBracket || isNumericRangeRequired)
                            {
                                isWordSpan = true;
                                if (fScanner.CurrentToken.Id == TokenId.OpenSquareBracket)
                                    NextToken();
                                spanRange = ParseNumericRange();
                                ValidateTokenAndAdvance(TokenId.CloseSquareBracket, TextResource.CloseSquareBracketExpected);
                                if (fScanner.CurrentToken.Id == TokenId.Tilde || IsStartOfPrimaryExpression())
                                {
                                    ValidateTokenAndAdvance(TokenId.Tilde, TextResource.TildeExpected);
                                    exclusion = ParsePrimaryExpression();
                                }
                            }
                            ValidateTokenAndAdvance(TokenId.DoublePeriod, TextResource.DoublePeriodExpected);
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
                    result = SetTextRange(result, startPosition);
                }
            }
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            return result;
        }

        private Syntax ParseSpanExtraction()
        {
            ThrowIfNotValidated(ValidateToken(TokenId.Identifier, TextResource.IdentifierExpected));
            string fieldName = fScanner.CurrentToken.TextSlice.ToString();
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax result;
            if (fFieldByName.TryGetValue(fieldName, out FieldSyntax field))
            {
                result = Syntax.Extraction(field);
                if (fExtractedFields.Add(field))
                    fAccessibleFields.Add(field);
                else
                    AddError(CreateError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name));
            }
            else
            {
                AddError(CreateError(TextResource.UndeclaredField, fieldName));
                result = Syntax.Extraction(Syntax.Field(fieldName));
            }
            NextToken();
            return SetTextRange(result, startPosition);
        }

        private Syntax ParseWordSequence()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax result = ParseSequence();
            if (fScanner.CurrentToken.Id == TokenId.Underscore)
            {
                var elements = new List<Syntax> { result };
                while (fScanner.CurrentToken.Id == TokenId.Underscore)
                {
                    NextToken();
                    Syntax element = ParseSequence();
                    if (element != null)
                        elements.Add(element);
                }
                result = SetTextRange(Syntax.WordSequence(elements), startPosition);
            }
            return result;
        }

        private Syntax ParseSequence()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax result = ParsePrimaryExpression();
            // If primary expression is followed by another primary expression without operator separating them,
            // and second primary expression is not start of a pattern, add missing operator expected error and
            // continue parsing expressions as part of sequence.
            if (fScanner.CurrentToken.Id == TokenId.Plus || !fIsAbortingDueToPatternDefinition && fNestingContext != NestingContext.Variation && IsStartOfPrimaryExpression())
            {
                var elements = new List<Syntax> { result };
                while (fScanner.CurrentToken.Id == TokenId.Plus || fNestingContext != NestingContext.Variation && IsStartOfPrimaryExpression() && !IsStartOfPattern())
                {
                    if (fScanner.CurrentToken.Id == TokenId.Plus)
                        NextToken();
                    else
                        AddError(CreateErrorAfterRange(fPreviousTokenRange, TextResource.OperatorExpected));
                    Syntax element = ParsePrimaryExpression();
                    if (element != null)
                        elements.Add(element);
                }
                result = SetTextRange(Syntax.Sequence(elements), startPosition);
            }
            return result;
        }

        private bool IsStartOfPrimaryExpression()
        {
            return fScanner.CurrentToken.Id == TokenId.OpenParenthesis
                   || fScanner.CurrentToken.Id == TokenId.OpenCurlyBrace  
                   || fScanner.CurrentToken.Id == TokenId.OpenSquareBracket
                   || fScanner.CurrentToken.Id == TokenId.Question 
                   || fScanner.CurrentToken.Id == TokenId.Identifier
                   || fScanner.CurrentToken.Id == TokenId.StringLiteral;
        }

        private bool IsStartOfPattern()
        {
            bool saveIsErrorRecovery = fIsErrorRecovery;
            fIsErrorRecovery = true;
            TextRange savePreviousTokenRange = fPreviousTokenRange;
            fScanner.SaveState();
            var isIdentifierPresent = false;
            var isEqualPresent = false;
            if (fScanner.CurrentToken.Id == TokenId.HashSign)
                NextToken();
            if (fScanner.CurrentToken.Id == TokenId.Identifier)
            {
                ParseMultipartIdentifier(false);
                isIdentifierPresent = true;
            }
            if (isIdentifierPresent && fScanner.CurrentToken.Id == TokenId.OpenParenthesis)
                ParseFields();
            if (fScanner.CurrentToken.Id == TokenId.Equal)
                isEqualPresent = true;
            bool result = isIdentifierPresent && isEqualPresent;
            fIsErrorRecovery = saveIsErrorRecovery;
            fPreviousTokenRange = savePreviousTokenRange;
            fScanner.RestoreState();
            return result;
        }

        private Syntax ParsePrimaryExpression()
        {
            Syntax result = null;
            bool isParsed;
            do
            {
                isParsed = true;
                switch (fScanner.CurrentToken.Id)
                {
                    case TokenId.OpenParenthesis:
                        result = ParseParenthesizedExpression();
                        break;
                    case TokenId.OpenCurlyBrace:
                        result = ParseVariation();
                        break;
                    case TokenId.OpenSquareBracket:
                        result = ParseSpan();
                        break;
                    case TokenId.Question:
                        int startPosition = fScanner.CurrentToken.TextSlice.Position;
                        NextToken();
                        Syntax body = ParsePrimaryExpression();
                        result = SetTextRange(Syntax.Optionality(body), startPosition);
                        break;
                    case TokenId.Identifier:
                        if (IsStartOfPattern())
                        {
                            AddError(CreateError(TextResource.ExpressionExpected));
                            fIsAbortingDueToPatternDefinition = true;
                        }
                        else
                            result = ParseExtractionOrReference();
                        break;
                    case TokenId.StringLiteral:
                        result = ParseText();
                        break;
                    // Error recovery case
                    case TokenId.HashSign:
                        AddError(CreateError(TextResource.ExpressionExpected));
                        if (IsStartOfPattern())
                            fIsAbortingDueToPatternDefinition = true;
                        else
                        {
                            isParsed = false;
                            NextToken();
                        }
                        break;
                    default:
                        AddError(CreateError(TextResource.ExpressionExpected));
                        if (!IsEndSign())
                        {
                            isParsed = false;
                            NextToken();
                        }
                        break;
                }
            } while (!isParsed);
            return result;
        }

        private Syntax ParseParenthesizedExpression()
        {
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenParenthesis, TextResource.OpenParenthesisExpected));
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfParenthesizedExpression;
            NestingContext saveIsInVariationContext = fNestingContext;
            fNestingContext = NestingContext.Parenthesis;
            Syntax result = ParseInsideOrOutsideOrHaving();
            ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
            fEndSign = saveEndSign;
            fNestingContext = saveIsInVariationContext;
            return result;
        }

        private List<T> ParseCommaSeparatedList<T>(Func<T> parseElement, TokenId endToken, Func<bool> isStartOfElement)
        {
            var elements = new List<T>();
            var isListParsed = false;
            do
            {
                T element = parseElement();
                if (element != null)
                    elements.Add(element);
                if (fScanner.CurrentToken.Id == TokenId.Comma)
                    NextToken();
                else if (fScanner.CurrentToken.Id == endToken || IsEndSign() || fIsAbortingDueToPatternDefinition)
                    isListParsed = true;
                else
                {
                    AddError(GetCommaOrEndOfListExpectedError(endToken));
                    while (fScanner.CurrentToken.Id != TokenId.Comma && !IsEndSign() && !isStartOfElement())
                    {
                        AddError(CreateError(TextResource.UnexpectedToken));
                        NextToken();
                    }
                    if (fScanner.CurrentToken.Id == TokenId.Comma)
                        NextToken();
                    else if (IsEndSign())
                        isListParsed = true;
                }
            }
            while (!isListParsed);
            return elements;
        }

        private Error GetCommaOrEndOfListExpectedError(TokenId endToken)
        {
            Error result;
            switch (endToken)
            {
                case TokenId.CloseCurlyBrace:
                    result = CreateError(TextResource.CloseCurlyBraceOrCommaExpected);
                    break;
                case TokenId.CloseParenthesis:
                    result = CreateError(TextResource.CloseParenthesisOrCommaExpected);
                    break;
                case TokenId.CloseSquareBracket:
                    result = CreateError(TextResource.CloseSquareBracketOrCommaExpected);
                    break;
                default:
                    result = CreateError(TextResource.CommaExpected);
                    break;
            };
            return result;
        }

        private VariationSyntax ParseVariation()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected));
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfVariation;
            NestingContext saveIsInVariationContext = fNestingContext;
            fNestingContext = NestingContext.Variation;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            List<Syntax> elements = ParseCommaSeparatedList(ParseVariationElement, TokenId.CloseCurlyBrace, IsStartOfVariationElement);
            ValidateTokenAndAdvance(TokenId.CloseCurlyBrace, TextResource.CloseCurlyBraceExpected);
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            VariationSyntax result = SetTextRange(Syntax.Variation(elements), startPosition);
            fEndSign = saveEndSign;
            fNestingContext = saveIsInVariationContext;
            return result;
        }

        private Syntax ParseVariationElement()
        {
            Syntax result = fScanner.CurrentToken.Id == TokenId.Tilde ? ParseException() : ParseInsideOrOutsideOrHaving();
            return result;
        }

        private bool IsStartOfVariationElement() => fScanner.CurrentToken.Id == TokenId.Tilde || IsStartOfPrimaryExpression();

        private ExceptionSyntax ParseException()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.Tilde, TextResource.TildeSignExpected));
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Syntax body = ParseInsideOrOutsideOrHaving();
            ExceptionSyntax result = Syntax.Exception(body);
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            return SetTextRange(result, startPosition);
        }

        private SpanSyntax ParseSpan()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenSquareBracket, TextResource.OpenSquareBracketExpected));
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfSpan;
            NestingContext saveIsInVariationContext = fNestingContext;
            fNestingContext = NestingContext.Span;
            List<Syntax> elements = ParseCommaSeparatedList(ParseSpanElement, TokenId.CloseSquareBracket, IsStartOfSpanElement);
            ValidateTokenAndAdvance(TokenId.CloseSquareBracket, TextResource.CloseSquareBracketExpected);
            SpanSyntax result = SetTextRange(Syntax.Span(elements), startPosition);
            fEndSign = saveEndSign;
            fNestingContext = saveIsInVariationContext;
            return result;
        }

        private Syntax ParseSpanElement()
        {
            Syntax result = fScanner.CurrentToken.Id == TokenId.Tilde ? (Syntax)ParseException() : ParseRepetition();
            return result;
        }

        private bool IsStartOfSpanElement()
        {
            return fScanner.CurrentToken.Id == TokenId.Tilde
                   || fScanner.CurrentToken.Id == TokenId.Question
                   || fScanner.CurrentToken.Id == TokenId.IntegerLiteral
                   || IsStartOfPrimaryExpression(); // Covered by error recovery
        }

        private RepetitionSyntax ParseRepetition()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Range repetitionRange = ParseNumericRange();
            bool isNumericRangeParsed = startPosition != fScanner.CurrentToken.TextSlice.Position;
            Syntax body = ParseInsideOrOutsideOrHaving();
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            RepetitionSyntax result;
            if (!isNumericRangeParsed && body == null)
                result = null;
            else
            {
                result = Syntax.Repetition(repetitionRange.LowBound, repetitionRange.HighBound, body);
                result = SetTextRange(result, startPosition);
            }
            return result;
        }

        private Range ParseNumericRange()
        {
            Range result;
            switch (fScanner.CurrentToken.Id)
            {
                case TokenId.Question:
                    NextToken();
                    result = new Range(0, 1);
                    break;
                case TokenId.IntegerLiteral:
                    TextRange lowBoundTextRange = fScanner.CurrentToken.GetTextRange();
                    result = new Range();
                    result.LowBound = ParseNumericRangeBound();
                    result.HighBound = result.LowBound;
                    switch (fScanner.CurrentToken.Id)
                    {
                        case TokenId.Plus:
                            NextToken();
                            result.HighBound = Range.Max;
                            break;
                        case TokenId.Minus:
                            NextToken();
                            if (fScanner.CurrentToken.Id == TokenId.IntegerLiteral)
                                result.HighBound = ParseNumericRangeBound();
                            else
                                AddError(CreateError(TextResource.HighBoundOfNumericRangeExpected));
                            if (result.HighBound != -1 && result.LowBound > result.HighBound)
                                AddError(CreateError(lowBoundTextRange, TextResource.NumericRangeLowBoundCannotBeGreaterThanHighBound));
                            break;
                    }
                    break;
                default:
                    AddError(CreateError(TextResource.NumericRangeExpected));
                    result = new Range(0, 0);
                    break;
            }
            return result;
        }

        private int ParseNumericRangeBound()
        {
            ThrowIfNotValidated(ValidateToken(TokenId.IntegerLiteral, TextResource.IntegerLiteralExpected));
            if (int.TryParse(fScanner.CurrentToken.TextSlice.ToString(), out int result))
            {
                if (result >= 0 && result != int.MaxValue)
                    NextToken();
                else
                    AddError(CreateError(TextResource.InvalidValueOfNumericRangeBound, result));
            }
            else
            {
                AddError(CreateError(TextResource.StringLiteralCannotBeConvertedToIntegerValue));
                result = -1;
            }
            return result;
        }

        private Syntax ParseExtractionOrReference()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            Syntax result;
            int nameStart = fScanner.CurrentToken.TextSlice.Position;
            string name = ParseMultipartIdentifier(canEndWithWildcard: false);
            int nameEnd = fPreviousTokenRange.End;
            var nameRange = new TextRange(nameStart, nameEnd);
            if (fScanner.CurrentToken.Id == TokenId.Colon)
            {
                NextToken();
                if (fFieldByName.TryGetValue(name, out FieldSyntax field))
                {
                    if (fExtractedFields.Add(field))
                        fAccessibleFields.Add(field);
                    else
                        AddError(CreateError(nameRange, TextResource.FieldAlreadyUsedForTextExtraction, field.Name));
                }
                else
                {
                    field = Syntax.Field(name);
                    AddError(CreateError(nameRange, TextResource.UndeclaredField, name));
                }
                Syntax body = ParsePrimaryExpression();
                result = Syntax.Extraction(field, body);
            }
            else if (fFieldByName.TryGetValue(name, out FieldSyntax referencedField))
            {
                result = Syntax.FieldReference(referencedField);
                if (!fAccessibleFields.Contains(referencedField))
                    AddError(CreateError(nameRange, TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse, referencedField.Name));
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
                result = null;
                if (fScanner.CurrentToken.Id == TokenId.OpenParenthesis)
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
                                ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
                                break;
                        }
                    }
                    if (result == null)
                    {
                        bool saveIsErrorRecovery = fIsErrorRecovery;
                        fIsErrorRecovery = true;
                        int errorStart = fScanner.CurrentToken.TextSlice.Position;
                        ParseTextAttributes(allowWordClass: false);
                        int errorEnd = fPreviousTokenRange.End;
                        fIsErrorRecovery = saveIsErrorRecovery;
                        var errorRange = new TextRange(errorStart, errorEnd);
                        AddError(CreateError(errorRange, TextResource.AttributesAreNotAllowedForStandardPattern, patternName));
                    }
                }
                if (result == null)
                {
                    if (pattern.Body is TokenSyntax token)
                        result = new TokenSyntax(token.TokenKind, token.Text, token.IsCaseSensitive, token.TextIsPrefix, token.TokenAttributes);
                    else if (pattern.Body is VariationSyntax variation)
                        result = new VariationSyntax(variation.Elements, checkCanReduce: false);
                    else
                        throw InternalError(TextResource.InternalCompilerError);
                }
            }
            else
            {
                List<Syntax> extractionFromFields;
                if (fScanner.CurrentToken.Id == TokenId.OpenParenthesis)
                {
                    NextToken();
                    if (fScanner.CurrentToken.Id == TokenId.CloseParenthesis)
                        extractionFromFields = new List<Syntax>();
                    else
                    {
                        EndSign saveEndSign = fEndSign;
                        fEndSign |= EndSign.EndOfExtractionFromFields;
                        extractionFromFields = ParseCommaSeparatedList(ParseExtractionFromField,
                            TokenId.CloseParenthesis,
                            isStartOfElement: () => fScanner.CurrentToken.Id == TokenId.Identifier);
                        fEndSign = saveEndSign;
                    }
                    ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
                }
                else
                    extractionFromFields = new List<Syntax>();
                result = Syntax.PatternReference(patternName, extractionFromFields);
            }
            return result;
        }

        private Syntax ParseExtractionFromField()
        {
            if (fScanner.CurrentToken.Id != TokenId.Identifier)
            {
                AddError(CreateError(TextResource.FieldNameExpected));
                return null;
            }
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            string fieldName = fScanner.CurrentToken.TextSlice.ToString();
            if (fFieldByName.TryGetValue(fieldName, out FieldSyntax field))
            {
                if (!fExtractedFields.Contains(field))
                {
                    fExtractedFields.Add(field);
                    fAccessibleFields.Add(field);
                }
                else
                    AddError(CreateError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name));
            }
            else
            {
                AddError(CreateError(TextResource.UndeclaredField, fieldName));
                field = Syntax.Field(fieldName);
            }
            NextToken();
            ValidateTokenAndAdvance(TokenId.Colon, TextResource.ColonExpected);
            string fromFieldName;
            if (fScanner.CurrentToken.Id == TokenId.Identifier)
            {
                fromFieldName = fScanner.CurrentToken.TextSlice.ToString();
                NextToken();
            }
            else
            {
                AddError(CreateError(TextResource.FromFieldNameExpected));
                fromFieldName = null;
            }
            Syntax result = SetTextRange(Syntax.ExtractionFromField(field, fromFieldName), startPosition);
            return result;
        }

        private TextSyntax ParseText()
        {
            int startPosition = fScanner.CurrentToken.TextSlice.Position;
            TextSyntax result;
            string text = ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix);
            if (fScanner.CurrentToken.Id == TokenId.OpenParenthesis)
            {
                int attributesStart = fScanner.CurrentToken.TextSlice.Position;
                // If text is not prefix, set fIsErrorRecovery to true to avoid adding possible errors
                bool saveIsErrorRecovery = fIsErrorRecovery;
                fIsErrorRecovery = fIsErrorRecovery || !textIsPrefix;
                WordAttributes attributes = ParseTextAttributes(allowWordClass: true);
                fIsErrorRecovery = saveIsErrorRecovery;
                if (!textIsPrefix)
                {
                    int attributesEnd = fPreviousTokenRange.End;
                    var errorRange = new TextRange(attributesStart, attributesEnd);
                    AddError(CreateError(errorRange, TextResource.TextAttributesAreAllowedOnlyForTextPrefixLiterals));
                }
                if (!string.IsNullOrEmpty(text))
                    result = Syntax.Text(text, isCaseSensitive, attributes);
                else
                    result = Syntax.EmptyText(isCaseSensitive, attributes);
            }
            else
            {
                if (!string.IsNullOrEmpty(text))
                    result = Syntax.Text(text, isCaseSensitive, textIsPrefix);
                else
                    result = Syntax.EmptyText(isCaseSensitive, textIsPrefix);
            }
            SetTextRange(result, startPosition);
            return result;
        }

        private string ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix)
        {
            ThrowIfNotValidated(ValidateToken(TokenId.StringLiteral, TextResource.StringLiteralExpected));
            char quote = fScanner.CurrentToken.TextSlice[0];
            isCaseSensitive = false;
            textIsPrefix = false;
            int cutNum = 0;
            if (fScanner.CurrentToken.TextSlice[fScanner.CurrentToken.TextSlice.Length - 1] == '*')
            {
                textIsPrefix = true;
                cutNum++;
            }
            if (fScanner.CurrentToken.TextSlice[fScanner.CurrentToken.TextSlice.Length - 1 - cutNum] == '!')
            {
                isCaseSensitive = true;
                cutNum++;
            }
            string text = fScanner.CurrentToken.TextSlice.SubSlice(1, fScanner.CurrentToken.TextSlice.Length - 2 - cutNum).ToString();
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
            }
            else
                AddError(CreateError(TextResource.NonEmptyStringLiteralExpected));
            NextToken();
            return text;
        }

        private WordAttributes ParseTextAttributes(bool allowWordClass)
        {
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenParenthesis, TextResource.OpenParenthesisExpected));
            WordAttributes result = null;
            WordClass wordClass = WordClass.Any;
            Range lengthRange = Range.ZeroPlus();
            CharCase charCase = CharCase.Undefined;
            if (fScanner.CurrentToken.Id != TokenId.CloseParenthesis)
            {
                if (fScanner.CurrentToken.Id == TokenId.Identifier)
                {
                    string value = fScanner.CurrentToken.TextSlice.ToString();
                    if (IsWordClass(value, out wordClass))
                    {
                        if (!allowWordClass)
                            AddError(CreateError(TextResource.WordClassAttributeIsAllowedOnlyForTextPrefixLiterals));
                        NextToken();
                    }
                    else if (!(IsCharCase(fScanner.CurrentToken.TextSlice.ToString(), out _) ||
                               IsStartOfPattern()))
                    {
                        AddError(CreateError(TextResource.UnknownAttribute));
                        NextToken();
                    }
                    if (fScanner.CurrentToken.Id == TokenId.Comma)
                        NextToken();
                }
                if (fScanner.CurrentToken.Id == TokenId.IntegerLiteral)
                {
                    lengthRange = ParseNumericRange();
                    if (fScanner.CurrentToken.Id == TokenId.Comma)
                        NextToken();
                }
                if (fScanner.CurrentToken.Id == TokenId.Identifier)
                {
                    string value = fScanner.CurrentToken.TextSlice.ToString();
                    if (IsCharCase(value, out charCase))
                        NextToken();
                    else if (IsWordClass(value, out _))
                        AddError(CreateError(TextResource.AttributeIsInWrongPlace, value));
                    else
                        AddError(CreateError(TextResource.UnknownAttribute, value));
                }
                if (fScanner.CurrentToken.Id != TokenId.CloseParenthesis)
                {
                    while (fScanner.CurrentToken.Id != TokenId.CloseParenthesis && !IsEndSign() && !IsStartOfPattern())
                    {
                        if (fScanner.CurrentToken.Id == TokenId.Identifier)
                        {
                            string value = fScanner.CurrentToken.TextSlice.ToString();
                            if (IsWordClass(value, out _) || IsCharCase(value, out _))
                                AddError(CreateError(TextResource.AttributeIsInWrongPlace, value));
                            else
                                AddError(CreateError(TextResource.UnknownAttribute, value));
                            NextToken();
                        }
                        else if (fScanner.CurrentToken.Id == TokenId.IntegerLiteral)
                        {
                            bool saveIsErrorRecovery = fIsErrorRecovery;
                            fIsErrorRecovery = true;
                            int errorStart = fScanner.CurrentToken.TextSlice.Position;
                            ParseNumericRange();
                            int errorEnd = fPreviousTokenRange.End;
                            fIsErrorRecovery = saveIsErrorRecovery;
                            var errorRange = new TextRange(errorStart, errorEnd);
                            AddError(CreateError(errorRange, TextResource.NumericRangeIsInWrongPlace));
                        }
                        else
                        {
                            AddError(CreateError(TextResource.CloseParenthesisExpected));
                            NextToken();
                        }
                    }
                }
                ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
                if (wordClass != WordClass.Any || !lengthRange.IsZeroPlus() || charCase != CharCase.Undefined)
                    result = new WordAttributes(wordClass, lengthRange, charCase);
            }
            else
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
            fPreviousTokenRange = fScanner.CurrentToken.GetTextRange();
            do
            {
                fScanner.NextTokenOrComment();
                switch (fScanner.CurrentToken.Id)
                {
                    case TokenId.Unknown:
                        AddError(CreateError(TextResource.InvalidCharacter, fScanner.CurrentToken.TextSlice.ToString()));
                        break;
                    case TokenId.UnknownKeyword:
                        AddError(CreateError(TextResource.UnknownKeyword, fScanner.CurrentToken.TextSlice));
                        break;
                    case TokenId.UnterminatedComment:
                        AddError(CreateError(fScanner.CurrentToken.TextSlice.End, TextResource.UnterminatedComment));
                        break;
                    case TokenId.UnterminatedStringLiteral:
                        AddError(CreateError(fScanner.CurrentToken.TextSlice.End, TextResource.UnterminatedStringLiteral));
                        break;
                }
            } while (fScanner.CurrentToken.Id == TokenId.Comment || fScanner.CurrentToken.Id == TokenId.UnterminatedComment);
        }

        private bool ValidateToken(TokenId id, string error, bool shouldAdvance = false)
        {
            bool result;
            if (fScanner.CurrentToken.Id == id)
            {
                if (shouldAdvance)
                    NextToken();
                result = true;
            }
            else
            {
                AddError(CreateError(error, args: fScanner.CurrentToken));
                result = false;
            }
            return result;
        }

        private bool ValidateTokenAndAdvance(TokenId id, string error) => ValidateToken(id, error, true);

        private T SetTextRange<T>(T syntax, int start) where T : Syntax
        {
            int end = fScanner.CurrentToken.TextSlice.Position;
            syntax.TextRange = new TextRange(start, end);
            return syntax;
        }

        // Used to determine if current erroneous token can be parsed by higher level parsing method or can be
        // safely skipped to continue parsing.
        private bool IsEndSign()
        {
            if (fScanner.CurrentToken.Id == TokenId.End
                || fScanner.CurrentToken.Id == TokenId.SearchKeyword
                || fScanner.CurrentToken.Id == TokenId.PatternKeyword
                || fScanner.CurrentToken.Id == TokenId.NamespaceKeyword
                || fScanner.CurrentToken.Id == TokenId.RequireKeyword)
                return true;
            if (fScanner.CurrentToken.Id == TokenId.Unknown
                || fScanner.CurrentToken.Id == TokenId.UnknownKeyword
                || fScanner.CurrentToken.Id == TokenId.Comment
                || fScanner.CurrentToken.Id == TokenId.UnterminatedComment
                || fScanner.CurrentToken.Id == TokenId.UnterminatedStringLiteral)
                return false;
            // Comma should be treated as end sign only if current nesting context is either variation or span. Otherwise it can be skipped.
            if ((fNestingContext == NestingContext.Variation || fNestingContext == NestingContext.Span) && fScanner.CurrentToken.Id == TokenId.Comma)
                return true;
            var mask = 1;
            var result = false;
            while (mask <= MaxEndSign && !result)
            {
                switch (fEndSign & (EndSign)mask)
                {
                    case EndSign.EndOfNamespaceBody:
                    case EndSign.EndOfNestedPatterns:
                    case EndSign.EndOfVariation:
                        result = fScanner.CurrentToken.Id == TokenId.CloseCurlyBrace;
                        break;
                    case EndSign.EndOfPattern:
                        result = fScanner.CurrentToken.Id == TokenId.Semicolon && fNestingContext == NestingContext.None;
                        break;
                    case EndSign.EndOfSpan:
                        result = fScanner.CurrentToken.Id == TokenId.CloseSquareBracket;
                        break;
                    case EndSign.EndOfFields:
                        result = fScanner.CurrentToken.Id == TokenId.CloseParenthesis || fScanner.CurrentToken.Id == TokenId.Equal;
                        break;
                    case EndSign.EndOfExtractionFromFields:
                        result = fScanner.CurrentToken.Id == TokenId.CloseParenthesis;
                        break;
                    case EndSign.EndOfParenthesizedExpression:
                        result = fScanner.CurrentToken.Id == TokenId.CloseParenthesis;
                        break;
                    case EndSign.StartOfNestedPatterns:
                        result = fScanner.CurrentToken.Id == TokenId.WhereKeyword;
                        break;
                }
                mask <<= 1;
            }
            return result;
        }

        private void AddErrorWithoutCheck(in Error error) => AddError(error, checkForMultipleErrors: false);

        private void AddError(in Error error, bool checkForMultipleErrors = true)
        {
            if (!fIsErrorRecovery
                && (!checkForMultipleErrors || fErrors.Count == 0 || error.ErrorRange.Start > fErrors[fErrors.Count - 1].ErrorRange.Start)
                && error.ErrorRange.Start < fTextLength)
                fErrors.Add(error);
        }

        private Error CreateError(string format) => 
            CreateError(fScanner.CurrentToken.Id == TokenId.End ? fPreviousTokenRange : fScanner.CurrentToken.GetTextRange(), format, args: fScanner.CurrentToken);
        
        private Error CreateError(string format, params object[] args) => 
            CreateError(fScanner.CurrentToken.Id == TokenId.End ? fPreviousTokenRange : fScanner.CurrentToken.GetTextRange(), format, args);

        private Error CreateError(int position, string format)
        {
            var errorRange = new TextRange(position, position + 1);
            return CreateError(errorRange, format);
        }

        private Error CreateErrorAfterRange(in TextRange range, string format)
        {
            TextRange errorRange;
            if (range.End < fTextLength)
                errorRange = new TextRange(range.End, range.End + 1);
            else
                errorRange = new TextRange(fTextLength - 1, fTextLength);
            return CreateError(errorRange, format);
        }

        private Error CreateError(in TextRange range, string format, params object[] args)
        {
            var result = new Error(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), range);
            return result;
        }

        private void ThrowIfNotValidated(bool isValidated)
        {
            if (!isValidated)
                throw InternalError(string.Format(TextResource.InternalParserErrorFormat, fErrors[fErrors.Count - 1]));
        }

        private Exception InternalError(string message) => new InternalNevodErrorException(message);

        [Flags]
        private enum EndSign
        {
            EndOfNamespaceBody = 1 << 0,
            EndOfPattern = 1 << 1,
            StartOfNestedPatterns = 1 << 2,
            EndOfNestedPatterns = 1 << 3,
            EndOfVariation = 1 << 4,
            EndOfSpan = 1 << 5,
            EndOfParenthesizedExpression = 1 << 6,
            EndOfFields = 1 << 7,
            EndOfExtractionFromFields = 1 << 8
        }

        private const int MaxEndSign = (int)EndSign.EndOfExtractionFromFields;

        private enum NestingContext
        {
            None,
            Variation,
            Span,
            Parenthesis
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

    public readonly struct Error
    {
        public string ErrorMessage { get; }
        public TextRange ErrorRange { get; }

        public Error(string errorMessage, TextRange errorRange)
        {
            ErrorMessage = errorMessage;
            ErrorRange = errorRange;
        }
    }

    internal static partial class TextResource
    {
        public const string InternalParserErrorFormat = "Internal parser error: {0}";
        public const string RequireKeywordExpected = "@require keyword expected, but '{0}' found";
        public const string PatternNameExpected = "Pattern name expected, but '{0}' found";
        public const string ExpressionExpected = "Expression expected, but '{0}' found";
        public const string RequireDefinitionShouldEndWithSemicolon = "@require definition should end with semicolon";
        public const string FilePathAsStringLiteralExpected = "File path as string literal expected, but '{0}' found";
        public const string InvalidSpecifierAfterStringLiteral = "Invalid specifier after string literal";
        public const string UnknownKeyword = "Unknown keyword '{0}'";
        public const string UnterminatedComment = "Unterminated comment";
        public const string UnterminatedStringLiteral = "Unterminated string literal";
        public const string IdentifierExpected = "Identifier expected, but '{0}' found";
        public const string IdentifierOrAsteriskExpected = "Identifier or asterisk expected, but '{0}' found";
        public const string DuplicatedField = "Duplicated field '{0}'";
        public const string SearchTargetDefinitionShouldEndWithSemicolon = "Search target definition should end with semicolon";
        public const string PatternDefinitionExpected = "Pattern definition expected, but '{0}' found";
        public const string PatternShouldEndWithSemicolon = "Pattern should end with semicolon";
        public const string EqualSignExpectedInPatternDefinition = "Equal sign expected in pattern definition, but '{0}' found";
        public const string FieldNameExpected = "Field name expected, but '{0}' found";
        public const string FromFieldNameExpected = "From field name expected, but '{0}' found";
        public const string ListOfFieldNamesExpected = "List of field names expected, but '{0}' found";
        public const string UndeclaredField = "Undeclared field: '{0}'";
        public const string ValueOfFieldShouldBeExtractedFromTextBeforeUse = "Value of field '{0}' should be extracted from text before use";
        public const string FieldAlreadyUsedForTextExtraction = "Field '{0}' already used for text extraction";
        public const string OpenParenthesisExpected = "Open parenthesis expected, but '{0}' found";
        public const string CloseParenthesisExpected = "Close parenthesis expected, but '{0}' found";
        public const string CloseParenthesisOrCommaExpected = "Close parenthesis or comma expected, but '{0}' found";
        public const string DoublePeriodExpected = "Double period expected, but '{0}' found";
        public const string StringLiteralExpected = "String literal expected, but '{0}' found";
        public const string NonEmptyStringLiteralExpected = "Expected non-empty string literal";
        public const string OpenCurlyBraceExpected = "Open curly brace expected, but '{0}' found";
        public const string CloseCurlyBraceExpected = "Close curly brace expected, but '{0}' found";
        public const string CloseCurlyBraceOrCommaExpected = "Close curly brace or comma expected, but '{0}' found";
        public const string OpenSquareBracketExpected = "Open square bracket expected, but '{0}' found";
        public const string CloseSquareBracketExpected = "Close square bracket expected, but '{0}' found";
        public const string CloseSquareBracketOrCommaExpected = "Close square bracket or comma expected, but '{0}' found";
        public const string CommaExpected = "Comma expected, but '{0}' found";
        public const string OperatorExpected = "Operator expected";
        public const string TildeSignExpected = "Tilde sign expected, but '{0}' found";
        public const string IntegerLiteralExpected = "Integer literal expected, but '{0}' found";
        public const string StringLiteralCannotBeConvertedToIntegerValue = "String literal '{0}' cannot be converted to integer value";
        public const string InvalidValueOfNumericRangeBound = "Invalid value of numeric range bound: '{0}', the value should be greater or equal to 0 and less than maximum 32-bit integer";
        public const string NumericRangeLowBoundCannotBeGreaterThanHighBound = "Numeric range low bound cannot be greater than high bound";
        public const string ColonExpected = "Colon expected, but '{0}' found";
        public const string AttributesAreNotAllowedForStandardPattern = "Attributes are not allowed for standard pattern '{0}'";
        public const string UnknownAttribute = "Unknown attribute: '{0}'";
        public const string RequireKeywordsAreOnlyAllowedInTheBeginning = "Require keywords are only allowed in the beginning of the file";
        public const string PatternBodyExpected = "Pattern body expected";
        public const string NumericRangeExpected = "Numeric range expected, but '{0}' found";
        public const string TildeExpected = "Tilde expected, but '{0}' found";
        public const string TextAttributesAreAllowedOnlyForTextPrefixLiterals = "Text attributes are allowed only for text prefix literals";
        public const string WordClassAttributeIsAllowedOnlyForTextPrefixLiterals = "Word class attribute is allowed only for text prefix literals";
        public const string AttributeIsInWrongPlace = "Attribute '{0}' is in wrong place. The correct order is: word class, numeric range, char case";
        public const string NumericRangeIsInWrongPlace = "Numeric range is in wrong place. The correct order is: word class, numeric range, char case";
        public const string HighBoundOfNumericRangeExpected = "High bound of numeric range expected, but '{0}' found";
        public const string UnexpectedToken = "Unexpected token: '{0}'";
        public const string InvalidCharacter = "Invalid character: '{0}'";
        public const string NamespacesAreNotAllowedInNestedPatterns = "Namespaces are not allowed in nested patterns";
        public const string PatternDefinitionsAreNotAllowedInExpressionMode = "Pattern definitions are not allowed in expression mode";
        public const string NamespacesAreNotAllowedInExpressionMode = "Namespaces are not allowed in expression mode";
        public const string RequireKeywordsAreNotAllowedInExpressionMode = "Require keywords are not allowed in expression mode";
        public const string EndOfExpressionExpectedInExpressionMode = "End of expression expected, but '{0}' found. Only one expression is allowed in expression mode";
    }
}
