//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    public class FieldSyntax : Syntax
    {
        public string Name { get; }
        public bool IsInternal { get; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            SyntaxUtils.CreateChildrenForRange(TextRange, children, scanner);
            Children = children.AsReadOnly();
        }

        internal FieldSyntax(string name, bool isInternal)
        {
            Name = name;
            IsInternal = isInternal;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitField(this);
        }
    }

    public partial class Syntax
    {
        public static FieldSyntax Field(string name)
        {
            var result = new FieldSyntax(name, false);
            return result;
        }

        public static FieldSyntax Field(string name, bool isInternal)
        {
            var result = new FieldSyntax(name, isInternal);
            return result;
        }
    }
}
