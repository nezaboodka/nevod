//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Text.Parsing
{
    public class ParsedText
    {
        private readonly List<TokenReference> fPlainTextTokens;
        private readonly List<FormattingTag> fFormattingTags;
        private readonly List<DocumentTag> fDocumentTags;
        private readonly List<string> fXhtmlElements;
        private readonly List<int> fPlainTextInXhtml;

        // Public

        public List<TokenReference> PlainTextTokens => fPlainTextTokens;
        public IEnumerable<FormattingTag> FormattingTags => fFormattingTags;
        public IEnumerable<DocumentTag> DocumentTags => fDocumentTags;
        public List<string> XhtmlElements => fXhtmlElements;

        public ParsedText()
        {
            fPlainTextTokens = new List<TokenReference>();
            fFormattingTags = new List<FormattingTag>();
            fXhtmlElements = new List<string>();
            fPlainTextInXhtml = new List<int>();
            fDocumentTags = new List<DocumentTag>();
        }

        public string GetPlainText()
        {
            StringBuilder plainTextBuilder = new StringBuilder();
            foreach (int plainTextElementIndex in fPlainTextInXhtml)
            {
                plainTextBuilder.Append(fXhtmlElements[plainTextElementIndex]);
            }
            return plainTextBuilder.ToString();
        }

        public string GetTokenText(TokenReference token)
        {
            string result;
            if (token.StringPosition + token.StringLength <= fXhtmlElements[token.XhtmlIndex].Length)
                result = fXhtmlElements[token.XhtmlIndex].Substring(token.StringPosition, token.StringLength);
            else
                result = GetCompoundTokenText(token);
            return result;
        }

        public string GetTagText(FormattingTag tag)
        {
            StringBuilder result = new StringBuilder();
            for (int tokenIndex = tag.TokenPosition, i = 0; i < tag.TokenLength; i++, tokenIndex++)
                result.Append(GetTokenText(PlainTextTokens[tokenIndex]));
            return result.ToString();
        }

        // Internal

        internal void AddToken(TokenReference token)
        {
            fPlainTextTokens.Add(token);
        }

        internal void AddTag(FormattingTag tag)
        {
            fFormattingTags.Add(tag);
        }

        internal void AddTags(IEnumerable<FormattingTag> tags)
        {
            foreach (FormattingTag tag in tags)
            {
                AddTag(tag);
            }
        }

        internal void AddDocumentTag(DocumentTag documentTag)
        {
            fDocumentTags.Add(documentTag);
        }

        internal void AddXhtmlElement(string xhtmlElement)
        {
            fXhtmlElements.Add(xhtmlElement);            
        }

        internal void AddPlainTextElement(string plainTextElement)
        {
            AddXhtmlElement(plainTextElement);
            fPlainTextInXhtml.Add(fXhtmlElements.Count - 1);
        }

        private string GetCompoundTokenText(TokenReference token)
        {
            StringBuilder tokenTextBuilder = new StringBuilder();
            int currentSubstringLength = fXhtmlElements[token.XhtmlIndex].Length - token.StringPosition;
            tokenTextBuilder.Append(fXhtmlElements[token.XhtmlIndex].Substring(token.StringPosition, currentSubstringLength));
            int copiedLength = currentSubstringLength;            
            int plainTextElement = fPlainTextInXhtml.BinarySearch(token.XhtmlIndex) + 1;
            while (copiedLength < token.StringLength)
            {
                int xhtmlElement = fPlainTextInXhtml[plainTextElement];
                int remainedLength = token.StringLength - copiedLength;                
                if (fXhtmlElements[xhtmlElement].Length <= remainedLength)
                    currentSubstringLength = fXhtmlElements[xhtmlElement].Length;
                else
                    currentSubstringLength = remainedLength;
                tokenTextBuilder.Append(fXhtmlElements[xhtmlElement].Substring(0, currentSubstringLength));
                copiedLength += currentSubstringLength;
                plainTextElement++;
            }
            return tokenTextBuilder.ToString();
        }      
    }

    public struct TokenReference
    {
        public int XhtmlIndex;
        public int StringPosition;
        public int StringLength;
        public TokenKind TokenKind;
        public bool IsHexadecimal;
    }

    public enum TokenKind
    {
        Undefined,
        Start,
        End,
        Alphabetic,
        AlphaNumeric,
        NumericAlpha,
        LineFeed,
        Numeric,
        Punctuation,
        Symbol,
        WhiteSpace
    }

    public struct FormattingTag
    {
        public int TokenPosition;
        public int TokenLength;
        public string TagName;
    }

    public struct DocumentTag
    {
        public string TagName;
        public string Content;
    }
}
