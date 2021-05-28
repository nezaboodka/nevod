//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal abstract class MultiplaceExpression : CompoundExpression
    {
        public Expression[] Elements { get; }
        public int NonOptionalElementCount { get; private set; }

        public MultiplaceExpression(Syntax syntax, Expression[] elements)
            : base(syntax)
        {
            Elements = elements;
            for (int i = 0; i < Elements.Length; i++)
            {
                Expression element = Elements[i];
                element.SetParentExpression(this, i);
            }
        }

        public override void RefreshIsOptional()
        {
            int nonOptionalElementCount = 0;
            for (int i = 0; i < Elements.Length; i++)
            {
                Expression element = Elements[i];
                if (element != null && !element.IsOptional)
                    nonOptionalElementCount++;
            }
            NonOptionalElementCount = nonOptionalElementCount;
            IsOptional = NonOptionalElementCount == 0;
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }
    }
}
