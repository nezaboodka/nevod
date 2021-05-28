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
    internal class ExpressionIndex
    {
        public ITokenExpressionIndex TokenIndex { get; set; }
        public ITokenExpressionIndex ExceptionTokenIndex { get; set; }
        public IReferenceExpressionIndex ReferenceIndex { get; set; }
        public IReferenceExpressionIndex ExceptionReferenceIndex { get; set; }

        public ITokenExpressionIndex OptionalTokenIndex { get; set; }
        public ITokenExpressionIndex ExceptionOptionalTokenIndex { get; set; }
        public IReferenceExpressionIndex OptionalReferenceIndex { get; set; }
        public IReferenceExpressionIndex ExceptionOptionalReferenceIndex { get; set; }

        public ExpressionIndex()
        {
        }

        public ExpressionIndex(ITokenExpressionIndex tokenIndex, IReferenceExpressionIndex referenceIndex = null)
        {
            TokenIndex = tokenIndex;
            ReferenceIndex = referenceIndex;
        }

        public ExpressionIndex(IReferenceExpressionIndex referenceIndex)
            : this(tokenIndex: null, referenceIndex)
        {
        }

        public bool HasExceptionTokens(bool includeOptional)
        {
            return (includeOptional && ExceptionOptionalTokenIndex != null) || (ExceptionTokenIndex != null);
        }


        public void SelectMatchingTokenExpressions(Token token, bool includeOptional, bool[] excludeFlagPerPattern,
            HashSet<TokenExpression> result)
        {
            if (TokenIndex != null)
                TokenIndex.SelectMatchingExpressions(token, excludeFlagPerPattern, result);
            if (includeOptional && OptionalTokenIndex != null)
                OptionalTokenIndex.SelectMatchingExpressions(token, excludeFlagPerPattern, result);
        }

        public void SelectMatchingExceptionTokenExpressions(Token token, bool includeOptional,
            bool[] excludeFlagPerPattern, HashSet<TokenExpression> result)
        {
            if (ExceptionTokenIndex != null)
                ExceptionTokenIndex.SelectMatchingExpressions(token, excludeFlagPerPattern, result);
            if (includeOptional && ExceptionOptionalTokenIndex != null)
                ExceptionOptionalTokenIndex.SelectMatchingExpressions(token, excludeFlagPerPattern, result);
        }

        public void HandleMatchingTokenExpressions(Token token, bool includeOptional, Action<TokenExpression> action)
        {
            if (TokenIndex != null)
                TokenIndex.HandleMatchingExpressions(token, action);
            if (includeOptional && OptionalTokenIndex != null)
                OptionalTokenIndex.HandleMatchingExpressions(token, action);
        }

        public void HandleMatchingExceptionTokenExpressions(Token token, bool includeOptional,
            Action<TokenExpression> action)
        {
            if (ExceptionTokenIndex != null)
                ExceptionTokenIndex.HandleMatchingExpressions(token, action);
            if (includeOptional && ExceptionOptionalTokenIndex != null)
                ExceptionOptionalTokenIndex.HandleMatchingExpressions(token, action);
        }

        // Static

        public static ExpressionIndex CreateFromToken(TokenExpression tokenExpression)
        {
            var implementation = new SingleTokenExpressionIndex(tokenExpression);
            return new ExpressionIndex(implementation);
        }

        public static ExpressionIndex CreateFromReference(PatternReferenceExpression referenceExpression)
        {
            var referenceExpressionIndex = new SingleReferenceExpressionIndex(referenceExpression);
            return new ExpressionIndex(referenceExpressionIndex);
        }
    }

    internal interface ITokenExpressionIndex
    {
        ITokenExpressionIndex Clone();
        ITokenExpressionIndex MergeFrom(ITokenExpressionIndex anotherIndex);
        void SelectMatchingExpressions(Token key, bool[] excludeFlagPerPattern, HashSet<TokenExpression> result);
        void HandleMatchingExpressions(Token key, Action<TokenExpression> action);
    }

    internal interface IReferenceExpressionIndex : IEnumerable<PatternReferenceExpression>
    {
        IReferenceExpressionIndex Clone();
        IReferenceExpressionIndex MergeFrom(IReferenceExpressionIndex anotherIndex);
        void SelectMatchingExpressions(int key, bool[] excludeFlagPerPattern, HashSet<PatternReferenceExpression> result);
    }

    internal static partial class ExpressionIndexExtension
    {
        // Добавление элементов в основную часть индекса как из основной части другого индекса,
        // так и из опциональной части другого индекса
        public static ExpressionIndex MergeIntoMainPart(this ExpressionIndex index, ExpressionIndex anotherIndex)
        {
            if (anotherIndex != null)
            {
                if (index == null)
                    index = new ExpressionIndex();
                index.TokenIndex = index.TokenIndex
                    .MergeFromNullable(anotherIndex.TokenIndex)
                    .MergeFromNullable(anotherIndex.OptionalTokenIndex);
                index.ExceptionTokenIndex = index.ExceptionTokenIndex
                    .MergeFromNullable(anotherIndex.ExceptionTokenIndex)
                    .MergeFromNullable(anotherIndex.ExceptionOptionalTokenIndex);
                index.ReferenceIndex = index.ReferenceIndex
                    .MergeFromNullable(anotherIndex.ReferenceIndex)
                    .MergeFromNullable(anotherIndex.OptionalReferenceIndex);
                index.ExceptionReferenceIndex = index.ExceptionReferenceIndex
                    .MergeFromNullable(anotherIndex.ExceptionReferenceIndex)
                    .MergeFromNullable(anotherIndex.ExceptionOptionalReferenceIndex);
            }
            return index;
        }

        // Добавление элементов в опциональную часть индекса как из опциональной части другого индекса,
        // так и из основной части другого индекса
        public static ExpressionIndex MergeIntoOptionalPart(this ExpressionIndex index, ExpressionIndex anotherIndex)
        {
            if (anotherIndex != null)
            {
                if (index == null)
                    index = new ExpressionIndex();
                index.OptionalTokenIndex = index.OptionalTokenIndex
                    .MergeFromNullable(anotherIndex.OptionalTokenIndex)
                    .MergeFromNullable(anotherIndex.TokenIndex);
                index.ExceptionOptionalTokenIndex = index.ExceptionOptionalTokenIndex
                    .MergeFromNullable(anotherIndex.ExceptionOptionalTokenIndex)
                    .MergeFromNullable(anotherIndex.ExceptionTokenIndex);
                index.OptionalReferenceIndex = index.OptionalReferenceIndex
                    .MergeFromNullable(anotherIndex.OptionalReferenceIndex)
                    .MergeFromNullable(anotherIndex.ReferenceIndex);
                index.ExceptionOptionalReferenceIndex = index.ExceptionOptionalReferenceIndex
                    .MergeFromNullable(anotherIndex.ExceptionOptionalReferenceIndex)
                    .MergeFromNullable(anotherIndex.ExceptionReferenceIndex);
            }
            return index;
        }

        // Добавление элементов в основную часть индекса исключений как из основной части индекса исключения,
        // так и из опциональной части индекса исключения
        public static ExpressionIndex MergeIntoMainPartFromException(this ExpressionIndex index,
            ExpressionIndex exceptionIndex)
        {
            if (exceptionIndex != null)
            {
                if (index == null)
                    index = new ExpressionIndex();
                index.ExceptionTokenIndex = index.ExceptionTokenIndex
                    .MergeFromNullable(exceptionIndex.ExceptionTokenIndex)
                    .MergeFromNullable(exceptionIndex.TokenIndex);
                index.ExceptionOptionalTokenIndex = index.ExceptionOptionalTokenIndex
                    .MergeFromNullable(exceptionIndex.ExceptionOptionalTokenIndex)
                    .MergeFromNullable(exceptionIndex.OptionalTokenIndex);
                index.ExceptionReferenceIndex = index.ExceptionReferenceIndex
                    .MergeFromNullable(exceptionIndex.ExceptionReferenceIndex)
                    .MergeFromNullable(exceptionIndex.ReferenceIndex);
                index.ExceptionOptionalReferenceIndex = index.ExceptionOptionalReferenceIndex
                    .MergeFromNullable(exceptionIndex.ExceptionOptionalReferenceIndex)
                    .MergeFromNullable(exceptionIndex.OptionalReferenceIndex);
            }
            return index;
        }

        public static ITokenExpressionIndex MergeFromNullable(
            this ITokenExpressionIndex destination, ITokenExpressionIndex source)
        {
            ITokenExpressionIndex result;
            if (destination != null)
            {
                if (source != null)
                    result = destination.MergeFrom(source);
                else
                    result = destination;
            }
            else
            {
                if (source != null)
                    result = source.Clone();
                else
                    result = null;
            }
            return result;
        }

        public static IReferenceExpressionIndex MergeFromNullable(
            this IReferenceExpressionIndex destination, IReferenceExpressionIndex source)
        {
            IReferenceExpressionIndex result;
            if (destination != null)
            {
                if (source != null)
                    result = destination.MergeFrom(source);
                else
                    result = destination;
            }
            else
            {
                if (source != null)
                    result = source.Clone();
                else
                    result = null;
            }
            return result;
        }
    }
}
