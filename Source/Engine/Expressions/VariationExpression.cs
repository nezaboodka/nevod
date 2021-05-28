//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class VariationExpression : MultiplaceExpression
    {
        public bool HasExceptions { get; private set; }
        public int VariationWithExceptionsId { get; set; }
        public VariationExpression ParentVariationWithExceptions { get; set; }

        public VariationExpression(Syntax syntax, Expression[] elements)
            : base(syntax, elements)
        {
            HasExceptions = Array.Exists(Elements, x => x is ExceptionExpression);
            RefreshIsOptional();
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            VariationCandidate result = candidateFactory.CreateVariationCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitVariation(this);
        }
    }
}
