//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    public class ExceptionSyntax : Syntax
    {
        public Syntax Body { get; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var children = new List<Syntax>();
            var scanner = new Scanner(text);
            if (Body != null)
            {
                SyntaxUtils.CreateChildrenForRange(TextRange.Start, Body.TextRange.Start, children, scanner);
                children.Add(Body);
                SyntaxUtils.CreateChildrenForRange(Body.TextRange.End, TextRange.End, children, scanner);
            }
            else
                SyntaxUtils.CreateChildrenForRange(TextRange, children, scanner);
            Children = children.AsReadOnly();
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
