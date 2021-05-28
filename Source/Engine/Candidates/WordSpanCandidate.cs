//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal sealed class WordSpanCandidate : AnySpanCandidate
    {
        public int WordSpanCount;

        public WordSpanCandidate(WordSpanExpression expression)
            : base(expression)
        {
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            WordSpanExpression expression = (WordSpanExpression)Expression;
            if (matchingEvent is TokenEvent tokenEvent)
            {
                if (WordSpanCount >= expression.SpanRangeInWords.LowBound)
                {
                    bool processed = OnNextToken(tokenEvent, expression.Right, includeOptional: true,
                        alwaysCloneCandidateToContinueMatching: true);
                    matchingEvent.Ignore();
                }
                if (tokenEvent.Token.Kind == TokenKind.Word)
                {
                    WordSpanCount++;
                    if (WordSpanCount > expression.SpanRangeInWords.HighBound)
                        matchingEvent.Reject();
                }
            }
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            Expression elementExpression = element.Expression;
            int positionInParent = elementExpression.PositionInParentExpression;
            switch (positionInParent)
            {
                case WordSpanExpression.LeftPosition:
                    SaveEndOfLeft(left: element);
                    break;
                case WordSpanExpression.RightPosition:
                    SaveExtractionOfSpanIfSpecified();
                    TryFinalMatchOrLeavePending();
                    CompleteMatch(matchingEvent);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Invalid position in parent.");
            }
        }
    }
}
