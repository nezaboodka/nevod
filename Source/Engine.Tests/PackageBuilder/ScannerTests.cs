using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("Scanner")]
    public class ScannerTests
    {
        [TestMethod]
        public void SimpleNextTokenOrComment()
        {
            string text = @"
Pattern = Word;
// Single line comment
'string'!
/* Multiline
comment */
";
            var scanner = new Scanner(text);
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Identifier, "Pattern");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Equal, "=");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Identifier, "Word");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Semicolon, ";");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Comment, @"// Single line comment
");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.StringLiteral, "'string'!");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Comment, @"/* Multiline
comment */");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.End, "");
        }

        [TestMethod]
        public void SetPositionAndSaveState()
        {
            string text = @"Pattern = Word + Num & AlphaNum;";
            var scanner = new Scanner(text);
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Identifier, "Pattern");
            scanner.SaveState();
            // Start of Num
            scanner.SetPosition(17);
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Identifier, "Num");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Amphersand, "&");
            scanner.RestoreState();
            TestToken(scanner.CurrentToken, TokenId.Identifier, "Pattern");
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.Equal, "=");
        }

        [TestMethod]
        public void DetermineLanguage()
        {
            string text = @"
// ru
@шаблон
";
            var scanner = new Scanner(text);
            // Skip comment
            scanner.NextTokenOrComment();
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.PatternKeyword, "@шаблон");
        }

        [TestMethod]
        public void DetermineLanguageWithSetPosition()
        {
            string text = @"
// Some comment
// ru
@шаблон
".Replace("\r\n", "\n");
            var scanner = new Scanner(text);
            // Start of @шаблон
            scanner.SetPosition(23);
            scanner.NextTokenOrComment();
            TestToken(scanner.CurrentToken, TokenId.PatternKeyword, "@шаблон");
        }
    }
}
