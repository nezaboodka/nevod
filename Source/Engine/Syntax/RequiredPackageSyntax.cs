//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class RequiredPackageSyntax : Syntax
    {
        public string BaseDirectory { get; }
        public string RelativePath { get; }
        public string FullPath { get; }
        public new LinkedPackageSyntax Package { get; }

        internal RequiredPackageSyntax(string baseDirectory, string relativePath, LinkedPackageSyntax package)
        {
            BaseDirectory = baseDirectory;
            RelativePath = relativePath;
            FullPath = GetRequiredFilePath(baseDirectory, relativePath);
            Package = package;
        }

        internal RequiredPackageSyntax(RequiredPackageSyntax source, LinkedPackageSyntax package)
        {
            BaseDirectory = source.BaseDirectory;
            RelativePath = source.RelativePath;
            FullPath = source.FullPath;
            Package = package;
        }

        internal RequiredPackageSyntax Update(LinkedPackageSyntax package)
        {
            RequiredPackageSyntax result = this;
            if (package != Package)
                result = new RequiredPackageSyntax(this, package);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitRequiredPackage(this);
        }
    }

    public partial class Syntax
    {
        public static RequiredPackageSyntax RequiredPackage(string baseDirectory, string relativePath,
            LinkedPackageSyntax package)
        {
            RequiredPackageSyntax result = new RequiredPackageSyntax(baseDirectory, relativePath, package);
            return result;
        }

        public static string GetRequiredFilePath(string baseDirectory, string relativePath)
        {
            string filePath;
            if (!string.IsNullOrEmpty(baseDirectory))
                filePath = Path.Combine(baseDirectory, relativePath);
            else
                filePath = relativePath;
            filePath = Path.GetFullPath(filePath);
            return filePath;
        }
    }
}
