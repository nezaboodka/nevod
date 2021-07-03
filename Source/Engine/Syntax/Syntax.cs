//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nezaboodka.Text;

namespace Nezaboodka.Nevod
{
    public abstract partial class Syntax
    {
        public override string ToString()
        {
            string result = SyntaxStringBuilder.SyntaxToString(this);
            return result;
        }

        public Slice TextSlice { get; set; }
        public int StartLine { get; set; }
        public int StartCharacter { get; set; } 
        public int EndLine { get; set; }
        public int EndCharacter { get; set; }
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
