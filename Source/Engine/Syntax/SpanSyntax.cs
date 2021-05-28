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
    public class SpanSyntax : Syntax
    {
        protected bool fCanReduce;

        public ReadOnlyCollection<Syntax> Elements { get; }

        public bool IsSingleElementWithOneRepetition() =>
            Elements.Count == 1 && Elements[0] is RepetitionSyntax r && r.RepetitionRange.IsOne();

        public bool AnyElementIsOneRepetition() =>
            Elements.Any(x => x is RepetitionSyntax r && r.RepetitionRange.IsOne());

        public bool IsSingleElementWithZeroPlusOrOnePlusRepetition() =>
            Elements.Count == 1 && Elements[0] is RepetitionSyntax r && r.RepetitionRange.IsZeroPlusOrOnePlus();

        public bool AnyElementIsZeroPlusOrOnePlusRepetition() =>
            Elements.Any(x => x is RepetitionSyntax r && r.RepetitionRange.IsZeroPlusOrOnePlus());

        public bool AnyElementIsZeroPlusOrOnePlusRepetitionOfSpanWithSingleElementWithZeroPlusOrOnePlusRepetition() =>
            Elements.Any(x => x is RepetitionSyntax r && r.Body is SpanSyntax s
                && s.IsSingleElementWithZeroPlusOrOnePlusRepetition());

        internal override bool CanReduce => fCanReduce;

        internal SpanSyntax(IList<Syntax> elements)
            : this(elements, checkCanReduce: true)
        {
        }

        internal SpanSyntax(IList<Syntax> elements, bool checkCanReduce)
        {
            Elements = new ReadOnlyCollection<Syntax>(elements);
            if (checkCanReduce)
                fCanReduce = IsSingleElementWithOneRepetition() || AnyElementIsOneRepetition() ||
                    AnyElementIsZeroPlusOrOnePlusRepetitionOfSpanWithSingleElementWithZeroPlusOrOnePlusRepetition();
        }

        internal override Syntax Reduce()
        {
            Syntax result = this;
            if (IsSingleElementWithOneRepetition())
                result = Elements[0];
            else // if (AnyElementIsOneRepetition()) || AnyElementIsZeroPlusOrOnePlusRepetitionOfSpanWithSingleElementWithZeroPlusOrOnePlusRepetition()
            {
                List<Syntax> newElements = null;
                for (int i = 0, n = Elements.Count; i < n; i++)
                {
                    Syntax element = Elements[i];
                    if (element != null)
                    {
                        if (element.CanReduce)
                            element = element.Reduce();
                        SpanSyntax elementSpan = element as SpanSyntax;
                        if (!object.ReferenceEquals(element, Elements[i]) ||
                            elementSpan != null && (elementSpan.IsSingleElementWithOneRepetition()
                                || elementSpan.IsSingleElementWithZeroPlusOrOnePlusRepetition()))
                        {
                            if (newElements == null)
                            {
                                newElements = new List<Syntax>(n);
                                for (int j = 0; j < i; j++)
                                    newElements.Add(Elements[j]);
                            }
                            if (elementSpan != null)
                            {
                                var repetition = (RepetitionSyntax)elementSpan.Elements[0];
                                newElements.Add(repetition.Body);
                            }
                            else
                                newElements.Add(element);
                        }
                        else if (newElements != null)
                            newElements.Add(element);
                    }
                }
                if (newElements == null)
                    result = new SpanSyntax(this.Elements, checkCanReduce: false);
                else
                    result = new SpanSyntax(new ReadOnlyCollection<Syntax>(newElements),
                        checkCanReduce: false);
            }
            return result;
        }

        internal SpanSyntax Update(ReadOnlyCollection<Syntax> elements)
        {
            SpanSyntax result = this;
            if (elements != Elements)
                result = Span(elements);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitSpan(this);
        }
    }

    public partial class Syntax
    {
        public static SpanSyntax Span(Syntax element)
        {
            var result = new SpanSyntax(new Syntax[] { element });
            return result;
        }

        public static SpanSyntax Span(params Syntax[] elements)
        {
            var result = new SpanSyntax(elements);
            return result;
        }

        public static SpanSyntax Span(IList<Syntax> elements)
        {
            var result = new SpanSyntax(elements);
            return result;
        }
    }
}
