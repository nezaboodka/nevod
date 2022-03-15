using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Nezaboodka.Nevod.Engine.Tests.ErrorRecoveryTestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("PatternLinker"), TestCategory("Linker error recovery"), TestCategory("Error recovery")]
    public class LinkerErrorRecoveryTests
    {
        [TestMethod]
        public void ErrorsInSingleFile()
        {
            LinkAndCompareErrors(
                patterns: @"
P1 = Word;
P2 = P1 + UndefinedPattern;
",
                CreateExpectedError(invalidToken: "UndefinedPattern", TextResource.ReferenceToUndefinedPattern));
            LinkAndCompareErrors(
                patterns: @"
DuplicatedName = 'This is an original pattern';
DuplicatedName = 'This is a pattern with duplicated name';
",
                CreateExpectedErrorWithArgs(invalidToken: "DuplicatedName = 'This is a pattern with duplicated name';\n", 
                    TextResource.DuplicatedPatternName, "DuplicatedName"));
            LinkAndCompareErrors(
                patterns: "Word = 'Word';",
                CreateExpectedErrorWithArgs(invalidToken: "Word = 'Word';",
                    TextResource.DuplicatedStandardPatternName, "Word"));
            LinkAndCompareErrors(
                patterns: @"
PatternWithFields(X, Y) = X: Word + Y: Num;
ReferenceWithUndefinedField(A, B) = PatternWithFields(A: X, B: Z);
",
                CreateExpectedErrorWithArgs(invalidToken: "B: Z", 
                    TextResource.UndefinedFieldInReferencedPattern, "Z", "PatternWithFields"));
            LinkAndCompareErrors(
                patterns: "@search UndefinedSearchTarget;",
                CreateExpectedErrorWithArgs(invalidToken: "@search UndefinedSearchTarget;", 
                    TextResource.SearchTargetIsUndefinedPattern, "UndefinedSearchTarget"));
        }

        [TestMethod]
        public void DuplicatedPatternWithOriginalInRequiredPackage()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'Required.np';

P1 = Word;
";
                    case "Required.np":
                        return @"
P1 = Word;
";
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            LinkAndCompareErrors(
                filePath: "Main.np",
                GetFileContent,
                CreateExpectedErrorWithArgs(invalidToken: "P1 = Word;\n", 
                    TextResource.DuplicatedPatternIsAlreadyDeclaredIn, "P1", "Required.np"));
        }

        [TestMethod]
        public void NameConflictsInRequiredPackages()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'A.np';
@require 'B.np';
@require 'C.np';
@require 'D.np';
";
                    case "A.np":
                        return @"
A1 = Word;
// Error about this duplicated pattern name should not be added to errors of Main.np
A1 = Word;
";
                    case "B.np":
                        return @"
B1 = Word;
B2 = Word;
";
                    case "C.np":
                        return @"
C1 = Word;
C2 = Word;
C3 = Word;
C4 = Word;
C5 = Word;
";
                    case "D.np":
                        return @"
A1 = Word;
// Error about this duplicated pattern name should not be added to errors of Main.np
A1 = Word;

B1 = Word;
B2 = Word;

C1 = Word;
C2 = Word;
C3 = Word;
C4 = Word;
C5 = Word;
";
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            LinkAndCompareErrors(
                filePath: "Main.np",
                GetFileContent,
                new List<ExpectedError>
                {
                    CreateExpectedErrorWithArgs(invalidToken: "@require 'D.np';\n", TextResource.DuplicatedPatternInRequiredPackage,
                        "D.np", "A1", "A.np"),
                    CreateExpectedErrorWithArgs(invalidToken: "@require 'D.np';\n", TextResource.DuplicatedPatternsInRequiredPackage,
                        "D.np", 2, "'B1', 'B2'", "B.np"),
                    CreateExpectedErrorWithArgs(invalidToken: "@require 'D.np';\n", TextResource.DuplicatedPatternsAndMoreInRequiredPackage,
                        "D.np", 5, "'C1', 'C2', 'C3'", 2, "C.np")
                });
        }

        [TestMethod]
        public void DuplicatedRequiredPackage()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'Required.np';
@require './Required.np';

P2 = P1;
";
                    case "Required.np":
                        return @"
