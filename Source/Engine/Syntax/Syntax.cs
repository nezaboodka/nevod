//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    public abstract partial class Syntax
    {
        public TextRange TextRange { get; set; }
        public ReadOnlyCollection<Syntax> Children { get; set; }

        public override string ToString()
        {
            string result = SyntaxStringBuilder.SyntaxToString(this);
            return result;
        }

        public virtual void CreateChildren(string text)
        {
            if (Children == null)
                Children = Array.AsReadOnly(Array.Empty<Syntax>());
        }

        internal virtual bool CanReduce => false;

        internal virtual Syntax Reduce()
        {
            if (CanReduce)
                throw new InvalidOperationException("Reducible must override Reduce");
            return this;
        }

        protected internal virtual Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitSyntax(this);
        }
    }
}
