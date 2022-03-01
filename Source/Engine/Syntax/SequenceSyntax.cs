//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    public class SequenceSyntax : Syntax
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

        internal SequenceSyntax(IList<Syntax> elements) : this(elements, checkCanReduce: true)
        {
        }

        internal SequenceSyntax(IList<Syntax> elements, bool checkCanReduce)
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
