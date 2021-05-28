//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;

namespace Nezaboodka.Nevod
{
    public class PackageCache
    {
        public static readonly PackageCache Global = new PackageCache();

        private int fLastRootExpressionId;
        internal Dictionary<string, PatternExpression> PatternByName { get; private set; }

        public Dictionary<string, LinkedPackageSyntax> PackageSyntaxByFilePath { get; private set; }

        public HashSet<string> GeneratedPackages { get; private set; }

        public PackageCache()
        {
            PatternByName = new Dictionary<string, PatternExpression>();
            PackageSyntaxByFilePath = new Dictionary<string, LinkedPackageSyntax>(StringComparer.InvariantCultureIgnoreCase);
            GeneratedPackages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            fLastRootExpressionId = -1;
        }

        public void Clear()
        {
            PackageSyntaxByFilePath.Clear();
            PatternByName.Clear();
            GeneratedPackages.Clear();
            Interlocked.Exchange(ref fLastRootExpressionId, -1);
        }

        public int GetNextRootExpressionId()
        {
            return Interlocked.Increment(ref fLastRootExpressionId);
        }

        public int GetLastRootExpressionId()
        {
            return fLastRootExpressionId;
        }
    }
}
