namespace Nezaboodka.Nevod
{
    public class TerminalSyntax : Syntax
    {
        public TokenId TokenId { get; }

        internal TerminalSyntax(TokenId tokenId)
        {
            TokenId = tokenId;
        }
    }

    public partial class Syntax
    {
        public static TerminalSyntax Terminal(TokenId tokenId)
        {
            var result = new TerminalSyntax(tokenId);
            return result;
        }
    }
}
