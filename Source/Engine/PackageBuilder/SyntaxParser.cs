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
        private Slice fText;
        private int fTextPosition;
        private int fLineNumber;
        private int fLinePosition;
        private int fLineLength;
        private char fCharacter;
        private Token fToken;
        private bool fIsTokenSaveMode;
        private TextRange fPreviousTokenRange;
        private Dictionary<string, TokenId> fTokenByKeyword;
        private NameScope fCurrentScope;
        private Stack<NameScope> fScopeStack;
        private List<RequiredPackageSyntax> fRequiredPackages;
        private List<Syntax> fPatterns;
        private List<Syntax> fSearchTargets;
        private Dictionary<string, PatternSyntax> fStandardPatterns;
        private Dictionary<string, FieldSyntax> fFieldByName;
        private HashSet<FieldSyntax> fExtractedFields;
        private HashSet<FieldSyntax> fAccessibleFields;
        private Stack<HashSet<FieldSyntax>> fAccessibleFieldsStack;
        private List<Error> fErrors;
        private EndSign fEndSign;
        private NestingContext fNestingContext;
        private bool fIsAbortingDueToPatternDefinition;

        public static readonly string EmptyNamespace = string.Empty;

        public SyntaxParser()
        {
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
            fErrors = new List<Error>();
            fNestingContext = NestingContext.None;
            fIsAbortingDueToPatternDefinition = false;
            NextCharacter();
            NextKnownTokenOrComment();
            PackageSyntax result = ParsePackage();
            result.Errors = fErrors;
            return result;
        }
        
        public PackageSyntax ParseExpressionText(string text)
        {
            fText = text.Slice();
            fTextPosition = -1;
            fErrors = new List<Error>();
            fNestingContext = NestingContext.None;
            fIsAbortingDueToPatternDefinition = false;
            NextCharacter();
            NextToken();
            int startPosition = fToken.TextSlice.Position;
            Syntax patternBody = ParseInsideOrOutsideOrHaving();
            PatternSyntax pattern = SetTextRange(Syntax.Pattern(isSearchTarget: true, "Pattern", patternBody, null), startPosition);
            PackageSyntax result = SetTextRange(Syntax.Package(pattern), startPosition);
            result.Errors = fErrors;
            return result;
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
            int startPosition = fToken.TextSlice.Position;
            // 1. Metadata
            ParseMetadata();
            // 2. Required packages
            ParseRequires();
            // 3. Pattern definitions within namespaces
            while (fToken.Id != TokenId.End)
                ParseNamespacesAndPatterns();
            PackageSyntax result = SetTextRange(Syntax.Package(fRequiredPackages, fSearchTargets, fPatterns), startPosition);
            fCurrentScope = null;
            fScopeStack = null;
            fRequiredPackages = null;
            fPatterns = null;
            fSearchTargets = null;
            return result;
        }

        private void ParseMetadata()
        {
            while (fToken.Id == TokenId.Comment)
            {
                char commentType = fToken.TextSlice[1];
                int cutNum = commentType == '*' ? 2 : 0;
                string comment = fToken.TextSlice.SubSlice(2, fToken.TextSlice.Length - 2 - cutNum).ToString();
                string metadata = comment.Trim();
                ParseMetadataValue(metadata);
                NextKnownTokenOrComment();
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
                if (requiredPackage != null)
                {
                    if (!fRequiredPackages.Any(x => x.RelativePath == requiredPackage.RelativePath))
                        fRequiredPackages.Add(requiredPackage);
                    else
                        AddError(GetError(requiredPackage.TextRange, TextResource.DuplicatedRequiredPackage, requiredPackage.RelativePath));
                }
            }
        }

        private RequiredPackageSyntax ParseRequire()
        {
            int startPosition = fToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.RequireKeyword, TextResource.RequireKeywordExpected));
            RequiredPackageSyntax result = null;
            if (ValidateToken(TokenId.StringLiteral, TextResource.FilePathAsStringLiteralExpected))
            {
                string relativePath = ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix);
                if (isCaseSensitive || textIsPrefix)
                    AddError(GetError(TextResource.InvalidSpecifierAfterStringLiteral));
                result = new RequiredPackageSyntax(relativePath);
            }
            ValidateTokenAndAdvance(TokenId.Semicolon, TextResource.RequireDefinitionShouldEndWithSemicolon);
            if (result != null)
                SetTextRange(result, startPosition);
            return result;
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
                    PatternSyntax pattern1 = ParsePattern(isSearchTarget: false);
                    if (pattern1 != null)
                    {
                        SetTextRange(pattern1, startPosition1);
                        fPatterns.Add(pattern1);
                    }
                    break;
                case TokenId.SearchKeyword:
                    int startPosition2 = fToken.TextSlice.Position;
                    NextToken();
                    if (fToken.Id == TokenId.PatternKeyword)
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
                        SearchTargetSyntax searchTarget = SetTextRange(ParseSearchTarget(), startPosition2);
                        fSearchTargets.Add(searchTarget);
                    }
                    break;
                default:
                    PatternSyntax pattern3 = ParsePattern(isSearchTarget: false);
                    if (pattern3 != null)
                        fPatterns.Add(pattern3);
                    break;
            }
        }

        private SearchTargetSyntax ParseSearchTarget()
        {
            int startPosition = fToken.TextSlice.Position;
            string name = ParseMultipartIdentifier(shouldStartFromIdentifier: true, canEndWithWildcard: true);
            string fullName = Syntax.GetFullName(fCurrentScope.Namespace, name);
            ValidateTokenAndAdvance(TokenId.Semicolon, TextResource.SearchTargetDefinitionShouldEndWithSemicolon);
            SearchTargetSyntax result;
            if (fullName.EndsWith(".*"))
            {
                string ns = fullName.TrimEnd('*', '.');
                result = Syntax.NamespaceSearchTarget(ns);
            }
            else
            {
                PatternReferenceSyntax patternReference = SetTextRange(Syntax.PatternReference(name), startPosition);
                result = Syntax.PatternSearchTarget(fullName, patternReference);
            }
            return SetTextRange(result, startPosition);
        }

        private void ParseNamespace()
        {
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfNamespaceBody;
            string name = ParseMultipartIdentifier(shouldStartFromIdentifier: true, canEndWithWildcard: false);
            ValidateTokenAndAdvance(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected);
            fScopeStack.Push(fCurrentScope);
            string nameSpace = Syntax.GetFullName(fCurrentScope.Namespace, name);
            fCurrentScope = new NameScope(nameSpace, fCurrentScope.MasterPatternName);
            try
            {
                while (fToken.Id != TokenId.End && fToken.Id != TokenId.CloseCurlyBrace)
                {
                    ParseNamespacesAndPatterns();
                }
                ValidateTokenAndAdvance(TokenId.CloseCurlyBrace, TextResource.CloseCurlyBraceExpected);
            }
            finally
            {
                fCurrentScope = fScopeStack.Pop();
            }
            fEndSign = saveEndSign;
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
            {
                AddError(GetError(TextResource.IdentifierExpected));
                return "";
            }
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
                {
                    AddError(GetError(TextResource.IdentifierOrAsteriskExpected));
                    return result.ToString();
                }
                else
                {
                    AddError(GetError(TextResource.IdentifierExpected));
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
            int startPosition = fToken.TextSlice.Position;
            if (fToken.Id == TokenId.HashSign)
            {
                isSearchTarget = true;
                NextToken();
            }
            string name = null;
            if (fToken.Id == TokenId.Identifier)
                name = ParseMultipartIdentifier(shouldStartFromIdentifier: true, canEndWithWildcard: false);
            else
                AddError(GetError("Pattern name expected"));
            fFieldByName.Clear();
            fExtractedFields.Clear();
            fAccessibleFields.Clear();
            fAccessibleFieldsStack.Clear();
            FieldSyntax[] fields = null;
            if (fToken.Id == TokenId.OpenParenthesis)
            {
                fields = ParseFields();
            }
            ValidateTokenAndAdvance(TokenId.Equal, TextResource.EqualSignExpectedInPatternDefinition);
            if (IsEndSign() || IsStartOfPattern())
            {
                AddError(GetError("Pattern body expected"));
                if (fToken.Id == TokenId.Semicolon)
                    NextToken();
            }
            else if (fToken.TextSlice.Position == startPosition && !IsStartOfPrimaryExpression())
            {
                NextToken();
            }
            else
            {
                Syntax body = ParsePatternBody();
                if (body == null)
                    AddError(GetError("Pattern body expected"));
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
                if (fToken.Id == TokenId.Semicolon) 
                    NextToken();
                else
                    AddError(GetErrorAfter(fPreviousTokenRange, TextResource.PatternShouldEndWithSemicolon));
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
            while (fToken.Id != TokenId.End && fToken.Id != TokenId.CloseCurlyBrace)
            {
                // TODO: handle namespace
                switch (fToken.Id)
                {
                    case TokenId.PatternKeyword:
                        int startPosition1 = fToken.TextSlice.Position;
                        NextToken();
                        PatternSyntax pattern1 = ParsePattern(isSearchTarget: false);
                        if (pattern1 != null)
                        {
                            SetTextRange(pattern1, startPosition1);
                            nestedPatterns.Add(pattern1);
                        }
                        break;
                    case TokenId.SearchKeyword:
                        int startPosition2 = fToken.TextSlice.Position;
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
                        AddError(GetError("Namespaces are not allowed in nested patterns"));
                        NextToken();
                        ParseNamespace();
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
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfFields;
            List<FieldSyntax> result = ParseCommaSeparatedList(ParseField, IsFieldStart);
            ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
            fEndSign = saveEndSign;
            return result.ToArray();
        }

        private bool IsFieldStart() => fToken.Id == TokenId.Identifier || fToken.Id == TokenId.Tilde;

        private FieldSyntax ParseField()
        {
            int startPosition = fToken.TextSlice.Position;
            var isInternal = false;
            if (fToken.Id == TokenId.Tilde)
            {
                isInternal = true;
                NextToken();
            }
            FieldSyntax result = null;
            if (fToken.Id == TokenId.Identifier)
            {
                string name = fToken.TextSlice.ToString();
                result = Syntax.Field(name, isInternal);
                if (!fFieldByName.ContainsKey(name))
                {
                    if (!fIsTokenSaveMode)
                        fFieldByName.Add(name, result);
                }
                else
                    AddError(GetError(TextResource.DuplicatedField, name));
                NextToken();
                SetTextRange(result, startPosition);
            }
            else if (isInternal)
                AddError(GetError(fPreviousTokenRange, TextResource.FieldNameExpected));
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
                    result = SetTextRange(result, startPosition);
                }
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
                    if (element != null)
                        elements.Add(element);
                }
                result = SetTextRange(Syntax.Conjunction(elements), startPosition);
            }
            return result;
        }

        private Syntax ParseAnySpanOrWordSpan()
        {
            int startPosition = fToken.TextSlice.Position;
            // TODO: Why here?
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
                            bool isNumericRangeRequired = false;
                            if (fToken.Id == TokenId.Identifier)
                            {
                                extractionOfSpan = ParseSpanExtraction();
                                if (fToken.Id == TokenId.Colon)
                                {
                                    NextToken();
                                    ValidateTokenAndAdvance(TokenId.OpenSquareBracket, TextResource.OpenSquareBracketExpected);
                                    isNumericRangeRequired = true;
                                }
                            }
                            if (fToken.Id == TokenId.OpenSquareBracket || isNumericRangeRequired)
                            {
                                isWordSpan = true;
                                if (fToken.Id == TokenId.OpenSquareBracket)
                                    NextToken();
                                spanRange = ParseNumericRange();
                                ValidateTokenAndAdvance(TokenId.CloseSquareBracket, TextResource.CloseSquareBracketExpected);
                                if (fToken.Id == TokenId.Tilde || IsStartOfPrimaryExpression())
                                {
                                    ValidateTokenAndAdvance(TokenId.Tilde, "Tilde expected, but '{0}' found");
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
            string fieldName = fToken.TextSlice.ToString();
            int startPosition = fToken.TextSlice.Position;
            Syntax result;
            if (fFieldByName.TryGetValue(fieldName, out FieldSyntax field))
            {
                result = SetTextRange(Syntax.Extraction(field), startPosition);
                if (fExtractedFields.Add(field))
                    fAccessibleFields.Add(field);
                else
                    AddError(GetError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name));
            }
            else
            {
                AddError(GetError("Undeclared field: '{0}'", fieldName));
                result = SetTextRange(Syntax.Extraction(Syntax.Field(fieldName)), startPosition);
            }
            NextToken();
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
                    if (element != null)
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
            if (fToken.Id == TokenId.Plus || !fIsAbortingDueToPatternDefinition && fNestingContext != NestingContext.Variation && IsStartOfPrimaryExpression())
            {
                var elements = new List<Syntax> { result };
                while (fToken.Id == TokenId.Plus || fNestingContext != NestingContext.Variation && IsStartOfPrimaryExpression() && !IsStartOfPattern())
                {
                    if (fToken.Id == TokenId.Plus)
                        NextToken();
                    else
                        AddError(GetErrorAfter(fPreviousTokenRange, "Operator expected"));
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
            return fToken.Id == TokenId.OpenParenthesis
                   || fToken.Id == TokenId.OpenCurlyBrace  
                   || fToken.Id == TokenId.OpenSquareBracket
                   || fToken.Id == TokenId.Question 
                   || fToken.Id == TokenId.Identifier
                   || fToken.Id == TokenId.StringLiteral;
        }

        private bool IsStartOfPattern()
        {
            fIsTokenSaveMode = true;
            TextRange savePreviousTokenRange = fPreviousTokenRange;
            int saveTextPosition = fTextPosition;
            char saveCharacter = fCharacter;
            Token saveToken = fToken;
            var isIdentifierPresent = false;
            var isEqualPresent = false;
            if (fToken.Id == TokenId.HashSign)
                NextToken();
            if (fToken.Id == TokenId.Identifier)
            {
                ParseMultipartIdentifier(true, false);
                isIdentifierPresent = true;
            }
            if (fToken.Id == TokenId.OpenParenthesis)
            {
                ParseFields();
            }
            if (fToken.Id == TokenId.Equal)
                isEqualPresent = true;
            bool result = isIdentifierPresent && isEqualPresent;
            fIsTokenSaveMode = false;
            fPreviousTokenRange = savePreviousTokenRange;
            fTextPosition = saveTextPosition;
            fCharacter = saveCharacter;
            fToken = saveToken;
            return result;
        }

        private Syntax ParsePrimaryExpression()
        {
            Syntax result = null;
            bool isParsed;
            do
            {
                isParsed = true;
                switch (fToken.Id)
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
                        int startPosition = fToken.TextSlice.Position;
                        NextToken();
                        Syntax body = ParsePrimaryExpression();
                        result = SetTextRange(Syntax.Optionality(body), startPosition);
                        break;
                    case TokenId.Identifier:
                        if (IsStartOfPattern())
                        {
                            AddError(GetError("Expression expected, but '{0}' found"));
                            fIsAbortingDueToPatternDefinition = true;
                        }
                        else
                            result = ParseExtractionOrReference();
                        break;
                    case TokenId.StringLiteral:
                        result = ParseText();
                        break;
                    default:
                        AddError(GetError("Expression expected, but '{0}' found"));
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
            fEndSign |= EndSign.CloseParenthesis;
            NestingContext saveIsInVariationContext = fNestingContext;
            fNestingContext = NestingContext.Parenthesis;
            Syntax result = ParseInsideOrOutsideOrHaving();
            ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisOrOperatorExpected);
            fEndSign = saveEndSign;
            fNestingContext = saveIsInVariationContext;
            return result;
        }

        private List<T> ParseCommaSeparatedList<T>(Func<T> parseElement, Func<bool> isStartOfElement)
        {
            var elements = new List<T>();
            var isListParsed = false;
            do
            {
                T element = parseElement();
                if (element != null)
                    elements.Add(element);
                if (fToken.Id == TokenId.Comma)
                    NextToken();
                else if (IsEndSign() || fIsAbortingDueToPatternDefinition)
                    isListParsed = true;
                else
                {
                    // TODO: think about error not added after previous because of same start positions
                    AddError(GetErrorAfter(fPreviousTokenRange, "Comma expected"));
                    while (fToken.Id != TokenId.Comma && !IsEndSign() && !isStartOfElement())
                    {
                        AddError(GetError("Unexpected token: '{0}'"));
                        NextToken();
                    }
                    if (fToken.Id == TokenId.Comma)
                        NextToken();
                    else if (IsEndSign())
                        isListParsed = true;
                }
            }
            while (!isListParsed);
            return elements;
        }

        private VariationSyntax ParseVariation()
        {
            int startPosition = fToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenCurlyBrace, TextResource.OpenCurlyBraceExpected));
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfVariation;
            NestingContext saveIsInVariationContext = fNestingContext;
            fNestingContext = NestingContext.Variation;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            List<Syntax> elements = ParseCommaSeparatedList(ParseVariationElement, IsStartOfVariationElement);
            ValidateTokenAndAdvance(TokenId.CloseCurlyBrace, TextResource.CloseCurlyBraceOrCommaExpected);
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            VariationSyntax result = SetTextRange(Syntax.Variation(elements), startPosition);
            fEndSign = saveEndSign;
            fNestingContext = saveIsInVariationContext;
            return result;
        }

        private Syntax ParseVariationElement()
        {
            Syntax result = fToken.Id == TokenId.Tilde ? ParseException() : ParseInsideOrOutsideOrHaving();
            return result;
        }

        private bool IsStartOfVariationElement() => fToken.Id == TokenId.Tilde || IsStartOfPrimaryExpression();

        private ExceptionSyntax ParseException()
        {
            int startPosition = fToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.Tilde, TextResource.TildeSignExpected));
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Syntax body = ParseInsideOrOutsideOrHaving();
            ExceptionSyntax result = Syntax.Exception(body);
            fAccessibleFields = fAccessibleFieldsStack.Pop();
            return SetTextRange(result, startPosition);
        }

        private SpanSyntax ParseSpan()
        {
            int startPosition = fToken.TextSlice.Position;
            ThrowIfNotValidated(ValidateTokenAndAdvance(TokenId.OpenSquareBracket, TextResource.OpenSquareBracketExpected));
            EndSign saveEndSign = fEndSign;
            fEndSign |= EndSign.EndOfSpan;
            NestingContext saveIsInVariationContext = fNestingContext;
            fNestingContext = NestingContext.Span;
            List<Syntax> elements = ParseCommaSeparatedList(ParseSpanElement, IsStartOfSpanElement);
            ValidateTokenAndAdvance(TokenId.CloseSquareBracket, TextResource.CloseSquareBracketExpected);
            SpanSyntax result = SetTextRange(Syntax.Span(elements), startPosition);
            fEndSign = saveEndSign;
            fNestingContext = saveIsInVariationContext;
            return result;
        }

        private Syntax ParseSpanElement()
        {
            Syntax result = fToken.Id == TokenId.Tilde ? (Syntax)ParseException() : ParseRepetition();
            return result;
        }

        private bool IsStartOfSpanElement()
        {
            return fToken.Id == TokenId.Tilde 
                   || fToken.Id == TokenId.IntegerLiteral 
                   || IsStartOfPrimaryExpression(); // Covered by error recovery
        }

        private RepetitionSyntax ParseRepetition()
        {
            int startPosition = fToken.TextSlice.Position;
            fAccessibleFieldsStack.Push(new HashSet<FieldSyntax>(fAccessibleFields));
            Range repetitionRange;
            if (fToken.Id == TokenId.Question || fToken.Id == TokenId.IntegerLiteral)
                repetitionRange = ParseNumericRange();
            else
            {
                AddError(GetErrorAfter(fPreviousTokenRange, "Numeric range expected"));
                repetitionRange = new Range(0, 0);
            }
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
                    TextRange lowBoundTextRange = TokenTextRange(fToken);
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
                            if (fToken.Id == TokenId.IntegerLiteral)
                                result.HighBound = ParseNumericRangeBound();
                            else
                                AddError(GetError("High bound of numeric range expected, but '{0}' found"));
                            if (result.HighBound != -1 && result.LowBound > result.HighBound)
                                AddError(GetError(lowBoundTextRange, TextResource.NumericRangeLowBoundCannotBeGreaterThanHighBound));
                            break;
                    }
                    break;
                default:
                    AddError(GetError("Numeric range expected, but '{0}' found"));
                    result = new Range(0, 0);
                    break;
            }
            return result;
        }

        private int ParseNumericRangeBound()
        {
            ThrowIfNotValidated(ValidateToken(TokenId.IntegerLiteral, TextResource.IntegerLiteralExpected));
            if (int.TryParse(fToken.TextSlice.ToString(), out int result))
            {
                if (result >= 0 && result != int.MaxValue)
                    NextToken();
                else
                    AddError(GetError(TextResource.InvalidValueOfNumericRangeBound, result));
            }
            else
            {
                AddError(GetError(TextResource.StringLiteralCannotBeConvertedToIntegerValue));
                result = -1;
            }
            return result;
        }

        private Syntax ParseExtractionOrReference()
        {
            int startPosition = fToken.TextSlice.Position;
            Syntax result;
            int nameStart = fToken.TextSlice.Position;
            string name = ParseMultipartIdentifier(shouldStartFromIdentifier: false, canEndWithWildcard: false);
            int nameEnd = fPreviousTokenRange.End;
            var nameRange = new TextRange(nameStart, nameEnd);
            if (fToken.Id == TokenId.Colon)
            {
                NextToken();
                if (fFieldByName.TryGetValue(name, out FieldSyntax field))
                {
                    if (fExtractedFields.Add(field))
                        fAccessibleFields.Add(field);
                    else
                        AddError(GetError(nameRange, TextResource.FieldAlreadyUsedForTextExtraction, field.Name));
                }
                else
                {
                    field = Syntax.Field(name);
                    AddError(GetError(nameRange, "Undeclared field: '{0}'", name));
                }
                Syntax body = ParsePrimaryExpression();
                result = Syntax.Extraction(field, body);
            }
            else if (fFieldByName.TryGetValue(name, out FieldSyntax referencedField))
            {
                result = Syntax.FieldReference(referencedField);
                if (!fAccessibleFields.Contains(referencedField))
                    AddError(GetError(nameRange, TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse, referencedField.Name));
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
                                ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisExpected);
                                break;
                        }
                    }
                    if (result == null)
                    {
                        int errorStart = fToken.TextSlice.Position;
                        NextToken();
                        while (fToken.Id != TokenId.CloseParenthesis && !IsEndSign())
                            NextToken();
                        if (fToken.Id == TokenId.CloseParenthesis)
                            NextToken();
                        int errorEnd = fPreviousTokenRange.End;
                        var errorRange = new TextRange(errorStart, errorEnd);
                        AddError(GetError(errorRange, TextResource.AttributesAreNotAllowedForStandardPattern, patternName));
                    }
                }
                result ??= pattern.Body switch
                {
                    TokenSyntax token => new TokenSyntax(token.TokenKind, token.Text, token.IsCaseSensitive, token.TextIsPrefix,
                        token.TokenAttributes),
                    VariationSyntax variation => new VariationSyntax(variation.Elements, checkCanReduce: false),
                    _ => throw SyntaxError(TextResource.InternalCompilerError)
                };
            }
            else
            {
                List<Syntax> extractionFromFields;
                if (fToken.Id == TokenId.OpenParenthesis)
                {
                    NextToken();
                    EndSign saveEndSign = fEndSign;
                    fEndSign |= EndSign.EndOfFields;
                    extractionFromFields = ParseCommaSeparatedList(ParseExtractionFromField,
                        isStartOfElement: () => fToken.Id == TokenId.Identifier);
                    ValidateTokenAndAdvance(TokenId.CloseParenthesis, TextResource.CloseParenthesisOrCommaExpected);
                    fEndSign = saveEndSign;
                }
                else
                    extractionFromFields = new List<Syntax>();
                result = Syntax.PatternReference(patternName, extractionFromFields);
            }
            return result;
        }

        private Syntax ParseExtractionFromField()
        {
            if (fToken.Id != TokenId.Identifier)
                return null;
            int startPosition = fToken.TextSlice.Position;
            string fieldName = fToken.TextSlice.ToString();
            if (fFieldByName.TryGetValue(fieldName, out FieldSyntax field))
            {
                if (!fExtractedFields.Contains(field))
                {
                    fExtractedFields.Add(field);
                    fAccessibleFields.Add(field);
                }
                else
                    AddError(GetError(TextResource.FieldAlreadyUsedForTextExtraction, field.Name));
            }
            else
            {
                AddError(GetError("Undeclared field '{0}'", fieldName));
                field = Syntax.Field(fieldName);
            }
            NextToken();
            ValidateTokenAndAdvance(TokenId.Colon, TextResource.ColonExpected);
            string fromFieldName;
            if (fToken.Id == TokenId.Identifier)
            {
                fromFieldName = fToken.TextSlice.ToString();
                NextToken();
            }
            else
            {
                AddError(GetError(TextResource.FromFieldNameExpected));
                fromFieldName = null; // TODO: null or empty string?
            }
            Syntax result = SetTextRange(Syntax.ExtractionFromField(field, fromFieldName), startPosition);
            return result;
        }

        private TextSyntax ParseText()
        {
            int startPosition = fToken.TextSlice.Position;
            TextSyntax result;
            string text = ParseStringLiteral(out bool isCaseSensitive, out bool textIsPrefix);
            if (fToken.Id == TokenId.OpenParenthesis)
            {
                if (!textIsPrefix)
                    AddError(GetError("Text attributes are only valid for text prefix literals"));
                WordAttributes attributes = ParseTextAttributes(allowWordClass: true);
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
            }
            else
                AddError(GetError(TextResource.NonEmptyStringLiteralExpected));
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
            if (fToken.Id != TokenId.CloseParenthesis)
            {
                if (fToken.Id == TokenId.Identifier)
                {
                    string value = fToken.TextSlice.ToString();
                    if (IsWordClass(value, out wordClass))
                    {
                        if (!allowWordClass)
                            AddError(GetError("Word class attribute is allowed only for text prefix literals"));
                        NextToken();
                    }
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
                        AddError(GetError(TextResource.UnknownWordAttribute, value));
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
            fPreviousTokenRange = TokenTextRange(fToken);
            do
            {
                NextKnownTokenOrComment();
            } while (fToken.Id == TokenId.Comment);
        }

        private void NextKnownTokenOrComment()
        {
            bool isUnknownToken;
            do
            {
                isUnknownToken = true;
                NextTokenOrComment();
                switch (fToken.Id)
                {
                    case TokenId.Unknown:
                        AddError(GetError("Invalid character"));
                        break;
                    case TokenId.UnknownKeyword:
                        AddError(GetError(TextResource.UnknownKeyword, fToken.TextSlice));
                        break;
                    default:
                        isUnknownToken = false;
                        break;
                }
            } while (isUnknownToken);
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
                            tokenId = TokenId.UnknownKeyword;
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
                            AddError(GetError(fTextPosition - 1, TextResource.UnterminatedComment));
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
                        {
                            AddError(GetError(fTextPosition - 1, TextResource.UnterminatedStringLiteral));
                            break;
                        }
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

        private bool ValidateToken(TokenId id, string error, bool shouldAdvance = false)
        {
            bool result;
            if (fToken.Id == id)
            {
                if (shouldAdvance)
                    NextToken();
                result = true;
            }
            else
            {
                AddError(GetError(error, args: fToken));
                result = false;
            }
            return result;
        }

        private bool ValidateTokenAndAdvance(TokenId id, string error) => ValidateToken(id, error, true);

        private T SetTextRange<T>(T syntax, int start) where T : Syntax
        {
            int end = fToken.TextSlice.Position;
            syntax.TextRange = new TextRange(start, end);
            return syntax;
        }

        private bool IsEndSign()
        {
            if (fToken.Id == TokenId.End)
                return true;
            if (fToken.Id == TokenId.SearchKeyword
                || fToken.Id == TokenId.PatternKeyword
                || fToken.Id == TokenId.NamespaceKeyword)
                return true;
            if ((fNestingContext == NestingContext.Variation || fNestingContext == NestingContext.Span) && fToken.Id == TokenId.Comma)
                return true;
            int mask = 1;
            var result = false;
            while (mask <= MaxEndSign && !result)
            {
                switch (fEndSign & (EndSign)mask)
                {
                    case EndSign.EndOfNamespaceBody:
                    case EndSign.EndOfNestedPatterns:
                    case EndSign.EndOfVariation:
                        result = fToken.Id == TokenId.CloseCurlyBrace;
                        break;
                    case EndSign.EndOfPattern:
                        result = fToken.Id == TokenId.Semicolon && fNestingContext == NestingContext.None;
                        break;
                    case EndSign.EndOfSpan:
                        result = fToken.Id == TokenId.CloseSquareBracket;
                        break;
                    case EndSign.EndOfFields:
                    case EndSign.CloseParenthesis:
                        result = fToken.Id == TokenId.CloseParenthesis;
                        break;
                    case EndSign.StartOfNestedPatterns:
                        result = fToken.Id == TokenId.WhereKeyword;
                        break;
                    default:
                        result = false;
                        break;
                }
                mask <<= 1;
            }
            return result;
        }

        private void AddError(in Error error)
        {
            if (!fIsTokenSaveMode && (fErrors.Count == 0 || fErrors[^1].ErrorRange.Start != error.ErrorRange.Start))
                fErrors.Add(error);
        }

        private Error GetError(string format) => GetError(TokenTextRange(fToken), format, args: fToken);
        
        private Error GetError(string format, params object[] args) => GetError(TokenTextRange(fToken), format, args);

        private Error GetError(int position, string format)
        {
            var errorRange = new TextRange(position, position + 1);
            return GetError(errorRange, format);
        }

        private Error GetErrorAfter(in TextRange range, string format)
        {
            var errorRange = new TextRange(range.End, range.End + 1);
            return GetError(errorRange, format);
        }

        private Error GetError(in TextRange range, string format, params object[] args)
        {
            var result = new Error(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), range);
            return result;
        }

        private TextRange TokenTextRange(in Token token)
        {
            if (token.TextSlice == null)
                return new TextRange(0, 1);
            return new TextRange(token.TextSlice.Position, token.TextSlice.Position + token.TextSlice.Length);
        }

        private void ThrowIfNotValidated(bool isValidated)
        {
            if (!isValidated)
                throw new Exception($"Internal compiler error: \"{fErrors[^1].ErrorMessage}\"");
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
            UnknownKeyword,
            End,
            Comment,
            Identifier,
            StringLiteral,
            IntegerLiteral,
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

        [Flags]
        private enum EndSign
        {
            EndOfNamespaceBody = 1 << 0,
            EndOfPattern = 1 << 1,
            StartOfNestedPatterns = 1 << 2,
            EndOfNestedPatterns = 1 << 3,
            EndOfVariation = 1 << 4,
            EndOfSpan = 1 << 5,
            CloseParenthesis = 1 << 6,
            EndOfFields = 1 << 7
        }

        private const int MaxEndSign = (int)EndSign.EndOfFields;
        
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
    
    public struct Error
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
        public const string SyntaxExceptionFormat = "{0} (at position {1}, line {2}: \"{3}\")";
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
        public const string FromFieldNameExpected = "From field name expected, but '{0}' found";
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
