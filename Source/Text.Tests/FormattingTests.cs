//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using Nezaboodka.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nezaboodka.Text.Tests
{
    [TestClass]
    [TestCategory("Text"), TestCategory("Text.Formatting")]
    public class FormattingTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void FormatTest()
        {
            FormatSample.TestFormat();
            FormatSample.TestFormat2();
        }
    }
}
