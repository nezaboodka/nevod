//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class MatchedTagsOfPattern
    {
        public PatternExpression Pattern { get; }
        public List<MatchedTag> MatchedTags { get; }
        public int CleaningPosition { get; private set; }
        public int CountOfWaitingForCleanup => (MatchedTags.Count - CleaningPosition);
        public bool SelfOverlapping { get; }

        public MatchedTagsOfPattern(PatternExpression pattern, bool selfOverlapping)
        {
            MatchedTags = new List<MatchedTag>();
            Pattern = pattern;
            SelfOverlapping = selfOverlapping;
        }

        public void Add(MatchedTag matchedTag)
        {
            if (MatchedTags.Count == 0)
                MatchedTags.Add(matchedTag);
            else
            {
                int lastIndex = MatchedTags.Count - 1;
                MatchedTag lastMatchedTag = MatchedTags[lastIndex];
                if (matchedTag.Start.TokenNumber > lastMatchedTag.Start.TokenNumber)
                    MatchedTags.Add(matchedTag);
                else
                {
                    if (matchedTag.Start.TokenNumber == lastMatchedTag.Start.TokenNumber
                        && matchedTag.End.TokenNumber > lastMatchedTag.End.TokenNumber)
                    {
                        if (!SelfOverlapping)
                            MatchedTags[lastIndex] = matchedTag;
                        else
                            MatchedTags.Add(matchedTag);
                    }
                    else
                    {
                        int pos = MatchedTags.BinarySearch(matchedTag, MatchedTag.StartTokenNumberComparer);
                        if (pos >= 0)
                        {
                            if (!SelfOverlapping)
                            {
                                if (matchedTag.End.TokenNumber > MatchedTags[pos].End.TokenNumber)
                                    MatchedTags[pos] = matchedTag;
                            }
                            else
                            {
                                if (matchedTag.End.TokenNumber > MatchedTags[pos].End.TokenNumber)
                                    pos++;
                                MatchedTags.Insert(pos, matchedTag);
                            }
                        }
                        else
                        {
                            pos = ~pos;
                            MatchedTags.Insert(pos, matchedTag);
                        }
                    }
                }
            }
        }

        public bool TryRemoveOverlaps(long cleaningTokenNumber)
        {
            bool hasNewCleanedMatchedTags = false;
            if (!SelfOverlapping)
            {
                if (CleaningPosition < MatchedTags.Count)
                {
                    int positionToKeep = CleaningPosition;
                    while (positionToKeep < MatchedTags.Count
                        && MatchedTags[positionToKeep].End.TokenNumber < cleaningTokenNumber)
                    {
                        int positionToRemove = positionToKeep + 1;
                        while (positionToRemove < MatchedTags.Count
                            && MatchedTags[positionToRemove].Start.TokenNumber <= MatchedTags[positionToKeep].End.TokenNumber)
                        {
                            positionToRemove++;
                        }
                        positionToRemove--;
                        // Теперь positionToRemove указывает на последний удаляемый элемент.
                        if (positionToRemove > positionToKeep)
                            MatchedTags.RemoveRange(positionToKeep + 1, positionToRemove - positionToKeep);
                        positionToKeep++;
                        // Теперь positionToKeep указывает на следующий элемент после удалённых.
                    }
                    positionToKeep--;
                    // Теперь positionToKeep указывает на последний обработанный элемент.
                    // Его необходимо учитывать во время следующей чистки пересечений.
                    // Для этого CleaningPosition устанавливается в positionToKeep.
                    // (Только в случае, когда был обработан хотя бы один элемент)
                    if (positionToKeep > CleaningPosition)
                        CleaningPosition = positionToKeep;
                    hasNewCleanedMatchedTags = !MatchedTags[CleaningPosition].WasPassedToCallback;
                }
            }
            return hasNewCleanedMatchedTags;
        }

        public void ForEachCleaned(Action<MatchedTag> action)
        {
            if (CleaningPosition < MatchedTags.Count)
            {
                for (int i = 0; i <= CleaningPosition; i++)
                    action(MatchedTags[i]);
            }
        }

        public void RemoveCleanedExceptLast()
        {
            if (CleaningPosition < MatchedTags.Count)
            {
                MatchedTags.RemoveRange(0, CleaningPosition);
                CleaningPosition = 0;
            }
        }
    }
}
