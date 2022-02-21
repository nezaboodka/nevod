//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class VariationSyntax : Syntax
    {
        protected bool fCanReduce;

        public ReadOnlyCollection<Syntax> Elements { get; }
        public bool HasExceptions { get; }

        public bool IsSingleElement() => (Elements.Count == 1);

        public bool AnyElementIsVariationWithoutExceptions() =>
            Elements.Any(x => x is VariationSyntax v && !v.HasExceptions);

        public bool AnyElementIsSpanWithSingleElementWithZeroOrOnePlusRepetition() =>
            Elements.Any(x => x is SpanSyntax s && s.IsSingleElementWithZeroPlusOrOnePlusRepetition());

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            int rangeStart = TextRange.Start;
            if (Elements.Count != 0)
            {
                int rangeEnd = Elements[0].TextRange.Start;
                SyntaxUtils.CreateChildrenForRange(rangeStart, rangeEnd, children, scanner);
                SyntaxUtils.CreateChildrenForElements(Elements, children, scanner);
                rangeStart = Elements[^1].TextRange.End;
            }
            SyntaxUtils.CreateChildrenForRange(rangeStart, TextRange.End, children, scanner);
            Children = children.AsReadOnly();
        }

        internal override bool CanReduce => fCanReduce;

        internal VariationSyntax(IList<Syntax> elements)
            : this(elements, checkCanReduce: true)
        {
        }

        internal VariationSyntax(IList<Syntax> elements, bool checkCanReduce)
        {
            Elements = new ReadOnlyCollection<Syntax>(elements);
            HasExceptions = elements.Any(x => x is ExceptionSyntax);
            if (checkCanReduce)
                fCanReduce = IsSingleElement() || AnyElementIsVariationWithoutExceptions();
        }

        internal override Syntax Reduce()
        {
            Syntax result = this;
            if (IsSingleElement())
                result = Elements[0];
            else // AnyElementIsVariationWithoutExceptions()
            {
                List<Syntax> newElements = null;
                for (int i = 0, n = Elements.Count; i < n; i++)
                {
                    Syntax element = Elements[i];
                    if (element != null)
                    {
                        if (element.CanReduce)
                            element = element.Reduce();
                        VariationSyntax elementVariation = element as VariationSyntax;
                        if (!object.ReferenceEquals(element, Elements[i]) ||
                            elementVariation != null && !elementVariation.HasExceptions)
                        {
                            if (newElements == null)
                            {
                                newElements = new List<Syntax>(n);
                                for (int j = 0; j < i; j++)
                                    newElements.Add(Elements[j]);
                            }
                            if (elementVariation != null)
                            {
                                ReadOnlyCollection<Syntax> subElements = elementVariation.Elements;
                                for (int j = 0; j < subElements.Count; j++)
                                {
                                    Syntax subElement = subElements[j];
                                    newElements.Add(subElement);
                                }
                            }
                            else
                                newElements.Add(element);
                        }
                        else if (newElements != null)
                            newElements.Add(element);
                    }
                }
                if (newElements == null)
                    result = new VariationSyntax(this.Elements, checkCanReduce: false);
                else
                    result = new VariationSyntax(new ReadOnlyCollection<Syntax>(newElements),
                        checkCanReduce: false);
            }
            return result;
        }

        internal VariationSyntax Update(ReadOnlyCollection<Syntax> elements)
        {
            VariationSyntax result = this;
            if (elements != Elements)
                result = Variation(elements);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitVariation(this);
        }
    }

    public partial class Syntax
    {
        public static VariationSyntax Variation(Syntax element)
        {
            var result = new VariationSyntax(new Syntax[] { element });
            return result;
        }

        public static VariationSyntax Variation(params Syntax[] elements)
        {
            var result = new VariationSyntax(elements);
            return result;
        }

        public static VariationSyntax Variation(IList<Syntax> elements)
        {
            var result = new VariationSyntax(elements);
            return result;
        }
    }
}
