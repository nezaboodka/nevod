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
    public class SequenceSyntax : Syntax
    {
        public ReadOnlyCollection<Syntax> Elements { get; }

        internal SequenceSyntax(IList<Syntax> elements)
        {
            Elements = new ReadOnlyCollection<Syntax>(elements);
        }

        internal SequenceSyntax Update(ReadOnlyCollection<Syntax> elements)
        {
            SequenceSyntax result = this;
            if (elements != Elements)
                result = Sequence(elements);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitSequence(this);
        }
    }

    public partial class Syntax
    {
        public static SequenceSyntax Sequence(Syntax first, Syntax second)
        {
            var result = new SequenceSyntax(new Syntax[] { first, second });
            return result;
        }

        public static SequenceSyntax Sequence(params Syntax[] elements)
        {
            var result = new SequenceSyntax(elements);
            return result;
        }

        public static SequenceSyntax Sequence(IList<Syntax> elements)
        {
            var result = new SequenceSyntax(elements);
            return result;
        }
    }
}
