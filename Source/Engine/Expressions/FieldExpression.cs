//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class FieldExpression : Expression
    {
        public string Name { get; }
        public bool IsInternal { get; }
        public int FieldNumber { get; set; }

        public FieldExpression(Syntax syntax, string name, bool isInternal)
            : base(syntax)
        {
            Name = name;
            IsInternal = isInternal;
            FieldNumber = -1;
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            throw new InvalidOperationException();
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitField(this);
        }
    }
}
