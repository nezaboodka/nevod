//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Nevod
{
    public class ExtractionSyntax : Syntax
    {
        public string FieldName { get; }
        public Syntax Body { get; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                if (Body != null)
                {
                    childrenBuilder.AddInsideRange(TextRange.Start, Body.TextRange.Start);
                    childrenBuilder.Add(Body);
                    childrenBuilder.AddInsideRange(Body.TextRange.End, TextRange.End);
                }
                else
                    childrenBuilder.AddInsideRange(TextRange);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal ExtractionSyntax(string fieldName, Syntax body)
        {
            FieldName = fieldName;
            Body = body;
        }

        internal ExtractionSyntax Update(string fieldName, Syntax body)
        {
            ExtractionSyntax result = this;
            if (fieldName != FieldName || body != Body)
                result = Extraction(fieldName, body);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitExtraction(this);
        }
    }

    public partial class Syntax
    {
        public static ExtractionSyntax Extraction(FieldSyntax field, Syntax body)
        {
            var result = new ExtractionSyntax(field.Name, body);
            return result;
        }

        public static ExtractionSyntax Extraction(FieldSyntax field)
        {
            var result = new ExtractionSyntax(field.Name, null);
            return result;
        }

        internal static ExtractionSyntax Extraction(string fieldName, Syntax body)
        {
            var result = new ExtractionSyntax(fieldName, body);
            return result;
        }
    }
}
