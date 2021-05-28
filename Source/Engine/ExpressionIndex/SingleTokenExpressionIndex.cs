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
    internal class SingleTokenExpressionIndex : ITokenExpressionIndex
    {
        public TokenExpression TokenExpression { get; protected set; }

        public SingleTokenExpressionIndex(TokenExpression tokenExpression)
        {
            TokenExpression = tokenExpression;
        }

        public ITokenExpressionIndex Clone()
        {
            var result = new SingleTokenExpressionIndex(this.TokenExpression);
            return result;
        }

        public void SelectMatchingExpressions(Token token, bool[] excludeFlagPerPattern,
            HashSet<TokenExpression> result)
        {
            if (excludeFlagPerPattern != null && excludeFlagPerPattern[TokenExpression.PatternId])
            {
                // не добавлять TokenExpression в результат
            }
            else
            {
                if (HasMatchingToken(token))
                    result.Add(TokenExpression);
            }
        }

        public void HandleMatchingExpressions(Token token, Action<TokenExpression> action)
        {
            if (HasMatchingToken(token))
                action(TokenExpression);
        }

        public ITokenExpressionIndex MergeFrom(ITokenExpressionIndex source)
        {
            ITokenExpressionIndex result = null;
            switch (source)
            {
                case TokenExpressionIndex sourceIndex:
                    result = new TokenExpressionIndex(sourceIndex);
                    break;
                case SingleTokenExpressionIndex sourceIndex:
                    result = new TokenExpressionIndex();
                    result = result.MergeFrom(sourceIndex);
                    break;
            }
            if (result != null)
                result = result.MergeFrom(this);
            return result;
        }

        protected virtual bool HasMatchingToken(Token token)
        {
            bool result = token.Kind == TokenExpression.Kind;
            if (result)
            {
                if (string.IsNullOrEmpty(TokenExpression.Text))
                {
                    result = (TokenExpression.TokenAttributes == null ||
                        TokenExpression.TokenAttributes.CompareTo(token, TokenExpression.Text));
                }
                else if (TokenExpression.TextIsPrefix)
                    result = WordComparer.WordToPrefixComparer(token.Text, TokenExpression.Text) == 0 &&
                        WordComparer.WordPrefixAttributesEqualityComparer(token, TokenExpression);
                else if (TokenExpression.IsCaseSensitive)
                    result = string.Equals(token.Text, TokenExpression.Text, StringComparison.Ordinal);
                else
                    result = string.Equals(token.Text, TokenExpression.Text, StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }
    }
}
