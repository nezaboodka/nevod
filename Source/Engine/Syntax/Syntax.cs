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
    public abstract partial class Syntax
    {
        public TextRange TextRange { get; set; } 
        
        public override string ToString()
        {
            string result = SyntaxStringBuilder.SyntaxToString(this);
            return result;
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
