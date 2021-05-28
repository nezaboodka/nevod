//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    internal interface IMatchingEventObserver
    {
        bool IsCompleted { get; }
        void OnNext(MatchingEvent matchingEvent);
        void OnCompleted();
    }

    internal abstract class MatchingEventObserver : IMatchingEventObserver
    {
        public bool IsCompleted { get; protected set; }

        public virtual void OnNext(MatchingEvent matchingEvent)
        {
            throw new InvalidOperationException();
        }

        public virtual void OnCompleted()
        {
            IsCompleted = true;
        }

        public virtual void Reset()
        {
            IsCompleted = false;
        }
    }
}
