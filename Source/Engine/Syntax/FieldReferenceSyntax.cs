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
    public class FieldReferenceSyntax : Syntax
    {
        public string FieldName { get; }

        internal FieldReferenceSyntax(string fieldName)
        {
            FieldName = fieldName;
        }

        internal FieldReferenceSyntax Update(string fieldName)
        {
            FieldReferenceSyntax result = this;
            if (fieldName != FieldName)
                result = FieldReference(fieldName);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitFieldReference(this);
        }
    }

    public partial class Syntax
    {
        public static FieldReferenceSyntax FieldReference(FieldSyntax field)
        {
            var result = new FieldReferenceSyntax(field.Name);
            return result;
        }

        public static FieldReferenceSyntax FieldReference(string fieldName)
        {
            var result = new FieldReferenceSyntax(fieldName);
            return result;
        }
    }
}
