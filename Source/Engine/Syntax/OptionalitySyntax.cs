﻿//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class OptionalitySyntax : Syntax
    {
        private bool fCanReduce;

        public Syntax Body { get; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            if (Body != null)
            {
                SyntaxUtils.CreateChildrenForRange(TextRange.Start, Body.TextRange.Start, children, scanner);
                children.Add(Body);
                SyntaxUtils.CreateChildrenForRange(Body.TextRange.End, TextRange.End, children, scanner);
            }
            else
                SyntaxUtils.CreateChildrenForRange(TextRange, children, scanner);
            Children = children.AsReadOnly();
        }

        internal override bool CanReduce => fCanReduce;

        internal OptionalitySyntax(Syntax body)
            : this(body, checkCanReduce: true)
        {
            Body = body;
        }

        internal OptionalitySyntax(Syntax body, bool checkCanReduce)
        {
            Body = body;
            if (checkCanReduce)
                fCanReduce = Body != null
                && (Body is OptionalitySyntax
                    || Body is SpanSyntax m && m.Elements.Count == 1 &&
                        ((RepetitionSyntax)m.Elements[0]).RepetitionRange.IsZeroPlusOrOnePlus()
                    || Body is VariationSyntax v
                        && v.Elements.Any(x => x is RepetitionSyntax xr && xr.RepetitionRange.IsZeroPlusOrOnePlus()));
        }

        internal override Syntax Reduce()
        {
            Syntax result = this;
            if (Body is OptionalitySyntax)
                result = Body;
            else if (Body is SpanSyntax m && m.Elements.Count == 1)
            {
                Range r = ((RepetitionSyntax)m.Elements[0]).RepetitionRange;
                if (r.IsZeroPlus())
                    result = Body;
                else if (r.IsOnePlus())
                    result = Syntax.Span(Syntax.Repetition(Range.ZeroPlus(), Body));
            }
            return result;
        }

        internal OptionalitySyntax Update(Syntax body)
        {
            OptionalitySyntax result = this;
            if (body != Body)
                result = Optionality(body);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitOptionality(this);
        }
    }

    public partial class Syntax
    {
        public static OptionalitySyntax Optionality(Syntax body)
        {
            var result = new OptionalitySyntax(body);
            return result;
        }
    }
}
