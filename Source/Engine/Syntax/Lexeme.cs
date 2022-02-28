namespace Nezaboodka.Nevod
{
    public class Lexeme : Syntax
    {
        public TokenId TokenId { get; }

        internal Lexeme(TokenId tokenId)
        {
            TokenId = tokenId;
        }
    }

    public partial class Syntax
    {
        public static Lexeme Lexeme(TokenId tokenId)
        {
            var result = new Lexeme(tokenId);
            return result;
        }
    }
}
