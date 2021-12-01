using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("PatternLinker")]
    public class PatternLinkerTests
    {
        [TestMethod]
        public void ResolveSimpleReference()
        {
            string patterns = @"
P1 = 'Nezaboodka';
P2 = P1;
";
            var linker = new PatternLinker();
            var syntax = new SyntaxParser().ParsePackageText(patterns);
            LinkedPackageSyntax package = linker.Link(syntax, Environment.CurrentDirectory, filePath: null);
            var p1 = (PatternSyntax)package.Patterns[0];
            var p2 = (PatternSyntax)package.Patterns[1];
            var reference = (PatternReferenceSyntax)p2.Body;
            Assert.AreSame(p1, reference.ReferencedPattern);
        }

        [TestMethod]
        public void ResolveReferenceWithinNamespace()
        {
            string patterns = @"
@namespace N
{
    P1 = 'Nezaboodka';
    P2 = P1;
}
P2 = 'Nevod';
";
            var linker = new PatternLinker();
            var syntax = new SyntaxParser().ParsePackageText(patterns);
            LinkedPackageSyntax package = linker.Link(syntax, Environment.CurrentDirectory, filePath: null);
            var p1 = (PatternSyntax)package.Patterns[0];
            var p2 = (PatternSyntax)package.Patterns[1];
            var p3 = (PatternSyntax)package.Patterns[2];
            var reference = (PatternReferenceSyntax)p2.Body;
            Assert.AreSame(p1, reference.ReferencedPattern);
        }

        [TestMethod]
        public void AvoidReferenceInjectionOutsideOfNamespace()
        {
            string patterns = @"
@namespace N
{
    P1 = 'Nezaboodka';
    P2 = P1;
}
P1 = 'Nevod';
";
            var linker = new PatternLinker();
            var syntax = new SyntaxParser().ParsePackageText(patterns);
            LinkedPackageSyntax package = linker.Link(syntax, Environment.CurrentDirectory, filePath: null);
            var p1 = (PatternSyntax)package.Patterns[0];
            var p2 = (PatternSyntax)package.Patterns[1];
            var p3 = (PatternSyntax)package.Patterns[2];
            var reference = (PatternReferenceSyntax)p2.Body;
            Assert.AreSame(p1, reference.ReferencedPattern);
        }

        [TestMethod]
        public void CannotRedefineStandardPatternReference()
        {
            string patterns = @"
@namespace N
{
    P1 = Word;
    P2 = N.Word;
    Word = 'Nevod';
}
";
            var linker = new PatternLinker();
            var syntax = new SyntaxParser().ParsePackageText(patterns);
            LinkedPackageSyntax package = linker.Link(syntax, Environment.CurrentDirectory, filePath: null);
            var p1 = (PatternSyntax)package.Patterns[0];
            var p2 = (PatternSyntax)package.Patterns[1];
            var p3 = (PatternSyntax)package.Patterns[2];
            Assert.IsTrue(p1.Body is TokenSyntax); // Word is translated to token but not reference
            var reference = (PatternReferenceSyntax)p2.Body;
            Assert.AreSame(p3, reference.ReferencedPattern);
        }
    }
}
