//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Nevod
{
    internal sealed class FieldReferenceCandidate : CompoundCandidate
    {
        public FieldReferenceCandidate(FieldReferenceExpression expression)
            : base(expression)
        {
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            Expression elementExpression = element.Expression;
            switch (elementExpression.PositionInParentExpression)
            {
                case FieldReferenceExpression.BodyPosition:
                    FieldReferenceExpression fieldReferenceExpression = (FieldReferenceExpression)Expression;
                    int fieldNumber = fieldReferenceExpression.FieldNumber;
                    ExtractionCandidate extraction = GetRootCandidate().GetFieldLatestValue(fieldNumber);
                    if (extraction != null)
                    {
                        if (extraction.TextExpression == null)
                        {
                            string text = SearchContext.GetText(extraction.Start, extraction.End);
                            extraction.TextExpression = TextSequenceGenerator.Generate(text, isCaseSensitive: false);
                        }
                        extraction.TextExpression.SetParentExpression(fieldReferenceExpression,
                            FieldReferenceExpression.TextPosition);
                        if (matchingEvent is TokenEvent tokenEvent)
                        {
                            bool processed = OnFirstTokenOfTextSequence(tokenEvent, extraction.TextExpression);
                            if (!processed)
                                matchingEvent.Reject();
                        }
                    }
                    else
                        throw new InvalidOperationException("Cannot reference to non-extracted field.");
                    break;
                case FieldReferenceExpression.TextPosition:
                    matchingEvent.UpdateEventObserver(this);
                    End = matchingEvent.Location;
                    CompleteMatch(matchingEvent);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Invalid position in parent.");
            }
        }

        // Internal

        // Во избежание повторного входа в LocalIndexHandler и перезаписи текущего состояния
        // совпадения кандидата, для ссылки на поле используется отдельный обработчик индекса.
        private bool OnFirstTokenOfTextSequence(TokenEvent tokenEvent, Expression nextExpression)
        {
            return SearchContext.FieldReferenceCandidateOnFirstToken(candidate: this, tokenEvent, nextExpression);
        }
    }
}
