//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Nevod
{
    public class OutsideSyntax : Syntax
    {
        public Syntax Body { get; }
        public new Syntax Exception { get; }

        public override void CreateChildren(string text)
        {
            if (Children != null)
                return;
            var childrenBuilder = new ChildrenBuilder(text);
            int rangeStart = TextRange.Start;
            if (Body != null)
            {
                childrenBuilder.Add(Body);
                rangeStart = Body.TextRange.End;
            }
            if (Exception != null)
            {
                int rangeEnd = Exception.TextRange.Start;
                childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                childrenBuilder.Add(Exception);
                rangeStart = Exception.TextRange.End;
            }
            childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
            Children = childrenBuilder.GetChildren();
        }

        internal OutsideSyntax(Syntax body, Syntax exception)
        {
            Body = body;
            Exception = exception;
        }

        internal OutsideSyntax Update(Syntax body, Syntax exception)
        {
            OutsideSyntax result = this;
            if (body != Body || exception != Exception)
                result = Outside(body, exception);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitOutside(this);
        }
    }

    public partial class Syntax
    {
        public static OutsideSyntax Outside(Syntax body, Syntax exception)
        {
            var result = new OutsideSyntax(body, exception);
            return result;
        }
    }
}
