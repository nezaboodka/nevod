//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal abstract class RootExpression : CompoundExpression
    {
        public int Id { get; }

        public RootExpression(Syntax syntax, int id)
            : base(syntax)
        {
            Id = id;
        }
    }
}
