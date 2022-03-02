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

        public bool IsSingleElement() => Elements.Count == 1;

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                int rangeStart = TextRange.Start;
                if (Elements.Count != 0)
                {
                    childrenBuilder.AddForElements(Elements);
                    rangeStart = Elements[Elements.Count - 1].TextRange.End;
                }
                childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal override bool CanReduce { get; }

        internal ConjunctionSyntax(IList<Syntax> elements) : this(elements, checkCanReduce: true)
        {
        }

        internal ConjunctionSyntax(IList<Syntax> elements, bool checkCanReduce)
        {
            Elements = new ReadOnlyCollection<Syntax>(elements);
            if (checkCanReduce)
                CanReduce = IsSingleElement();
        }

        internal override Syntax Reduce()
        {
            Syntax result = this;
            if (IsSingleElement())
                result = Elements[0];
            return result;
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
