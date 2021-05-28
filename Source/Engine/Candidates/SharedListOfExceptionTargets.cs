//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    // Список отменяемых кандидатов является общим для всех копий одного исключения.
    // При копировании одного из отменяемых кандидатов, для каждого исключения, которое с ним связано,
    // вызывается метод, добавляющий нового кандидата в данный список.
    // Таким образом, при наличии более одной копии исключения, добавление нового отменяемого кандидата
    // в их общий список будет производиться несколько раз через разные копии исключения.
    // Гарантируется, что за один проход каждая копия исключения добавляет одного и того же нового кандидата
    // в общий список. Также гарантируется, что за время добавления нового отменяемого кандидата
    // количество копий исключения не изменяется.

    // Необходимо избежать добавления одного кандидата в список несколько раз.

    // Для этого отслеживается количество копий исключения и ведётся счётчик модификаций,
    // который наращивается после проведения очередной операции над общим списком отменяемых кандидатов.
    // При проходе по всем исключениям и проведении модификации списка, выполняется только операция,
    // вызванная первым исключением (счётчик модификаций == 0).
    // Операции, вызываемые другими копиями исключения, пропускаются.
    // После того, как все копии исключения выполнят одну и ту же операцию
    // (счётчик модификаций > количества копий исключения), счётчик модификаций сбрасывается в 0,
    // что позволяет снова выполнить только первую операцию при следующем проходе по исключениям.
    internal class SharedListOfExceptionTargets
    {
        private List<RejectionTargetCandidate> fItems;
        private int fExceptionCopyCount;
        private int fUpdateCount;

        public int Count => fItems.Count;

        public SharedListOfExceptionTargets()
        {
            fItems = new List<RejectionTargetCandidate>();
        }

        public void RegisterExceptionCopy(ExceptionCandidate exception)
        {
            fExceptionCopyCount++;
            for (int i = 0, n = fItems.Count; i < n; i++)
                fItems[i].AddException(exception);
        }

        public void UnregisterExceptionCopy(ExceptionCandidate exception)
        {
            fExceptionCopyCount--;
            for (int i = 0, n = fItems.Count; i < n; i++)
                fItems[i].RemoveException(exception);
        }

        public void Add(RejectionTargetCandidate item)
        {
            if (fUpdateCount == 0)
                fItems.Add(item);
            IncreaseUpdateCount();
        }

        public void Remove(RejectionTargetCandidate item)
        {
            if (fUpdateCount == 0)
                fItems.Remove(item);
            IncreaseUpdateCount();
        }

        public void RejectAll()
        {
            // Во время отмены текущий кандидат изымается из списка,
            // поэтому обход производится с конца.
            int i = fItems.Count - 1;
            while (i >= 0)
            {
                fItems[i].Reject();
                i--;
            }
            fItems.Clear();
        }

        public RejectionTargetCandidate GetFirst()
        {
            return fItems[0];
        }

        // Internal

        private void IncreaseUpdateCount()
        {
            fUpdateCount++;
            if (fUpdateCount > fExceptionCopyCount)
                fUpdateCount = 0;
        }
    }
}
