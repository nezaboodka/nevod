//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class ConjunctionSyntax : Syntax
    {
        public ReadOnlyCollection<Syntax> Elements { get; }

        internal ConjunctionSyntax(IList<Syntax> elements)
        {
            Elements = new ReadOnlyCollection<Syntax>(elements);
        }

        internal ConjunctionSyntax Update(ReadOnlyCollection<Syntax> elements)
        {
            ConjunctionSyntax result = this;
            if (elements != Elements)
                result = Conjunction(elements);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitConjunction(this);
        }
    }

    public partial class Syntax
    {
        public static ConjunctionSyntax Conjunction(Syntax first, Syntax second)
        {
            var result = new ConjunctionSyntax(new Syntax[] { first, second });
            return result;
        }

        public static ConjunctionSyntax Conjunction(params Syntax[] elements)
        {
            var result = new ConjunctionSyntax(elements);
            return result;
        }

        public static ConjunctionSyntax Conjunction(IList<Syntax> elements)
        {
            var result = new ConjunctionSyntax(elements);
            return result;
        }
    }
}
