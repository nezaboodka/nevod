﻿//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class ExceptionSyntax : Syntax
    {
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

        internal ExceptionSyntax(Syntax body)
        {
            Body = body;
        }

        internal ExceptionSyntax Update(Syntax body)
        {
            ExceptionSyntax result = this;
            if (body != Body)
                result = Exception(body);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitException(this);
        }
    }

    public partial class Syntax
    {
        public static ExceptionSyntax Exception(Syntax body)
        {
            var result = new ExceptionSyntax(body);
            return result;
        }
    }
}
