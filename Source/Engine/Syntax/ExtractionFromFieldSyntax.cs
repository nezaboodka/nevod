//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class ExtractionFromFieldSyntax : Syntax
    {
        public string FieldName { get; }
        public string FromFieldName { get; }

        internal ExtractionFromFieldSyntax(string fieldName, string fromFieldName)
        {
            FieldName = fieldName;
            FromFieldName = fromFieldName;
        }

        internal ExtractionFromFieldSyntax Update(string fieldName, string fromFieldName)
        {
            ExtractionFromFieldSyntax result = this;
            if (fieldName != FieldName || fromFieldName != FromFieldName)
                result = ExtractionFromField(fieldName, fromFieldName);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitExtractionFromField(this);
        }
    }

    public partial class Syntax
    {
        public static ExtractionFromFieldSyntax ExtractionFromField(FieldSyntax field, string fromFieldName)
        {
            var result = new ExtractionFromFieldSyntax(field.Name, fromFieldName);
            return result;
        }

        internal static ExtractionFromFieldSyntax ExtractionFromField(string fieldName, string fromFieldName)
        {
            var result = new ExtractionFromFieldSyntax(fieldName, fromFieldName);
            return result;
        }
    }
}
