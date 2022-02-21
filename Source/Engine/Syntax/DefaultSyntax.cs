﻿//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Nevod
{
    public class DefaultSyntax : Syntax
    {
        internal DefaultSyntax()
        {
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitDefault(this);
        }
    }

    public partial class Syntax
    {
        public static DefaultSyntax Empty()
        {
            var result = new DefaultSyntax();
            return result;
        }
    }
}
