//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal abstract class Candidate : MatchingEventObserver
    {
        protected RootCandidate fRootCandidate; // закэшированная ссылка на корень
        protected RejectionTargetCandidate fRejectionTargetCandidate;   // закэшированная ссылка на ближайший
                                                                        // отменяемый кандидат

        public Expression Expression { get; private set; }
        public SearchContext SearchContext;
        public CompoundCandidate ParentCandidate;
        public CompoundCandidate TargetParentCandidate; // Кандидат, к которому будет привязана текущая
                                                        // цепочка кандидатов после окончания совпадения.
        public Candidate CurrentEventObserver;
        public TextLocation Start;
        public TextLocation End;

        public Candidate(Expression expression)
        {
            Expression = expression;
        }

        public void RejectRoot()
        {
            RootCandidate rootCandidate = GetRootCandidate();
            rootCandidate.Reject();
        }

        public void RejectTarget()
        {
            RejectionTargetCandidate rejectionTargetCandidate = GetRejectionTargetCandidate();
            rejectionTargetCandidate.Reject();
        }

        public RootCandidate GetRootCandidate()
        {
            if (fRootCandidate == null)
            {
                Candidate current;
                Candidate parent = this;
                do
                {
                    current = parent;
                    if (current.TargetParentCandidate != null)
                        parent = current.TargetParentCandidate;
                    else
                        parent = current.ParentCandidate;
                } while (parent != null);
                fRootCandidate = (RootCandidate)current;
            }
            return fRootCandidate;
        }

        public RejectionTargetCandidate GetRejectionTargetCandidate()
        {
            if (fRejectionTargetCandidate == null)
            {
                Candidate current;
                Candidate parent = this;
                do
                {
                    current = parent;
                    if (current.TargetParentCandidate != null)
                        parent = current.TargetParentCandidate;
                    else
                        parent = current.ParentCandidate;
                } while (!(current is RejectionTargetCandidate));
                fRejectionTargetCandidate = (RejectionTargetCandidate)current;
            }
            return fRejectionTargetCandidate;
        }

        public override string ToString()
        {
            string result;
            if (Start != null && End != null)
                result = SearchContext.TextSource.GetText(Start, End);
            else
                result = string.Empty;
            return result;
        }

        // Internal

        protected void CompleteMatch(MatchingEvent matchingEvent)
        {
            if (!IsCompleted)
            {
                if (TargetParentCandidate.IsExpectedParentOf(this))
                    ParentCandidate = TargetParentCandidate;
                else
                {
                    ParentCandidate = (CompoundCandidate)Expression.ParentExpression.CreateCandidate(
                        SearchContext, TargetParentCandidate);
                    ParentCandidate.Start = Start;
                }
                TargetParentCandidate = null;
                ParentCandidate.OnElementMatch(this, matchingEvent);
                OnCompleted();
            }
        }

        public static readonly CandidateStartTokenNumberComparer StartTokenNumberComparer =
            new CandidateStartTokenNumberComparer();

        public static readonly CandidateEndTokenNumberComparer EndTokenNumberComparer =
            new CandidateEndTokenNumberComparer();
    }

    internal class CandidateStartTokenNumberComparer : IComparer<Candidate>
    {
        public int Compare(Candidate x, Candidate y)
        {
            return x.Start.TokenNumber.CompareTo(y.Start.TokenNumber);
        }

        public int CompareToLong(long x, Candidate y)
        {
            return x.CompareTo(y.Start.TokenNumber);
        }
    }

    internal class CandidateEndTokenNumberComparer : IComparer<Candidate>
    {
        public int Compare(Candidate x, Candidate y)
        {
            return x.End.TokenNumber.CompareTo(y.End.TokenNumber);
        }

        public int CompareToLong(long x, Candidate y)
        {
            return x.CompareTo(y.End.TokenNumber);
        }
    }
}
