using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("PatternLinker")]
    public class PatternLinkerTests
    {
        [TestMethod]
        public void LinkResolvesPatternReferences()
        {
            string patterns = @"
P1 = 'Nezaboodka';
P2 = P1;
";
            var linker = new PatternLinker(linkRequiredPackages: false);
            LinkedPackageSyntax package = linker.Link(new SyntaxParser().ParsePackageText(patterns));
            var p1 = (PatternSyntax) package.Patterns[0];
            var p2 = (PatternSyntax) package.Patterns[1];
            var reference = (PatternReferenceSyntax) p2.Body;
            Assert.AreSame(p1, reference.ReferencedPattern);
        }
    }
}
