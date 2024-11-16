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
    public enum Separator
    {
        Blanks = 0,
        WordBreaks = 1,
    }

    public class WordSeparatorSyntax : Syntax
    {
        public Separator Separator { get; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                childrenBuilder.AddInsideRange(TextRange);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal override bool CanReduce => true;

        internal WordSeparatorSyntax(Separator separator)
        {
            Separator = separator;
        }

        internal override Syntax Reduce()
        {
            Syntax result;
            switch (Separator)
            {
                case Separator.Blanks:
                {
                    result = Syntax.Repetition(new Range(0, Range.Max), Syntax.StandardPattern.Blank);
                    break;
                }
                case Separator.WordBreaks:
                default:
                {
                    result = Syntax.Repetition(new Range(0, Range.Max), Syntax.StandardPattern.WordBreak);
                    break;
                }
            }
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitWordSeparator(this);
        }
    }

    public partial class Syntax
    {
        public static WordSeparatorSyntax WordSeparator(Separator separator)
        {
            var result = new WordSeparatorSyntax(separator);
            return result;
        }
    }
}
