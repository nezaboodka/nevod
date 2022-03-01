//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Nevod
{
    public class InsideSyntax : Syntax
    {
        public Syntax Inner { get; }
        public Syntax Outer { get; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                int rangeStart = TextRange.Start;
                if (Inner != null)
                {
                    childrenBuilder.Add(Inner);
                    rangeStart = Inner.TextRange.End;
                }
                if (Outer != null)
                {
                    int rangeEnd = Outer.TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.Add(Outer);
                    rangeStart = Outer.TextRange.End;
                }
                childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal InsideSyntax(Syntax inner, Syntax outer)
        {
            Inner = inner;
            Outer = outer;
        }

        internal InsideSyntax Update(Syntax inner, Syntax outer)
        {
            InsideSyntax result = this;
            if (inner != Inner || outer != Outer)
                result = Inside(inner, outer);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitInside(this);
        }
    }

    public partial class Syntax
    {
        public static InsideSyntax Inside(Syntax inner, Syntax outer)
        {
            var result = new InsideSyntax(inner, outer);
            return result;
        }
    }
}
