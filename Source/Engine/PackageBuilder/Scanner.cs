﻿using System;
using System.Collections.Generic;
using Nezaboodka.Text;

namespace Nezaboodka.Nevod
{
    internal class Scanner
    {
        private readonly Slice fText;
        private int fTextPosition;
        private char fCharacter;
        private readonly Dictionary<string, Keyword> fKeywordsDictionary;
        private bool fIsScanningMetadata;
        private bool fIsLanguageDetermined;
        private readonly Stack<State> fStates;

        public Language DetectedLanguage { get; private set; }
        public LexicalToken CurrentToken { get; private set; }

        public Scanner(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            fStates = new Stack<State>();
            fIsScanningMetadata = true;
            fKeywordsDictionary = Keywords.GetDictionary();
            fText = text.Slice();
            fTextPosition = -1;
            NextCharacter();
        }

        public void SetPosition(int position)
        {
            if (position < 0 || position >= fText.Length)
                throw new ArgumentOutOfRangeException(nameof(position));
            if (!fIsLanguageDetermined)
                ParseMetadataUntil(position);
            fTextPosition = position - 1;
            NextCharacter();
        }

        public void NextTokenOrComment()
        {
            while (char.IsWhiteSpace(fCharacter))
                NextCharacter();
            TokenId tokenId = TokenId.Unknown;
            int tokenPosition = fTextPosition;
            switch (fCharacter)
            {
                case '(':
                    NextCharacter();
                    tokenId = TokenId.OpenParenthesis;
                    break;
                case ')':
                    NextCharacter();
                    tokenId = TokenId.CloseParenthesis;
                    break;
                case '{':
                    NextCharacter();
                    tokenId = TokenId.OpenCurlyBrace;
                    break;
                case '}':
                    NextCharacter();
                    tokenId = TokenId.CloseCurlyBrace;
                    break;
                case '[':
                    NextCharacter();
                    tokenId = TokenId.OpenSquareBracket;
                    break;
                case ']':
                    NextCharacter();
                    tokenId = TokenId.CloseSquareBracket;
                    break;
                case '.':
                    NextCharacter();
                    if (fCharacter == '.')
                    {
                        NextCharacter();
                        if (fCharacter == '.')
                        {
                            NextCharacter();
                            tokenId = TokenId.Ellipsis;
                        }
                        else
                            tokenId = TokenId.DoublePeriod;
                    }
                    else
                        tokenId = TokenId.Period;
                    break;
                case ',':
                    NextCharacter();
                    tokenId = TokenId.Comma;
                    break;
                case ':':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.Assignment;
                    }
                    else if (fCharacter == ':')
                    {
                        NextCharacter();
                        tokenId = TokenId.DoubleColon;
                    }
                    else
                        tokenId = TokenId.Colon;
                    break;
                case ';':
                    NextCharacter();
                    tokenId = TokenId.Semicolon;
                    break;
                case '#':
                    NextCharacter();
                    tokenId = TokenId.HashSign;
                    break;
                case '~':
                    NextCharacter();
                    tokenId = TokenId.Tilde;
                    break;
                case '@':
                    NextCharacter();
                    if (char.IsLetter(fCharacter))
                    {
                        NextCharacter();
                        while (char.IsLetterOrDigit(fCharacter) || fCharacter == '-')
                            NextCharacter();
                        Slice keywordSlice = fText.SubSlice(tokenPosition, fTextPosition - tokenPosition);
                        string keywordName = keywordSlice.ToString();
                        Keyword keyword;
                        if (fKeywordsDictionary.TryGetValue(keywordName, out keyword))
                        {
                            tokenId = keyword.TokenId;
                            DetectedLanguage |= keyword.Language;
                        }
                        else
                            tokenId = TokenId.UnknownKeyword;
                    }
                    else
                        tokenId = TokenId.UnknownKeyword;
                    break;
                case '+':
                    NextCharacter();
                    tokenId = TokenId.Plus;
                    break;
                case '-':
                    NextCharacter();
                    tokenId = TokenId.Minus;
                    break;
                case '*':
                    NextCharacter();
                    tokenId = TokenId.Asterisk;
                    break;
                case '/':
                    NextCharacter();
                    if (fCharacter == '/')
                    {
                        NextCharacter();
                        while (fTextPosition < fText.Length && fCharacter != '\n')
                            NextCharacter();
                        if (fTextPosition < fText.Length)
                            NextCharacter();
                        tokenId = TokenId.Comment;
                    }
                    else if (fCharacter == '*')
                    {
                        NextCharacter();
                        char previousCharacter = '\0';
                        while (fTextPosition < fText.Length && !(previousCharacter == '*' && fCharacter == '/'))
                        {
                            previousCharacter = fCharacter;
                            NextCharacter();
                        }
                        if (fTextPosition < fText.Length)
                        {
                            NextCharacter();
                            tokenId = TokenId.Comment;
                        }
                        else
                            tokenId = TokenId.UnterminatedComment;
                    }
                    else
                        tokenId = TokenId.Slash;
                    break;
                case '&':
                    NextCharacter();
                    tokenId = TokenId.Amphersand;
                    break;
                case '?':
                    NextCharacter();
                    tokenId = TokenId.Question;
                    break;
                case '=':
                    NextCharacter();
                    tokenId = TokenId.Equal;
                    break;
                case '_':
                    NextCharacter();
                    if (fCharacter == '*')
                    {
                        NextCharacter();
                        tokenId = TokenId.UnderscoreAsterisk;
                    }
                    else
                        tokenId = TokenId.Underscore;
                    break;
                case '!':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.ExclamationEqual;
                    }
                    else
                        tokenId = TokenId.Exclamation;
                    break;
                case '<':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.LessThanEqual;
                    }
                    else
                        tokenId = TokenId.LessThan;
                    break;
                case '>':
                    NextCharacter();
                    if (fCharacter == '=')
                    {
                        NextCharacter();
                        tokenId = TokenId.GreaterThanEqual;
                    }
                    else
                        tokenId = TokenId.GreaterThan;
                    break;
                case '"':
                case '\'':
                    char quote = fCharacter;
                    do
                    {
                        NextCharacter();
                        while (fTextPosition < fText.Length && fCharacter != quote)
                            NextCharacter();
                        if (fTextPosition < fText.Length)
                            NextCharacter();
                        else
                        {
                            tokenId = TokenId.UnterminatedStringLiteral;
                            break;
                        }
                    } while (fCharacter == quote);
                    if (fCharacter == '!')
                        NextCharacter();
                    if (fCharacter == '*')
                        NextCharacter();
                    if (tokenId == TokenId.Unknown)
                        tokenId = TokenId.StringLiteral;
                    break;
                default:
                    if (char.IsLetter(fCharacter))
                    {
                        NextCharacter();
                        while (char.IsLetterOrDigit(fCharacter) || fCharacter == '-')
                            NextCharacter();
                        tokenId = TokenId.Identifier;
                        break;
                    }
                    if (char.IsDigit(fCharacter))
                    {
                        tokenId = TokenId.IntegerLiteral;
                        NextCharacter();
                        while (char.IsDigit(fCharacter))
                            NextCharacter();
                        break;
                    }
                    // Take next character to allow using fToken to format error message with the unknown character.
                    NextCharacter();
                    break;
            }
            if (tokenId == TokenId.Unknown && fTextPosition == fText.Length)
                tokenId = TokenId.End;
            CurrentToken = new LexicalToken
            {
                Id = tokenId,
                TextSlice = fText.SubSlice(tokenPosition, fTextPosition - tokenPosition)
            };
            if (tokenId == TokenId.Comment || tokenId == TokenId.UnterminatedComment)
            {
                if (fIsScanningMetadata)
                    TryParseMetadata();
            }
            else
                fIsScanningMetadata = false;
        }

        public void SaveState()
        {
            var state = new State(fTextPosition, fCharacter, CurrentToken);
            fStates.Push(state);
        }

        public void RestoreState()
        {
            if (fStates.Count == 0)
                throw new InvalidOperationException("States stack is empty");
            State state = fStates.Pop();
            fTextPosition = state.TextPosition;
            fCharacter = state.Character;
            CurrentToken = state.Token;
        }

        private void NextCharacter()
        {
            if (fTextPosition < fText.Length)
                fTextPosition++;
            fCharacter = fTextPosition < fText.Length ? fText.Source[fTextPosition] : '\0';
        }

        private void ParseMetadataUntil(int endPosition)
        {
            fTextPosition = -1;
            NextCharacter();
            fIsScanningMetadata = true;
            NextTokenOrComment();
            while (CurrentToken.TextSlice.End < endPosition && fIsScanningMetadata)
            {
                NextTokenOrComment();
            }
        }

        private void TryParseMetadata()
        {
            if (CurrentToken.Id == TokenId.Comment || CurrentToken.Id == TokenId.UnterminatedComment)
            {
                char commentType = CurrentToken.TextSlice[1];
                int cutNum = commentType == '*' ? 2 : 0;
                string comment = CurrentToken.TextSlice.SubSlice(2, CurrentToken.TextSlice.Length - 2 - cutNum).ToString();
                string metadata = comment.Trim();
                switch (metadata)
                {
                    case "en":
                        fIsLanguageDetermined = true;
                        DetectedLanguage |= Language.English;
                        break;
                    case "ru":
                        fIsLanguageDetermined = true;
                        DetectedLanguage |= Language.Russian;
                        break;
                }
            }
        }

        private struct State
        {
            public int TextPosition { get; }
            public char Character { get; }
            public LexicalToken Token { get; }

            public State(int textPosition, char character, LexicalToken token)
            {
                TextPosition = textPosition;
                Character = character;
                Token = token;
            }
        }
    }

    internal struct LexicalToken
    {
        public TokenId Id;
        public Slice TextSlice;

        public override string ToString()
        {
            string result;
            if (Id != TokenId.End)
                result = TextSlice.ToString();
            else
                result = "<end>";
            return result;
        }

        public TextRange GetTextRange()
        {
            if (TextSlice == null)
                return new TextRange(0, 1);
            return new TextRange(TextSlice.Position, TextSlice.Position + TextSlice.Length);
        }
    }

    public enum TokenId
    {
        Unknown,
        UnknownKeyword,
        End,
        Comment,
        Identifier,
        StringLiteral,
        IntegerLiteral,
        OpenParenthesis,
        CloseParenthesis,
        OpenCurlyBrace,
        CloseCurlyBrace,
        OpenSquareBracket,
        CloseSquareBracket,
        Period,
        Comma,
        Colon,
        Semicolon,
        HashSign,
        Tilde,
        Plus,
        Minus,
        Asterisk,
        Slash,
        Amphersand,
        Question,
        Exclamation,
        Equal,
        Underscore,
        UnderscoreAsterisk,
        LessThan,
        GreaterThan,
        DoublePeriod,
        Ellipsis,
        ExclamationEqual,
        LessThanEqual,
        GreaterThanEqual,
        Assignment,
        DoubleColon,
        RequireKeyword,
        NamespaceKeyword,
        PatternKeyword,
        SearchKeyword,
        WhereKeyword,
        InsideKeyword,
        OutsideKeyword,
        HavingKeyword,
        UnterminatedComment,
        UnterminatedStringLiteral,
    }

    [Flags]
    public enum Language {
        Unknown = 0,
        English = 1,
        Russian = 2,
        All = English | Russian
    }

    public struct Keyword {
        public string Name;
        public TokenId TokenId;
        public Language Language;
    }

    public static class Keywords {

        public static Keyword[] All = new Keyword[]
        {
            new Keyword { Name = "@require", TokenId = TokenId.RequireKeyword, Language = Language.English },
            new Keyword { Name = "@namespace", TokenId = TokenId.NamespaceKeyword, Language = Language.English },
            new Keyword { Name = "@pattern", TokenId = TokenId.PatternKeyword, Language = Language.English },
            new Keyword { Name = "@search", TokenId = TokenId.SearchKeyword, Language = Language.English },
            new Keyword { Name = "@where", TokenId = TokenId.WhereKeyword, Language = Language.English },
            new Keyword { Name = "@inside", TokenId = TokenId.InsideKeyword, Language = Language.English },
            new Keyword { Name = "@outside", TokenId = TokenId.OutsideKeyword, Language = Language.English },
            new Keyword { Name = "@having", TokenId = TokenId.HavingKeyword, Language = Language.English },
            new Keyword { Name = "@требуется", TokenId = TokenId.RequireKeyword, Language = Language.Russian },
            new Keyword { Name = "@пространство", TokenId = TokenId.NamespaceKeyword, Language = Language.Russian },
            new Keyword { Name = "@шаблон", TokenId = TokenId.PatternKeyword, Language = Language.Russian },
            new Keyword { Name = "@искать", TokenId = TokenId.SearchKeyword, Language = Language.Russian },
            new Keyword { Name = "@где", TokenId = TokenId.WhereKeyword, Language = Language.Russian },
            new Keyword { Name = "@внутри", TokenId = TokenId.InsideKeyword, Language = Language.Russian },
            new Keyword { Name = "@вне", TokenId = TokenId.OutsideKeyword, Language = Language.Russian },
            new Keyword { Name = "@содержащий", TokenId = TokenId.HavingKeyword, Language = Language.Russian }
        };

        public static Dictionary<string, Keyword> GetDictionary()
        {
            var result = new Dictionary<string, Keyword>();
            foreach (var keyword in All)
                result.Add(keyword.Name, keyword);
            return result;
        }
    }
}
