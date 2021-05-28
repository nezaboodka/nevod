//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class FieldReferenceExpression : CompoundExpression
    {
        public int FieldNumber { get; }
        public Expression Body { get; private set; }

        public FieldReferenceExpression(Syntax syntax, int fieldNumber, Expression body)
            : base(syntax)
        {
            FieldNumber = fieldNumber;
            Body = body;
            Body.SetParentExpression(this, BodyPosition);
        }

        public override void RefreshIsOptional()
        {
            // do nothing
        }

        public const int BodyPosition = 0;
        public const int TextPosition = 1;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            FieldReferenceCandidate result = candidateFactory.CreateFieldReferenceCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitFieldReference(this);
        }
    }
}
