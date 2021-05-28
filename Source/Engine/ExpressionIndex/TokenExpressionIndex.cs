//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    internal class TokenExpressionIndex : ITokenExpressionIndex
    {
        public const int CharIndexLength = 256;
        public static readonly int TokenKindIndexLength = typeof(TokenKind).GetEnumValues().Length;
        public static readonly int WordClassIndexLength = typeof(WordClass).GetEnumValues().Length;

        public ImmutableArray<TokenExpression>[] TokenKindIndex { get; private set; }
        public ImmutableArray<TokenExpression>[] WordClassIndex { get; private set; }
        public ImmutableArray<TokenExpression>[][] CharIndex { get; private set; }
        public Dictionary<string, ImmutableArray<TokenExpression>> CaseSensitiveIndex { get; private set; }
        public Dictionary<string, ImmutableArray<TokenExpression>> CaseInsensitiveIndex { get; private set; }
        public SortedList<string, ImmutableArray<TokenExpression>> WordPrefixIndex { get; private set; }

        public TokenExpressionIndex()
        {
        }

        public TokenExpressionIndex(TokenExpressionIndex source)
        {
            MergeFrom(source);
        }

        public ITokenExpressionIndex Clone()
        {
            var result = new TokenExpressionIndex(this);
            return result;
        }

        public ITokenExpressionIndex MergeFrom(ITokenExpressionIndex source)
        {
            switch (source)
            {
                case TokenExpressionIndex sourceIndex:
                    TokenKindIndex = TokenKindIndex.MergeFromNullable(sourceIndex.TokenKindIndex);
                    WordClassIndex = WordClassIndex.MergeFromNullable(sourceIndex.WordClassIndex);
                    CaseSensitiveIndex = CaseSensitiveIndex.MergeFromNullable(sourceIndex.CaseSensitiveIndex,
                        CaseSensitiveIndexConstructor);
                    CaseInsensitiveIndex = CaseInsensitiveIndex.MergeFromNullable(sourceIndex.CaseInsensitiveIndex,
                        CaseInsensitiveIndexConstructor);
                    WordPrefixIndex = WordPrefixIndex.MergeFromNullable(sourceIndex.WordPrefixIndex,
                        WordPrefixIndexConstructor);
                    CharIndex = CharIndex.MergeFromNullable(sourceIndex.CharIndex);
                    break;
                case SingleTokenExpressionIndex sourceIndex:
                    MergeFromSingleTokenExpression(sourceIndex.TokenExpression);
                    break;
            }
            return this;
        }

        public void SelectMatchingExpressions(Token token, bool[] excludeFlagPerPattern, HashSet<TokenExpression> result)
        {
            switch (token.Kind)
            {
                case TokenKind.Word:
                    SelectMatchingWords(token, excludeFlagPerPattern, result);
                    break;
                case TokenKind.Punctuation:
                case TokenKind.Symbol:
                    SelectMatchingTokens(token, excludeFlagPerPattern, result);
                    SelectMatchingCharacters(token, excludeFlagPerPattern, result);
                    break;
                case TokenKind.Space:
                case TokenKind.LineBreak:
                case TokenKind.Start:
                case TokenKind.End:
                    SelectMatchingTokens(token, excludeFlagPerPattern, result);
                    break;
            }
        }

        public void HandleMatchingExpressions(Token token, Action<TokenExpression> action)
        {
            switch (token.Kind)
            {
                case TokenKind.Word:
                    HandleMatchingWords(token, action);
                    break;
                case TokenKind.Punctuation:
                case TokenKind.Symbol:
                    HandleMatchingTokens(token, action);
                    HandleMatchingCharacters(token, action);
                    break;
                case TokenKind.Space:
                case TokenKind.LineBreak:
                case TokenKind.Start:
                case TokenKind.End:
                    HandleMatchingTokens(token, action);
                    break;
            }
        }

        private void SelectMatchingWords(Token token, bool[] excludeFlagPerPattern, HashSet<TokenExpression> result)
        {
            ImmutableArray<TokenExpression> values;
            if (WordClassIndex != null)
            {
                values = WordClassIndex[(int)WordClass.Any];
                SelectTokenExpressions(values, token, excludeFlagPerPattern, result);
                values = WordClassIndex[(int)token.WordClass];
                SelectTokenExpressions(values, token, excludeFlagPerPattern, result);
            }
            if (CaseInsensitiveIndex != null)
                if (CaseInsensitiveIndex.TryGetValue(token.Text, out values))
                {
                    for (int k = 0, q = values.Count; k < q; k++)
                    {
                        TokenExpression tokenExpression = values[k];
                        if (excludeFlagPerPattern != null && excludeFlagPerPattern[tokenExpression.PatternId])
                        {
                            // не добавлять TokenExpression в результат
                        }
                        else
                        {
                            result.Add(tokenExpression);
                        }
                    }
                }
            if (CaseSensitiveIndex != null)
                if (CaseSensitiveIndex.TryGetValue(token.Text, out values))
                {
                    for (int k = 0, q = values.Count; k < q; k++)
                    {
                        TokenExpression tokenExpression = values[k];
                        if (excludeFlagPerPattern != null && excludeFlagPerPattern[tokenExpression.PatternId])
                        {
                            // не добавлять TokenExpression в результат
                        }
                        else
                        {
                            result.Add(tokenExpression);
                        }
                    }
                }
            if (WordPrefixIndex != null)
            {
                WordPrefixIndex.Search(token.Text, WordComparer.WordToPrefixComparer, out int start, out int end);
                while (start < end)
                {
                    ImmutableArray<TokenExpression> list = WordPrefixIndex.Values[start];
                    for (int i = 0, n = list.Count; i < n; i++)
                    {
                        TokenExpression tokenExpression = list[i];
                        if (excludeFlagPerPattern != null && excludeFlagPerPattern[tokenExpression.PatternId])
                        {
                            // не добавлять TokenExpression в результат
                        }
                        else
                        {
                            if (WordComparer.WordPrefixAttributesEqualityComparer(token, tokenExpression))
                                result.Add(tokenExpression);
                        }
                    }
                    start++;
                }
            }
        }

        private void SelectMatchingTokens(Token token, bool[] excludeFlagPerPattern, HashSet<TokenExpression> result)
        {
            ImmutableArray<TokenExpression> values;
            if (TokenKindIndex != null)
            {
                values = TokenKindIndex[(int)token.Kind];
                SelectTokenExpressions(values, token, excludeFlagPerPattern, result);
            }
        }

        private void SelectMatchingCharacters(Token token, bool[] excludeFlagPerPattern,
            HashSet<TokenExpression> result)
        {
            ImmutableArray<TokenExpression> values;
            if (CharIndex != null)
            {
                char ch = token.Text[0];
                int i = ch >> 8;
                ImmutableArray<TokenExpression>[] group = CharIndex[i];
                if (group != null)
                {
                    int j = ch & 0x00FF;
                    values = group[j];
                    if (!values.IsNullOrEmpty())
                    {
                        for (int k = 0, q = values.Count; k < q; k++)
                        {
                            TokenExpression tokenExpression = values[k];
                            if (excludeFlagPerPattern != null && excludeFlagPerPattern[tokenExpression.PatternId])
                            {
                                // не добавлять TokenExpression в результат
                            }
                            else
                            {
                                result.Add(tokenExpression);
                            }
                        }
                    }
                }
            }
        }

        private void SelectTokenExpressions(ImmutableArray<TokenExpression> list, Token token, bool[] excludeFlagPerPattern,
            HashSet<TokenExpression> result)
        {
            if (!list.IsNullOrEmpty())
            {
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    TokenExpression tokenExpression = list[i];
                    if (excludeFlagPerPattern != null && excludeFlagPerPattern[tokenExpression.PatternId])
                    {
                        // не добавлять TokenExpression в результат
                    }
                    else
                    {
                        if (tokenExpression.TokenAttributes == null ||
                            tokenExpression.TokenAttributes.CompareTo(token, tokenExpression.Text))
                        {
                            result.Add(tokenExpression);
                        }
                    }
                }
            }
        }

        private void HandleMatchingWords(Token token, Action<TokenExpression> action)
        {
            ImmutableArray<TokenExpression> values;
            if (WordClassIndex != null)
            {
                values = WordClassIndex[(int)WordClass.Any];
                HandleTokenExpressions(values, token, action);
                values = WordClassIndex[(int)token.WordClass];
                HandleTokenExpressions(values, token, action);
            }
            if (CaseInsensitiveIndex != null)
                if (CaseInsensitiveIndex.TryGetValue(token.Text, out values))
                    for (int i = 0, n = values.Count; i < n; i++)
                    {
                        TokenExpression tokenExpression = values[i];
                        action(tokenExpression);
                    }
            if (CaseSensitiveIndex != null)
                if (CaseSensitiveIndex.TryGetValue(token.Text, out values))
                    for (int i = 0, n = values.Count; i < n; i++)
                    {
                        TokenExpression tokenExpression = values[i];
                        action(tokenExpression);
                    }
            if (WordPrefixIndex != null)
            {
                WordPrefixIndex.Search(token.Text, WordComparer.WordToPrefixComparer, out int start, out int end);
                while (start < end)
                {
                    ImmutableArray<TokenExpression> list = WordPrefixIndex.Values[start];
                    for (int i = 0, n = list.Count; i < n; i++)
                    {
                        TokenExpression tokenExpression = list[i];
                        if (WordComparer.WordPrefixAttributesEqualityComparer(token, tokenExpression))
                            action(tokenExpression);
                    }
                    start++;
                }
            }
        }

        private void HandleMatchingTokens(Token token, Action<TokenExpression> action)
        {
            ImmutableArray<TokenExpression> values;
            if (TokenKindIndex != null)
            {
                values = TokenKindIndex[(int)token.Kind];
                HandleTokenExpressions(values, token, action);
            }
        }

        private void HandleMatchingCharacters(Token token, Action<TokenExpression> action)
        {
            ImmutableArray<TokenExpression> values;
            if (CharIndex != null)
            {
                char ch = token.Text[0];
                int i = ch >> 8;
                ImmutableArray<TokenExpression>[] group = CharIndex[i];
                if (group != null)
                {
                    int j = ch & 0x00FF;
                    values = group[j];
                    if (!values.IsNullOrEmpty())
                        for (int k = 0, q = values.Count; k < q; k++)
                        {
                            TokenExpression tokenExpression = values[k];
                            action(tokenExpression);
                        }
                }
            }
        }

        private void HandleTokenExpressions(ImmutableArray<TokenExpression> list, Token token, 
            Action<TokenExpression> action)
        {
            if (!list.IsNullOrEmpty())
            {
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    TokenExpression tokenExpression = list[i];
                    if (tokenExpression.TokenAttributes == null ||
                        tokenExpression.TokenAttributes.CompareTo(token, tokenExpression.Text))
                    {
                        action(tokenExpression);
                    }
                }
            }
        }

        // Internal

        private void MergeFromSingleTokenExpression(TokenExpression tokenExpression)
        {
            if (tokenExpression.Kind == TokenKind.Word)
            {
                if (string.IsNullOrEmpty(tokenExpression.Text))
                {
                    WordClass wordClass;
                    if (tokenExpression.TokenAttributes == null)
                        wordClass = WordClass.Any;
                    else
                        wordClass = ((WordAttributes)tokenExpression.TokenAttributes).WordClass;
                    GetOrCreateWordClassIndex().AddValueListItem((int)wordClass, tokenExpression);
                }
                else if (tokenExpression.TextIsPrefix)
                    GetOrCreateWordPrefixIndex().AddValueListItem(tokenExpression.Text, tokenExpression);
                else if (tokenExpression.IsCaseSensitive)
                    GetOrCreateCaseSensitiveIndex().AddValueListItem(tokenExpression.Text, tokenExpression);
                else
                    GetOrCreateCaseInsensitiveIndex().AddValueListItem(tokenExpression.Text, tokenExpression);
            }
            else
            {
                if (string.IsNullOrEmpty(tokenExpression.Text))
                    GetOrCreateTokenKindIndex().AddValueListItem((int)tokenExpression.Kind, tokenExpression);
                else if (tokenExpression.Text.Length == 1)
                    GetOrCreateCharIndex().AddValueListItem(tokenExpression.Text[0], tokenExpression);
                else
                    GetOrCreateCaseSensitiveIndex().AddValueListItem(tokenExpression.Text, tokenExpression);
            }
        }

        private ImmutableArray<TokenExpression>[] GetOrCreateTokenKindIndex()
        {
            if (TokenKindIndex == null)
                TokenKindIndex = new ImmutableArray<TokenExpression>[TokenKindIndexLength];
            return TokenKindIndex;
        }

        private ImmutableArray<TokenExpression>[] GetOrCreateWordClassIndex()
        {
            if (WordClassIndex == null)
                WordClassIndex = new ImmutableArray<TokenExpression>[WordClassIndexLength];
            return WordClassIndex;
        }

        private ImmutableArray<TokenExpression>[][] GetOrCreateCharIndex()
        {
            if (CharIndex == null)
                CharIndex = new ImmutableArray<TokenExpression>[CharIndexLength][];
            return CharIndex;
        }

        private Dictionary<string, ImmutableArray<TokenExpression>> GetOrCreateCaseSensitiveIndex()
        {
            if (CaseSensitiveIndex == null)
                CaseSensitiveIndex = CaseSensitiveIndexConstructor();
            return CaseSensitiveIndex;
        }

        private Dictionary<string, ImmutableArray<TokenExpression>> GetOrCreateCaseInsensitiveIndex()
        {
            if (CaseInsensitiveIndex == null)
                CaseInsensitiveIndex = CaseInsensitiveIndexConstructor();
            return CaseInsensitiveIndex;
        }

        private SortedList<string, ImmutableArray<TokenExpression>> GetOrCreateWordPrefixIndex()
        {
            if (WordPrefixIndex == null)
                WordPrefixIndex = WordPrefixIndexConstructor();
            return WordPrefixIndex;
        }

        private static readonly Func<Dictionary<string, ImmutableArray<TokenExpression>>> CaseSensitiveIndexConstructor =
            () => new Dictionary<string, ImmutableArray<TokenExpression>>(StringComparer.Ordinal);
        private static readonly Func<Dictionary<string, ImmutableArray<TokenExpression>>> CaseInsensitiveIndexConstructor =
            () => new Dictionary<string, ImmutableArray<TokenExpression>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Func<SortedList<string, ImmutableArray<TokenExpression>>> WordPrefixIndexConstructor =
            () => new SortedList<string, ImmutableArray<TokenExpression>>(StringComparer.OrdinalIgnoreCase);
    }
}
