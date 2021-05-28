//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class ExceptionStubCandidate : RootCandidate
    {
        public ExceptionStubCandidate(CompoundExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            throw new InvalidOperationException();
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            throw new InvalidOperationException();
        }

        // Internal

        protected override void OnFinalMatch()
        {
            throw new InvalidOperationException();
        }
    }
}
