//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal sealed class RepetitionExpression : CompoundExpression
    {
        public Range RepetitionRange { get; }
        public Expression Body { get; }

        public RepetitionExpression(Syntax syntax, Range repetitionRange, Expression body)
            : base(syntax)
        {
            RepetitionRange = repetitionRange;
            Body = body;
            Body.SetParentExpression(this, BodyPosition);
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            IsOptional = (RepetitionRange.LowBound == 0);
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }

        public const int BodyPosition = 0;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            RepetitionCandidate result = candidateFactory.CreateRepetitionCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitRepetition(this);
        }
    }
}
