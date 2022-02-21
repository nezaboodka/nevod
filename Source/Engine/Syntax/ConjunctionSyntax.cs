//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    public class ConjunctionSyntax : Syntax
    {
        public ReadOnlyCollection<Syntax> Elements { get; }

        public bool IsSingleElement() => Elements.Count == 1;

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            int rangeStart = TextRange.Start;
            if (Elements.Count != 0)
            {
                SyntaxUtils.CreateChildrenForElements(Elements, children, scanner);
                rangeStart = Elements[^1].TextRange.End;
            }
            SyntaxUtils.CreateChildrenForRange(rangeStart, TextRange.End, children, scanner);
            Children = children.AsReadOnly();
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
