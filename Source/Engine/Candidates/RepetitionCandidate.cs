//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Nevod
{
    internal sealed class RepetitionCandidate : CompoundCandidate
    {
        public int RepetitionCount;

        public RepetitionCandidate(CompoundExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (RepetitionCandidate)MemberwiseClone();
            result.fRootCandidate = null;
            result.fRejectionTargetCandidate = null;
            return result;
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            if (matchingEvent is TokenEvent tokenEvent)
            {
                bool processed = OnNextToken(tokenEvent, Expression, includeOptional: false,
                    alwaysCloneCandidateToContinueMatching: false);
                if (!processed)
                    matchingEvent.Reject();
            }
        }

        // При полном переборе возникает ЭКСПОНЕНЦИАЛЬНЫЙ РОСТ ЧИСЛА КАНДИДАТОВ, если:
        // 1) внутри повторителя есть другой повторитель,
        // 2) верхняя граница не равна нижней в каждом повторителе,
        // 3) нижняя граница внутреннего повторителя меньше либо равна нижней границе внешнего повторителя.
        //
        // Пример
        // - шаблон: #T = S + [1+] {[1+] A, B};
        // - лексемы: SAAA...AA
        //
        // На каждой последующей лексеме "A" соответствующий элемент вариации копирует всё дерево:
        // - в одной копии на новое повторение идёт кандидат А,
        // - в другой копии на новое повторение идёт вся вариация.
        // Таким образом на каждой лексеме "А" число деревьев увеличивается в 2 раза;
        // на 1-й лексеме "А" будет 1 дерево кандидатов, а на N-й лексеме "А" будет 2^(N-1) деревьев,
        // обработка которых потребляет много памяти и значительно замедляет поиск.
        //
        // Для исправления экспоненциального роста числа кандидатов, в определённых условиях
        // внешний повторитель перестаёт пытаться набрать большее число повторений:
        //
        //        ,---> копия внутреннего повторителя продолжает совпадать
        //       /
        // ---> * (внутренний совпал)
        //       \
        //        \   ,---> [X] - копия внешнего повторителя ОТМЕНЯЕТСЯ
        //         \ /
        //          * (внешний совпал)
        //           \
        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            RepetitionCount++;
            RepetitionExpression repetitionExpression = (RepetitionExpression)Expression;
            Range range = repetitionExpression.RepetitionRange;
            if (RepetitionCount >= range.LowBound)
            {
                if (RepetitionCount < range.HighBound)
                {
                    // FIXME: Определить оптимальное условие для ограничения перебора
                    // if (matchingEvent.InnerRepetitionCandidate == null
                    //     || matchingEvent.InnerRepetitionCandidate.RepetitionCount > RepetitionCount)
                    if (matchingEvent.InnerRepetitionCandidate == null)
                    {
                        this.CloneState(out RootCandidate rootCopy);
                        bool success = SearchContext.TryAddToActiveCandidates(rootCopy);
                        if (!success)
                            rootCopy.Reject();
                    }
                }
                matchingEvent.InnerRepetitionCandidate = this;
                CompleteMatch(matchingEvent);
            }
        }
    }
}
