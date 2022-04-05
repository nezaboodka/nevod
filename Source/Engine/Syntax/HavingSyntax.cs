//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class HavingSyntax : Syntax
    {
        public Syntax Outer { get; }
        public Syntax Inner { get; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                int rangeStart = TextRange.Start;
                if (Outer != null)
                {
                    int rangeEnd = Outer.TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.Add(Outer);
                    rangeStart = Outer.TextRange.End;
                }
                if (Inner != null)
                {
                    int rangeEnd = Inner.TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.Add(Inner);
                    rangeStart = Inner.TextRange.End;
                }
                childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal HavingSyntax(Syntax outer, Syntax inner)
        {
            Outer = outer;
            Inner = inner;
        }

        internal HavingSyntax Update(Syntax outer, Syntax inner)
        {
            HavingSyntax result = this;
            if (outer != Outer || inner != Inner)
                result = Having(outer, inner);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitHaving(this);
        }
    }

    public partial class Syntax
    {
        public static HavingSyntax Having(Syntax outer, Syntax inner)
        {
            var result = new HavingSyntax(outer, inner);
            return result;
        }
    }
}
