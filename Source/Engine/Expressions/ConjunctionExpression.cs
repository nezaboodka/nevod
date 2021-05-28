//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class ConjunctionExpression : MultiplaceExpression
    {
        public ConjunctionExpression(Syntax syntax, Expression[] elements)
            : base(syntax, elements)
        {
            RefreshIsOptional();
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            ConjunctionCandidate result = candidateFactory.CreateConjunctionCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitConjunction(this);
        }
    }
}
