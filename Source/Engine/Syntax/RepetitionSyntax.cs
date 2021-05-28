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
    public class RepetitionSyntax : Syntax
    {
        private bool fCanReduce;

        public Range RepetitionRange { get; }
        public Syntax Body { get; }

        public bool IsBodyVariationWhereAnyElementIsSpanWithSingleElementWithZeroOrOnePlusRepetition() =>
            Body is VariationSyntax v && v.AnyElementIsSpanWithSingleElementWithZeroOrOnePlusRepetition();

        internal override bool CanReduce => fCanReduce;

        internal RepetitionSyntax(Range range, Syntax body)
            : this(range, body, checkCanReduce: true)
        {
            RepetitionRange = range;
            Body = body;
        }

        internal RepetitionSyntax(Range range, Syntax body, bool checkCanReduce)
        {
            RepetitionRange = range;
            Body = body;
            if (checkCanReduce)
                fCanReduce = IsBodyVariationWhereAnyElementIsSpanWithSingleElementWithZeroOrOnePlusRepetition();
        }

        internal override Syntax Reduce()
        {
            Syntax result = this;
            if (Body is VariationSyntax v)
            {
                List<Syntax> newElements = null;
                for (int i = 0, n = v.Elements.Count; i < n; i++)
                {
                    Syntax element = v.Elements[i];
                    if (element != null)
                    {
                        if (element.CanReduce)
                            element = element.Reduce();
                        SpanSyntax elementSpan = element as SpanSyntax;
                        if (!object.ReferenceEquals(element, v.Elements[i])
                            || elementSpan != null && elementSpan.IsSingleElementWithZeroPlusOrOnePlusRepetition())
                        {
                            if (newElements == null)
                            {
                                newElements = new List<Syntax>(n);
                                for (int j = 0; j < i; j++)
                                    newElements.Add(v.Elements[j]);
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
                VariationSyntax newBodyVariation;
                if (newElements == null)
                    newBodyVariation = v;
                else
                    newBodyVariation = new VariationSyntax(new ReadOnlyCollection<Syntax>(newElements),
                        checkCanReduce: false);
                result = new RepetitionSyntax(this.RepetitionRange, newBodyVariation, checkCanReduce: false);
            }
            return result;
        }

        internal RepetitionSyntax Update(Syntax body, Range repetitionRange)
        {
            RepetitionSyntax result = this;
            if (body != Body || !RepetitionRange.Equals(repetitionRange))
                result = Repetition(repetitionRange, body);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitRepetition(this);
        }
    }

    public partial class Syntax
    {
        public static RepetitionSyntax Repetition(Range range, Syntax body)
        {
            var result = new RepetitionSyntax(range, body);
            return result;
        }

        public static RepetitionSyntax Repetition(int lowBound, int highBound, Syntax body)
        {
            var result = new RepetitionSyntax(new Range(lowBound, highBound), body);
            return result;
        }
    }
}
