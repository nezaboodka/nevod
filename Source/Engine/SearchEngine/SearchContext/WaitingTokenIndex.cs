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
    // Индекс кандидатов, которые перестают обрабатывать приходящие токены по-одному и ожидают их вместе.
    // Предназначен для AnySpanCandidate и производных типов.
    // При совпадении левой части в указанных кандидатах, их корневые кандидаты (RootCandidate)
    // изымаются из обычного списка обработчиков и добавляются в текущий индекс.
    // Далее на каждом новом токене происходит поиск подходящих для продолжения совпадения кандидатов
    // в объединённом индексе.
    // Если кандидат для продолжения совпадения найден, то создаются их копии; исходный кандидат остаётся ожидать
    // с целью поиска более длинного совпадение на тот случай, если созданные копии отменятся.
    // Как только одна из копий успешно совпала, соответствующий ей исходный кандидат извлекается из индекса
    // и отменяется вместе с другими более длинными копиями.
    internal class WaitingTokenIndex
    {
        public const int CharIndexLength = TokenExpressionIndex.CharIndexLength;
        public static readonly int TokenKindIndexLength = TokenExpressionIndex.TokenKindIndexLength;
        public static readonly int WordClassIndexLength = TokenExpressionIndex.WordClassIndexLength;

        public SearchContext SearchContext { get; }
        public WaitingTokenList[] TokenKindIndex { get; private set; }
        public WaitingTokenList[] WordClassIndex { get; private set; }
        public WaitingTokenList[][] CharIndex { get; private set; }
        public Dictionary<string, WaitingTokenList> CaseSensitiveIndex { get; private set; }
        public Dictionary<string, WaitingTokenList> CaseInsensitiveIndex { get; private set; }
        public SortedList<string, WaitingTokenList> WordPrefixIndex { get; private set; }

        public WaitingTokenIndex(SearchContext searchContext)
        {
            SearchContext = searchContext;
            TokenKindIndex = new WaitingTokenList[TokenKindIndexLength];
            WordClassIndex = new WaitingTokenList[WordClassIndexLength];
            CharIndex = new WaitingTokenList[CharIndexLength][];
            CaseSensitiveIndex = new Dictionary<string, WaitingTokenList>();
            CaseInsensitiveIndex = new Dictionary<string, WaitingTokenList>(
                StringComparer.OrdinalIgnoreCase);
            WordPrefixIndex = new SortedList<string, WaitingTokenList>(
                StringComparer.OrdinalIgnoreCase);
        }

        public void Reset()
        {
            Array.Clear(TokenKindIndex, 0, TokenKindIndex.Length);
            Array.Clear(WordClassIndex, 0, WordClassIndex.Length);
            Array.Clear(CharIndex, 0, CharIndex.Length);
            CaseSensitiveIndex.Clear();
            CaseInsensitiveIndex.Clear();
            WordPrefixIndex.Clear();
        }

        public void AddWaitingTokens(ITokenExpressionIndex tokenExpressionIndex, AnySpanCandidate candidate,
            bool isException)
        {
            switch (tokenExpressionIndex)
            {
                case TokenExpressionIndex index:
                    AddValuesToIndex(index.TokenKindIndex, TokenKindIndex, candidate, isException);
                    AddValuesToIndex(index.WordClassIndex, WordClassIndex, candidate, isException);
                    AddValuesToIndex(index.CharIndex, CharIndex, candidate, isException);
                    AddValuesToIndex(index.CaseSensitiveIndex, CaseSensitiveIndex, candidate, isException);
                    AddValuesToIndex(index.CaseInsensitiveIndex, CaseInsensitiveIndex, candidate, isException);
                    AddValuesToIndex(index.WordPrefixIndex, WordPrefixIndex, candidate, isException);
                    break;
                case SingleTokenExpressionIndex index:
                    WaitingToken waitingToken = SearchContext.CandidateFactory.CreateWaitingToken(
                        index.TokenExpression, candidate, isException);
                    candidate.GetRootCandidate().AddWaitingToken(waitingToken);
                    TokenExpression tokenExpression = index.TokenExpression;
                    if (tokenExpression.Kind == TokenKind.Word)
                    {
                        if (string.IsNullOrEmpty(tokenExpression.Text))
                        {
                            WordClass wordClass;
                            if (tokenExpression.TokenAttributes == null)
                                wordClass = WordClass.Any;
                            else
                                wordClass = ((WordAttributes)tokenExpression.TokenAttributes).WordClass;
                            WordClassIndex.AddValueListItem((int)wordClass, waitingToken);
                        }
                        else if (tokenExpression.TextIsPrefix)
                            WordPrefixIndex.AddValueListItem(tokenExpression.Text, waitingToken);
                        else if (tokenExpression.IsCaseSensitive)
                            CaseSensitiveIndex.AddValueListItem(tokenExpression.Text, waitingToken);
                        else
                            CaseInsensitiveIndex.AddValueListItem(tokenExpression.Text, waitingToken);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tokenExpression.Text))
                            TokenKindIndex.AddValueListItem((int)tokenExpression.Kind, waitingToken);
                        else
                            CharIndex.AddValueListItem(tokenExpression.Text[0], waitingToken);
                    }
                    break;
                case null:
                    // Пропустить
                    break;
            }
        }

        public void SelectMatchingTokenCandidates(Token token, bool[] excludeFlagPerPattern,
            List<WaitingToken> candidates, List<WaitingToken> exceptions)
        {
            switch (token.Kind)
            {
                case TokenKind.Word:
                    SelectMatchingWords(token, excludeFlagPerPattern, candidates, exceptions);
                    break;
                case TokenKind.Punctuation:
                case TokenKind.Symbol:
                    SelectMatchingTokens(token, excludeFlagPerPattern, candidates, exceptions);
                    SelectMatchingCharacters(token, excludeFlagPerPattern, candidates, exceptions);
                    break;
                case TokenKind.Space:
                case TokenKind.LineBreak:
                case TokenKind.Start:
                case TokenKind.End:
                    SelectMatchingTokens(token, excludeFlagPerPattern, candidates, exceptions);
                    break;
            }
        }

        public void TryRemoveDisposedCandidates()
        {
            WaitingTokenList list;
            for (int i = 0, n = TokenKindIndex.Length; i < n; i++)
            {
                list = TokenKindIndex[i];
                if (list != null)
                {
                    list.TryRemoveDisposedCandidates();
                    if (list.IsEmpty())
                        TokenKindIndex[i] = null;
                }
            }
            for (int i = 0, n = WordClassIndex.Length; i < n; i++)
            {
                list = WordClassIndex[i];
                if (list != null)
                {
                    list.TryRemoveDisposedCandidates();
                    if (list.IsEmpty())
                        WordClassIndex[i] = null;
                }
            }
            for (int i = 0, n = CharIndex.Length; i < n; i++)
            {
                WaitingTokenList[] listOfLists = CharIndex[i];
                if (listOfLists != null)
                {
                    int disposedListCount = 0;
                    for (int j = 0, m = listOfLists.Length; j < m; j++)
                    {
                        list = listOfLists[j];
                        if (list != null)
                        {
                            list.TryRemoveDisposedCandidates();
                            if (list.IsEmpty())
                            {
                                listOfLists[j] = null;
                                disposedListCount++;
                            }
                        }
                        else
                            disposedListCount++;
                    }
                    if (listOfLists.Length == disposedListCount)
                        CharIndex[i] = null;
                }
            }
            foreach (var keyValue in CaseSensitiveIndex)
            {
                list = keyValue.Value;
                list.TryRemoveDisposedCandidates();
                // if (list.IsEmpty())
                //     CaseSensitiveIndex.Remove(keyValue.Key);
            }
            foreach (var keyValue in CaseInsensitiveIndex)
            {
                list = keyValue.Value;
                list.TryRemoveDisposedCandidates();
                // if (list.IsEmpty())
                //     CaseInsensitiveIndex.Remove(keyValue.Key);
            }
            foreach (var keyValue in WordPrefixIndex)
            {
                list = keyValue.Value;
                list.TryRemoveDisposedCandidates();
                // if (list.IsEmpty())
                //     WordPrefixIndex.Remove(keyValue.Key);
            }
        }

        public void RejectAll()
        {
            WaitingTokenList list;
            for (int i = 0, n = TokenKindIndex.Length; i < n; i++)
            {
                list = TokenKindIndex[i];
                if (list != null)
                    list.RejectAll();
                TokenKindIndex[i] = null;
            }
            for (int i = 0, n = WordClassIndex.Length; i < n; i++)
            {
                list = WordClassIndex[i];
                if (list != null)
                    list.RejectAll();
                WordClassIndex[i] = null;
            }
            for (int i = 0, n = CharIndex.Length; i < n; i++)
            {
                WaitingTokenList[] listOfLists = CharIndex[i];
                if (listOfLists != null)
                {
                    for (int j = 0, m = listOfLists.Length; j < m; j++)
                    {
                        list = listOfLists[j];
                        if (list != null)
                            list.RejectAll();
                        listOfLists[j] = null;
                    }
                    CharIndex[i] = null;
                }
            }
            foreach (var keyValue in CaseSensitiveIndex)
            {
                list = keyValue.Value;
                list.RejectAll();
            }
            CaseSensitiveIndex.Clear();
            foreach (var keyValue in CaseInsensitiveIndex)
            {
                list = keyValue.Value;
                list.RejectAll();
            }
            CaseInsensitiveIndex.Clear();
            foreach (var keyValue in WordPrefixIndex)
            {
                list = keyValue.Value;
                list.RejectAll();
            }
            WordPrefixIndex.Clear();
        }

        public void RejectCandidatesWithWordLimitExceeded()
        {
            ForEachWaitingToken((WaitingToken waitingToken) =>
            {
                if (!waitingToken.IsDisposed)
                {
                    AnySpanCandidate candidate = waitingToken.Candidate;
                    Range spanRangeInWords =
                        ((AnySpanExpression)candidate.Expression).SpanRangeInWords;
                    int skippedWordCount =
                        SearchContext.ProcessedWordCount - candidate.SavedWordCount;
                    if (skippedWordCount > spanRangeInWords.HighBound)
                        candidate.RejectTarget();
                }
            });
        }

        // Т.к. у одного корневого кандидата может быть несколько токенов для ожидания,
        // действие может применяться к одному и тому же корневому кандидату несколько раз.
        public void ForEach(Action<RootCandidate> action)
        {
            ForEachWaitingToken(x => action(x.Candidate.GetRootCandidate()));
        }

        public void ForEachWaitingToken(Action<WaitingToken> action)
        {
            WaitingTokenList list;
            for (int i = 0, n = TokenKindIndex.Length; i < n; i++)
            {
                list = TokenKindIndex[i];
                if (list != null)
                {
                    for (int j = 0, m = list.Count; j < m; j++)
                        action(list[j]);
                }
            }
            for (int i = 0, n = WordClassIndex.Length; i < n; i++)
            {
                list = WordClassIndex[i];
                if (list != null)
                {
                    for (int j = 0, m = list.Count; j < m; j++)
                        action(list[j]);
                }
            }
            for (int i = 0, n = CharIndex.Length; i < n; i++)
            {
                WaitingTokenList[] listOfLists = CharIndex[i];
                if (listOfLists != null)
                {
                    for (int j = 0, m = listOfLists.Length; j < m; j++)
                    {
                        list = listOfLists[j];
                        if (list != null)
                        {
                            for (int k = 0, q = list.Count; k < q; k++)
                                action(list[k]);
                        }
                    }
                }
            }
            foreach (var keyValue in CaseSensitiveIndex)
            {
                list = keyValue.Value;
                for (int j = 0, m = list.Count; j < m; j++)
                    action(list[j]);
            }
            foreach (var keyValue in CaseInsensitiveIndex)
            {
                list = keyValue.Value;
                for (int j = 0, m = list.Count; j < m; j++)
                    action(list[j]);
            }
            foreach (var keyValue in WordPrefixIndex)
            {
                list = keyValue.Value;
                for (int j = 0, m = list.Count; j < m; j++)
                    action(list[j]);
            }
        }

        // Internal

        private void AddValuesToIndex<TKey>(IDictionary<TKey, ImmutableArray<TokenExpression>> source,
            IDictionary<TKey, WaitingTokenList> target, AnySpanCandidate candidate, bool isException)
        {
            if (source != null)
            {
                foreach (KeyValuePair<TKey, ImmutableArray<TokenExpression>> keyValue in source)
                {
                    WaitingTokenList list = target.GetOrCreate(keyValue.Key);
                    for (int i = 0, n = keyValue.Value.Count; i < n; i++)
                    {
                        var waitingToken = SearchContext.CandidateFactory.CreateWaitingToken(keyValue.Value[i],
                            candidate, isException);
                        list.Add(waitingToken);
                        waitingToken.Container = list;
                        candidate.GetRootCandidate().AddWaitingToken(waitingToken);
                    }
                }
            }
        }

        private void AddValuesToIndex(ImmutableArray<TokenExpression>[] source,
            WaitingTokenList[] target, AnySpanCandidate candidate, bool isException)
        {
            if (source != null)
            {
                for (int i = 0, n = source.Length; i < n; i++)
                {
                    ImmutableArray<TokenExpression> values = source[i];
                    if (!values.IsNullOrEmpty())
                    {
                        WaitingTokenList list = target[i];
                        if (list == null)
                        {
                            list = new WaitingTokenList();
                            target[i] = list;
                        }
                        for (int j = 0, m = values.Count; j < m; j++)
                        {
                            var waitingToken = SearchContext.CandidateFactory.CreateWaitingToken(values[j],
                                candidate, isException);
                            list.Add(waitingToken);
                            waitingToken.Container = list;
                            candidate.GetRootCandidate().AddWaitingToken(waitingToken);
                        }
                    }
                }
            }
        }

        private void AddValuesToIndex(ImmutableArray<TokenExpression>[][] source,
            WaitingTokenList[][] target, AnySpanCandidate candidate, bool isException)
        {
            if (source != null)
            {
                for (int i = 0, n = source.Length; i < n; i++)
                {
                    ImmutableArray<TokenExpression>[] sourceLists = source[i];
                    if (sourceLists != null)
                    {
                        WaitingTokenList[] targetLists = target[i];
                        if (targetLists == null)
                        {
                            targetLists = new WaitingTokenList[CharIndexLength];
                            target[i] = targetLists;
                        }
                        for (int j = 0, m = sourceLists.Length; j < m; j++)
                        {
                            ImmutableArray<TokenExpression> sourceList = sourceLists[j];
                            if (!sourceList.IsNullOrEmpty())
                            {
                                WaitingTokenList targetList = targetLists[j];
                                if (targetList == null)
                                {
                                    targetList = new WaitingTokenList();
                                    targetLists[j] = targetList;
                                }
                                ImmutableArray<TokenExpression> values = source[i][j];
                                for (int k = 0, q = values.Count; k < q; k++)
                                {
                                    var waitingToken = SearchContext.CandidateFactory.CreateWaitingToken(
                                        values[k], candidate, isException);
                                    targetList.Add(waitingToken);
                                    waitingToken.Container = targetList;
                                    candidate.GetRootCandidate().AddWaitingToken(waitingToken);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SelectMatchingWords(Token token, bool[] excludeFlagPerPattern, List<WaitingToken> candidates,
            List<WaitingToken> exceptions)
        {
            WaitingTokenList values;
            if (WordClassIndex != null)
            {
                values = WordClassIndex[(int)WordClass.Any];
                SelectTokenExpressions(values, token, excludeFlagPerPattern, candidates, exceptions);
                values = WordClassIndex[(int)token.WordClass];
                SelectTokenExpressions(values, token, excludeFlagPerPattern, candidates, exceptions);
            }
            if (CaseSensitiveIndex != null)
            {
                if (CaseSensitiveIndex.TryGetValue(token.Text, out values))
                {
                    for (int k = 0, q = values.Count; k < q; k++)
                    {
                        WaitingToken candidate = values[k];
                        if (!candidate.IsDisposed)
                        {
                            if (excludeFlagPerPattern != null
                                && excludeFlagPerPattern[candidate.TokenExpression.PatternId])
                            {
                                // не добавлять WaitingToken в результат и изъять из ожидания
                                candidate.Candidate.RejectTarget();
                            }
                            else
                            {
                                Range spanRangeInWords =
                                    ((AnySpanExpression)candidate.Candidate.Expression).SpanRangeInWords;
                                int skippedWordCount =
                                    SearchContext.ProcessedWordCount - candidate.Candidate.SavedWordCount;
                                if (skippedWordCount >= spanRangeInWords.LowBound)
                                {
                                    if (skippedWordCount <= spanRangeInWords.HighBound)
                                    {
                                        if (candidate.IsException)
                                            exceptions.Add(candidate);
                                        else
                                            candidates.Add(candidate);
                                    }
                                    else
                                        candidate.Candidate.RejectTarget();
                                }
                            }
                        }
                    }
                }
            }
            if (CaseInsensitiveIndex != null)
            {
                if (CaseInsensitiveIndex.TryGetValue(token.Text, out values))
                {
                    for (int k = 0, q = values.Count; k < q; k++)
                    {
                        WaitingToken candidate = values[k];
                        if (!candidate.IsDisposed)
                        {
                            if (excludeFlagPerPattern != null
                                && excludeFlagPerPattern[candidate.TokenExpression.PatternId])
                            {
                                // не добавлять WaitingToken в результат и изъять из ожидания
                                candidate.Candidate.RejectTarget();
                            }
                            else
                            {
                                Range spanRangeInWords =
                                    ((AnySpanExpression)candidate.Candidate.Expression).SpanRangeInWords;
                                int skippedWordCount =
                                    SearchContext.ProcessedWordCount - candidate.Candidate.SavedWordCount;
                                if (skippedWordCount >= spanRangeInWords.LowBound)
                                {
                                    if (skippedWordCount <= spanRangeInWords.HighBound)
                                    {
                                        if (candidate.IsException)
                                            exceptions.Add(candidate);
                                        else
                                            candidates.Add(candidate);
                                    }
                                    else
                                        candidate.Candidate.RejectTarget();
                                }
                            }
                        }
                    }
                }
            }
            if (WordPrefixIndex != null)
            {
                WordPrefixIndex.Search(token.Text, WordComparer.WordToPrefixComparer, out int start, out int end);
                while (start < end)
                {
                    values = WordPrefixIndex.Values[start];
                    for (int k = 0, q = values.Count; k < q; k++)
                    {
                        WaitingToken candidate = values[k];
                        if (!candidate.IsDisposed)
                        {
                            if (excludeFlagPerPattern != null
                                && excludeFlagPerPattern[candidate.TokenExpression.PatternId])
                            {
                                // не добавлять WaitingToken в результат и изъять из ожидания
                                candidate.Candidate.RejectTarget();
                            }
                            else
                            {
                                Range spanRangeInWords =
                                    ((AnySpanExpression)candidate.Candidate.Expression).SpanRangeInWords;
                                int skippedWordCount =
                                    SearchContext.ProcessedWordCount - candidate.Candidate.SavedWordCount;
                                if (skippedWordCount >= spanRangeInWords.LowBound)
                                {
                                    if (skippedWordCount <= spanRangeInWords.HighBound)
                                    {
                                        if (WordComparer.WordPrefixAttributesEqualityComparer(token, candidate.TokenExpression))
                                        {
                                            if (candidate.IsException)
                                                exceptions.Add(candidate);
                                            else
                                                candidates.Add(candidate);
                                        }
                                    }
                                    else
                                        candidate.Candidate.RejectTarget();
                                }
                            }
                        }
                    }
                    start++;
                }
            }
        }

        private void SelectMatchingTokens(Token token, bool[] excludeFlagPerPattern,
            List<WaitingToken> candidates, List<WaitingToken> exceptions)
        {
            WaitingTokenList values;
            if (TokenKindIndex != null)
            {
                values = TokenKindIndex[(int)token.Kind];
                SelectTokenExpressions(values, token, excludeFlagPerPattern, candidates, exceptions);
            }
        }

        private void SelectMatchingCharacters(Token token, bool[] excludeFlagPerPattern,
            List<WaitingToken> candidates, List<WaitingToken> exceptions)
        {
            WaitingTokenList values;
            if (CharIndex != null)
            {
                char ch = token.Text[0];
                int i = ch >> 8;
                WaitingTokenList[] group = CharIndex[i];
                if (group != null)
                {
                    int j = ch & 0x00FF;
                    values = group[j];
                    if (values != null)
                    {
                        for (int k = 0, q = values.Count; k < q; k++)
                        {
                            WaitingToken candidate = values[k];
                            if (!candidate.IsDisposed)
                            {
                                if (excludeFlagPerPattern != null
                                    && excludeFlagPerPattern[candidate.TokenExpression.PatternId])
                                {
                                    // не добавлять WaitingToken в результат и изъять из ожидания
                                    candidate.Candidate.RejectTarget();
                                }
                                else
                                {
                                    Range spanRangeInWords =
                                        ((AnySpanExpression)candidate.Candidate.Expression).SpanRangeInWords;
                                    int skippedWordCount =
                                        SearchContext.ProcessedWordCount - candidate.Candidate.SavedWordCount;
                                    if (skippedWordCount >= spanRangeInWords.LowBound)
                                    {
                                        if (skippedWordCount <= spanRangeInWords.HighBound)
                                        {
                                            if (candidate.IsException)
                                                exceptions.Add(candidate);
                                            else
                                                candidates.Add(candidate);
                                        }
                                        else
                                            candidate.Candidate.RejectTarget();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SelectTokenExpressions(WaitingTokenList list, Token token, bool[] excludeFlagPerPattern,
            List<WaitingToken> candidates, List<WaitingToken> exceptions)
        {
            if (list != null)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    WaitingToken candidate = list[i];
                    if (!candidate.IsDisposed)
                    {
                        if (excludeFlagPerPattern != null
                            && excludeFlagPerPattern[candidate.TokenExpression.PatternId])
                        {
                            // не добавлять WaitingToken в результат и изъять из ожидания
                            candidate.Candidate.RejectTarget();
                        }
                        else
                        {
                            if (candidate.TokenExpression.TokenAttributes == null ||
                                candidate.TokenExpression.TokenAttributes.CompareTo(token, candidate.TokenExpression.Text))
                            {
                                Range spanRangeInWords =
                                    ((AnySpanExpression)candidate.Candidate.Expression).SpanRangeInWords;
                                int skippedWordCount =
                                    SearchContext.ProcessedWordCount - candidate.Candidate.SavedWordCount;
                                if (skippedWordCount >= spanRangeInWords.LowBound)
                                {
                                    if (skippedWordCount <= spanRangeInWords.HighBound)
                                    {
                                        if (candidate.IsException)
                                            exceptions.Add(candidate);
                                        else
                                            candidates.Add(candidate);
                                    }
                                    else
                                        candidate.Candidate.RejectTarget();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal static class CandidateIndexDictionaryExtension
    {
        public static void AddValueListItem<TKey>(this IDictionary<TKey, WaitingTokenList> dictionary,
            TKey key, WaitingToken value)
        {
            WaitingTokenList valueList = dictionary.GetOrCreate(key);
            valueList.Add(value);
            value.Container = valueList;
        }

        public static void AddValueListItem(this WaitingTokenList[] dictionary,
            int key, WaitingToken value)
        {
            WaitingTokenList valueList = dictionary[key];
            if (valueList != null)
            {
                valueList.Add(value);
            }
            else
            {
                valueList = new WaitingTokenList { value };
                dictionary[key] = valueList;
            }
            value.Container = valueList;
        }

        public static void AddValueListItem(this WaitingTokenList[][] dictionary,
            char key, WaitingToken value)
        {
            int high = key >> 8;
            WaitingTokenList[] list = dictionary[high];
            if (list == null)
            {
                list = new WaitingTokenList[256];
                dictionary[high] = list;
            }
            int low = key & 0x00FF;
            WaitingTokenList valueList = list[low];
            if (valueList != null)
            {
                valueList.Add(value);
            }
            else
            {
                valueList = new WaitingTokenList { value };
                list[low] = valueList;
            }
            value.Container = valueList;
        }
    }
}
