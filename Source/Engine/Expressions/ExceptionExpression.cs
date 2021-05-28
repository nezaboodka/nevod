//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal sealed class ExceptionExpression : RootExpression
    {
        public Expression Body { get; private set; }
        public RootExpression RootExpression { get; set; }

        public ExceptionExpression(Syntax syntax, int id, Expression body)
            : base(syntax, id)
        {
            Body = body;
            body.SetParentExpression(this, BodyPosition);
        }

        public override void RefreshIsOptional()
        {
            IsOptional = Body.IsOptional;
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }

        public const int BodyPosition = 0;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            ExceptionCandidate result = candidateFactory.CreateExceptionCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitException(this);
        }
    }
}