P1 = Word;
";
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            LinkAndCompareErrors(
                filePath: "Main.np",
                GetFileContent,
                CreateExpectedErrorWithArgs(invalidToken: "@require './Required.np';\n\n", 
                    TextResource.DuplicatedRequiredPackage, "./Required.np", "Required.np"));
        }
        
        [TestMethod]
        public void DuplicatedRequiredPackageCaseInsensitive()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'Required.np';
@require './REQUIRED.np';

P2 = P1;
";
                    case "Required.np":
                        return @"
P1 = Word;
";
                    case "REQUIRED.np":
                        return "";
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            var pathCaseNormalizer = new PathCaseNormalizer();
            if (!pathCaseNormalizer.IsFileSystemCaseSensitive)
            {
                LinkAndCompareErrors(
                    filePath: "Main.np",
                    GetFileContent,
                    CreateExpectedErrorWithArgs(invalidToken: "@require './REQUIRED.np';\n\n",
                        TextResource.DuplicatedRequiredPackage, "./REQUIRED.np", "Required.np"));
            }
            else
            {
                LinkAndCompareErrors(
                    filePath: "Main.np",
                    GetFileContent,
                    Array.Empty<ExpectedError>()
                    );
            }
        }
        
        [TestMethod]
        public void RecursiveDependency()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'Required.np';
";
                    case "Required.np":
                        return @"
@require 'Main.np';
";
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            string FullPath(string fileName) => Path.Combine(Directory.GetCurrentDirectory(), fileName);
            
            void SelectPatternsAndPackage(LinkedPackageSyntax rootPackage, out string patterns,
                out LinkedPackageSyntax package)
            {
                patterns = GetFileContent("Required.np").Replace("\r\n", "\n");
                package = rootPackage.RequiredPackages[0].Package;
            }
            
            LinkAndCompareErrors(
                filePath: "Main.np",
                GetFileContent,
                CreateExpectedErrorWithArgs(invalidToken: "@require 'Main.np';\n", 
                    TextResource.RecursiveFileDependencyIsNotSupported, 
                    $"{FullPath("Main.np")} -> {FullPath("Required.np")} -> {FullPath("Main.np")}"),
                SelectPatternsAndPackage);
        }

        [TestMethod]
        public void ErrorLoadingRequiredFile()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'Required.np';
";
                    case "Required.np":
                        throw new FileNotFoundException();
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            LinkAndCompareErrors(
                filePath: "Main.np",
                GetFileContent,
                CreateExpectedErrorWithArgs(invalidToken: "@require 'Required.np';\n", 
                    TextResource.FileNotFound, 
                    Path.Combine(Directory.GetCurrentDirectory(), "Required.np")));
        }

        [TestMethod]
        public void HasOwnOrRequiredPackageErrorsFlag()
        {
            string GetFileContent(string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                switch (fileName)
                {
                    case "Main.np":
                        return @"
@require 'RequiredWithErrors.np';
@require 'RequiredWithoutErrors.np';
@require 'RequiredWithoutOwnErrors.np';

// Missing semicolon error
P1 = Word
";
                    case "RequiredWithErrors.np":
                        return @"
// Reference to undefined pattern error
P2 = UndefinedPattern;
";
                    case "RequiredWithoutErrors.np":
                        return @"
// No errors
P3 = Num;
";
                    case "RequiredWithoutOwnErrors.np":
                        return @"
// Errors only in required package
@require 'RequiredWithErrors.np';

P4 = 'Word';
";
                    default:
                        throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
                }
            }
            
            PackageSyntax package = new SyntaxParser().ParsePackageText(GetFileContent("Main.np"));
            LinkedPackageSyntax linkedPackage = new PatternLinker(GetFileContent).Link(package, baseDirectory: "", filePath: "Main.np");
            Assert.IsTrue(linkedPackage.HasOwnOrRequiredPackageErrors);
            // RequiredWithErrors.np
            Assert.IsTrue(linkedPackage.RequiredPackages[0].Package.HasOwnOrRequiredPackageErrors);
            // RequiredWithoutErrors.np
            Assert.IsFalse(linkedPackage.RequiredPackages[1].Package.HasOwnOrRequiredPackageErrors); 
            // RequiredWithoutOwnErrors.np
            Assert.IsTrue(linkedPackage.RequiredPackages[2].Package.HasOwnOrRequiredPackageErrors);
        }
    }
}
