//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    internal sealed class TokenExpression : Expression
    {
        public TokenKind Kind { get; }
        public string Text { get; }
        public bool IsCaseSensitive { get; }
        public bool TextIsPrefix { get; }
        public TokenAttributes TokenAttributes { get; }

        public RootExpression RootExpression { get; set; }
        public int PatternId { get; set; }
        public VariationExpression ParentVariationWithExceptions { get; set; }
        public HavingExpression ParentConditionalHaving { get; set; }

        public TokenExpression(Syntax syntax, TokenKind kind, string text, bool isCaseSensitive, bool textIsPrefix,
            TokenAttributes tokenAttributes)
            : base(syntax)
        {
            Kind = kind;
            Text = text;
            IsCaseSensitive = isCaseSensitive;
            TextIsPrefix = textIsPrefix;
            TokenAttributes = tokenAttributes;
        }

        public TokenExpression(TokenKind kind)
            : base(syntax: null)
        {
            Kind = kind;
        }

        public RootCandidate CreateRootCandidate(SearchContext searchContext)
        {
            RootCandidate rootCandidate = (RootCandidate)RootExpression.CreateCandidate(searchContext,
                targetParentCandidate: null);
            return rootCandidate;
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            TokenCandidate result = candidateFactory.CreateTokenCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitToken(this);
        }
    }
}
