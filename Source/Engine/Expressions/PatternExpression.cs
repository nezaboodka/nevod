//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class PatternExpression : RootExpression
    {
        public PatternSyntax PatternSyntax => (PatternSyntax)Syntax;
        public string Name { get; }
        public FieldExpression[] Fields { get; }
        public Expression Body { get; private set; }
        public bool IsAnonymous => (Name == null);
        public bool HasReferences { get; set; }

        public PatternExpression(PatternSyntax syntax, int id, string name, FieldExpression[] fields, Expression body)
            : base(syntax, id)
        {
            Name = name;
            Fields = fields;
            Body = body;
            Body.SetParentExpression(this, BodyPosition);
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            IsOptional = Body.IsOptional;
        }

        public const int BodyPosition = 0;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            PatternCandidate result = candidateFactory.CreatePatternCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitPattern(this);
        }
    }
}
