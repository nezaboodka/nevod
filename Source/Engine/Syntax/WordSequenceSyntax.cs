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
    public class WordSequenceSyntax : Syntax
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

        internal override bool CanReduce => true;

        internal WordSequenceSyntax(IList<Syntax> elements)
        {
            Elements = new ReadOnlyCollection<Syntax>(elements);
        }

        internal override Syntax Reduce()
        {
            Syntax result;
            if (IsSingleElement())
                result = Elements[0];
            else
            {
                var sequenceElements = new List<Syntax>();
                Syntax wordBreaks = Syntax.Repetition(new Range(0, Range.Max), Syntax.StandardPattern.WordBreak);
                for (int i = 0, n = Elements.Count; i < n; i++)
                {
                    sequenceElements.Add(Elements[i]);
                    if (i < n - 1)
                        sequenceElements.Add(wordBreaks);
                } 
                result = Syntax.Sequence(sequenceElements);
            }
            return result;
        }

        internal WordSequenceSyntax Update(ReadOnlyCollection<Syntax> elements)
        {
            WordSequenceSyntax result = this;
            if (elements != Elements)
                result = WordSequence(elements);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitWordSequence(this);
        }
    }

    public partial class Syntax
    {
        public static WordSequenceSyntax WordSequence(Syntax first, Syntax second)
        {
            var result = new WordSequenceSyntax(new Syntax[] { first, second });
            return result;
        }

        public static WordSequenceSyntax WordSequence(params Syntax[] elements)
        {
            var result = new WordSequenceSyntax(elements);
            return result;
        }

        public static WordSequenceSyntax WordSequence(IList<Syntax> elements)
        {
            var result = new WordSequenceSyntax(elements);
            return result;
        }
    }
}
