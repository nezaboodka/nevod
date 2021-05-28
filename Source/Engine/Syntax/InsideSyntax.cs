//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class InsideSyntax : Syntax
    {
        public Syntax Inner { get; }
        public Syntax Outer { get; }

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
