//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class SequenceExpression : MultiplaceExpression
    {
        public int LastNonOptionalPosition { get; private set; }

        public SequenceExpression(Syntax syntax, Expression[] elements)
            : base(syntax, elements)
        {
            LastNonOptionalPosition = Elements.Length - 1;
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            base.RefreshIsOptional();
            if (IsOptional)
                LastNonOptionalPosition = -1;
            else
            {
                LastNonOptionalPosition = Elements.Length - 1;
                while (LastNonOptionalPosition > 0 && Elements[LastNonOptionalPosition].IsOptional)
                    LastNonOptionalPosition--;
            }
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            SequenceCandidate result = candidateFactory.CreateSequenceCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitSequence(this);
        }
    }
}
