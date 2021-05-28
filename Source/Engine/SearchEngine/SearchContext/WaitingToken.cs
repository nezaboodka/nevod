//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Nezaboodka.Text.Parsing;


namespace Nezaboodka.Nevod
{
    internal class WaitingToken
    {
        public TokenExpression TokenExpression;
        public AnySpanCandidate Candidate;
        public WaitingTokenList Container;
        public bool IsException;
        public bool IsDisposed;

        public WaitingToken(TokenExpression tokenExpression, AnySpanCandidate candidate, bool isException)
        {
            TokenExpression = tokenExpression;
            Candidate = candidate;
            IsException = isException;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Container.DisposedCount++;
            }
        }
    }

    internal class WaitingTokenList : List<WaitingToken>
    {
        public int DisposedCount;

        public bool IsEmpty()
        {
            return DisposedCount == Count;
        }

        public bool TryRemoveDisposedCandidates()
        {
            if (DisposedCount == Count)
                Clear();
            else
            {
                int i = 0;
                int j = 0;
                int count = Count;
                while (i < count)
                {
                    if (!this[i].IsDisposed)
                    {
                        if (i != j)
                            this[j] = this[i];
                        j++;
                    }
                    i++;
                }
                RemoveRange(j, i - j);
            }
            DisposedCount = 0;
            return DisposedCount == 0;
        }

        public void RejectAll()
        {
            for (int i = Count - 1; i >= 0; i--)
                this[i].Candidate.RejectTarget();
            Clear();
            DisposedCount = 0;
        }
    }
}
