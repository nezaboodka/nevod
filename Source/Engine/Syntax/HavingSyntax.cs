//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    public class HavingSyntax : Syntax
    {
        public Syntax Outer { get; }
        public Syntax Inner { get; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            int rangeStart = TextRange.Start;
            if (Outer != null)
            {
                children.Add(Outer);
                rangeStart = Outer.TextRange.End;
            }
            if (Inner != null)
            {
                int rangeEnd = Inner.TextRange.Start;
                SyntaxUtils.CreateChildrenForRange(rangeStart, rangeEnd, children, scanner);
                children.Add(Inner);
                rangeStart = Inner.TextRange.End;
            }
            SyntaxUtils.CreateChildrenForRange(rangeStart, TextRange.End, children, scanner);
            Children = children.AsReadOnly();
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
