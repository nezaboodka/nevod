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
    public class PatternReferenceSyntax : Syntax
    {
        public PatternSyntax ReferencedPattern { get; internal set; }
        public string PatternName { get; }
        public ReadOnlyCollection<Syntax> ExtractionFromFields { get; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                int rangeStart = TextRange.Start;
                if (ExtractionFromFields.Count != 0)
                {
                    int rangeEnd = ExtractionFromFields[0].TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.AddForElements(ExtractionFromFields);
                    rangeStart = ExtractionFromFields[ExtractionFromFields.Count - 1].TextRange.End;
                }
                childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal PatternReferenceSyntax(PatternSyntax pattern, IList<Syntax> extractionFromFields)
        {
            ReferencedPattern = pattern;
            PatternName = pattern.FullName;
            ExtractionFromFields = new ReadOnlyCollection<Syntax>(extractionFromFields);
        }

        internal PatternReferenceSyntax(string patternName, IList<Syntax> extractionFromFields)
        {
            PatternName = patternName;
            ExtractionFromFields = new ReadOnlyCollection<Syntax>(extractionFromFields);
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitPatternReference(this);
        }
    }

    public class EmbeddedPatternReferenceSyntax : PatternReferenceSyntax
    {
        internal EmbeddedPatternReferenceSyntax(PatternSyntax pattern, IList<Syntax> extractionFromFields)
            : base(pattern, extractionFromFields)
        {
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitEmbeddedPatternReference(this);
        }
    }

    public partial class Syntax
    {
        public static Syntax[] EmptyExtractionList()
        {
            return Array.Empty<Syntax>();
        }

        public static PatternReferenceSyntax PatternReference(PatternSyntax pattern)
        {
            var result = new PatternReferenceSyntax(pattern, EmptyExtractionList());
            return result;
        }

        public static PatternReferenceSyntax PatternReference(PatternSyntax pattern,
            params Syntax[] extractionFromFields)
        {
            var result = new PatternReferenceSyntax(pattern, extractionFromFields);
            return result;
        }

        public static PatternReferenceSyntax PatternReference(PatternSyntax pattern,
            IList<Syntax> extractionFromFields)
        {
            var result = new PatternReferenceSyntax(pattern, extractionFromFields);
            return result;
        }

        public static PatternReferenceSyntax PatternReference(string name)
        {
            var result = new PatternReferenceSyntax(name, EmptyExtractionList());
            return result;
        }

        public static PatternReferenceSyntax PatternReference(string name, params Syntax[] extractionFromFields)
        {
            var result = new PatternReferenceSyntax(name, extractionFromFields);
            return result;
        }

        public static PatternReferenceSyntax PatternReference(string name, IList<Syntax> extractionFromFields)
        {
            var result = new PatternReferenceSyntax(name, extractionFromFields);
            return result;
        }

        internal static EmbeddedPatternReferenceSyntax EmbeddedPatternReference(PatternSyntax pattern,
            ReadOnlyCollection<Syntax> extractionFromFields)
        {
            var result = new EmbeddedPatternReferenceSyntax(pattern, extractionFromFields);
            return result;
        }
    }
}
