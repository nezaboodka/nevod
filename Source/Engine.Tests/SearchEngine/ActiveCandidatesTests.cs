//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("CandidateList")]
    public class ActiveCandidatesTests
    {

        [TestMethod]
        public void EnumerateActiveCandidates()
        {
            ActiveCandidates cl = new ActiveCandidates();
            var a = new PatternCandidate(null);
            var b = new PatternCandidate(null);
            var c = new PatternCandidate(null);
            var d = new PatternCandidate(null);
            var e = new PatternCandidate(null);
            cl.Add(a);
            cl.Add(b);
            cl.Add(c);
            cl.Add(d);
            cl.UpdateOrRemoveEach(x =>
            {
                bool shouldRemove;
                if (x == a || x == c)
                    shouldRemove = true;
                else if (x == b)
                {
                    cl.Add(e);
                    shouldRemove = false;
                }
                else // if (x == d)
                    shouldRemove = false;
                return shouldRemove;
            });
            cl.UpdateOrRemoveEach(x =>
            {
                bool shouldRemove;
                if (x == b)
                    shouldRemove = true;
                else // if (x == "d" || x == "e")
                    shouldRemove = false;
                return shouldRemove;
            });
            Assert.AreEqual(2, cl.Elements.Count);
        }
    }
}
