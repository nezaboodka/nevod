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
    public class PackageCache : ILinkerCache
    {
        public static readonly PackageCache Global = new PackageCache();
        private int fLastRootExpressionId;

        public HashSet<string> GeneratedPackages { get; }
        internal Dictionary<string, PatternExpression> PatternByName { get; }
        private Dictionary<string, LinkedPackageSyntax> PackageSyntaxByFilePath { get; }

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

        public bool TryGetLinkedPackage(string filePath, out LinkedPackageSyntax linkedPackageSyntax)
        {
            return PackageSyntaxByFilePath.TryGetValue(filePath, out linkedPackageSyntax);
        }

        public void AddLinkedPackage(string filePath, LinkedPackageSyntax linkedPackageSyntax)
        {
            PackageSyntaxByFilePath[filePath] = linkedPackageSyntax;
        }
    }
}
