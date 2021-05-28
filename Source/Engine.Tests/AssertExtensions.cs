//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod.Engine.Tests
{
    internal static class AssertExtensions
    {
        public static void AreEqual<T>(this Assert assert, T expected, T actual, IEqualityComparer<T> comparer)
        {
            Assert.IsTrue(comparer.Equals(expected, actual));
        }
    }
}
