//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class ExtractionExpression : CompoundExpression
    {
        public int FieldNumber;
        public Expression Body;

        public ExtractionExpression(Syntax syntax, int fieldNumber, Expression body)
            : base(syntax)
        {
            FieldNumber = fieldNumber;
            Body = body;
            if (Body != null)
                Body.SetParentExpression(this, BodyPosition);
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            if (Body != null)
            {
                IsOptional = Body.IsOptional;
                if (IsOptional && ParentExpression != null)
                    ParentExpression.RefreshIsOptional();
            }
        }

        public const int BodyPosition = 0;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            ExtractionCandidate result = candidateFactory.CreateExtractionCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitExtraction(this);
        }
    }
}
