//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;

namespace Nezaboodka.Text.Parsing
{
    public class XhtmlParser : Parser
    {
        private const string HtmlTag = "html";
        private const string HeadTag = "head";
        private const string MetaTag = "meta";
        private const string TitleTag = "title";
        private const string BodyTag = "body";
        private const string NameAttribute = "name";
        private const string ContentAttribute = "content";

        private readonly XmlReader fXmlReader;
        private readonly XhtmlTagger fXhtmlTagger;
        private readonly CharacterBuffer fCharacterBuffer = new CharacterBuffer();
        private int fPlainTextXhtmlIndex;
        private int fTokenStartXhtmlIndex;
        private bool fDocumentTagsMode;
        private bool fEndOfFile;

        private int ProcessingXhtmlIndex => fCharacterBuffer.CurrentCharacterInfo.XhtmlIndex;
        private int ProcessingCharacterIndex => fCharacterBuffer.CurrentCharacterInfo.StringPosition;
        private bool IsLastCharacterInBuffer => fCharacterBuffer.CurrentCharacterInfo.IsLastCharacterInBuffer;
        private int XhtmlIndex => fParsedText.XhtmlElements.Count - 1;

        // Public

        public static ParsedText Parse(string xhtmlText)
        {
            ParsedText result;
            using (var parser = new XhtmlParser(xhtmlText))
                result = parser.Parse();
            return result;
        }

        public XhtmlParser(string xhtmlText)
        {
            if (xhtmlText != null)
            {
                fXmlReader = XmlReader.Create(new StringReader(xhtmlText));
                fXhtmlTagger = new XhtmlTagger(fParsedText);
            }
            else
                throw new ArgumentNullException(nameof(xhtmlText));
        }

        public override void Dispose()
        {
            fXmlReader.Dispose();
        }

        public override ParsedText Parse()
        {
            fParsedText.AddToken(StartToken);
            int currentTokenLength = 0;
            fDocumentTagsMode = TryParseDocumentTags();
            InitializeLookahead();
            ProcessTags();
            while (NextCharacter())
            {
                currentTokenLength++;
                fTokenClassifier.AddCharacter(fCharacterBuffer.CurrentCharacterInfo.Character);
                if (IsBreak())
                {
                    SaveToken(currentTokenLength);
                    currentTokenLength = 0;
                    ProcessTags();
                }
            }
            TokenReference endToken = GetEndToken(currentTokenLength);
            fParsedText.AddToken(endToken);
            return fParsedText;
        }

        // Internal

        private bool TryParseDocumentTags()
        {
            bool result = false;
            if (TryReadToHead())
            {
                result = true;
                bool endOfHead = false;
                while (!endOfHead && fXmlReader.Read())
                {
                    XmlNodeType nodeType = fXmlReader.NodeType;
                    if (!IsIgnoreableNode(nodeType))
                    {                            
                        if (nodeType == XmlNodeType.Element)
                            TryParseSingleDocumentTag();
                        else if (nodeType == XmlNodeType.EndElement)
                            endOfHead = fXmlReader.Name.Equals(HeadTag, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
            return result;
        }

        private bool TryReadToHead()
        {
            bool result = false;
            if (TryReadToTag(HtmlTag))
            {
                result = TryReadToTag(HeadTag);
                if (!result)
                {
                    SaveXhtmlElement(RepresentOpenTag(HtmlTag));
                    if ((fXmlReader.NodeType == XmlNodeType.Element) || (fXmlReader.NodeType == XmlNodeType.EndElement))
                        ProcessElement();
                    else if (fXmlReader.NodeType == XmlNodeType.Text)
                        ProcessText();
                }
            }
            else
            {
                ProcessElement();
            }
            return result;
        }

        private void TryParseSingleDocumentTag()
        {
            string name = string.Empty;
            string content = string.Empty;
            if (fXmlReader.Name.Equals(MetaTag, StringComparison.InvariantCultureIgnoreCase))
            {
                name = fXmlReader.GetAttribute(NameAttribute);
                content = fXmlReader.GetAttribute(ContentAttribute);                
            }
            else if (fXmlReader.Name.Equals(TitleTag, StringComparison.InvariantCultureIgnoreCase))
            {
                name = TitleTag;
                content = fXmlReader.ReadElementString();
            }
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
            {
                fParsedText.AddDocumentTag(new DocumentTag
                {
                    TagName = name,
                    Content = content
                });
            }
        }

       private bool TryReadToTag(string tag)
        {
            while (fXmlReader.Read() && IsIgnoreableNode(fXmlReader.NodeType));
            XmlNodeType nodeType = fXmlReader.NodeType;
            string tagName = fXmlReader.Name;
            return (nodeType == XmlNodeType.Element) && (tagName.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
        }

        private void InitializeLookahead()
        {
            NextCharacter();
            NextCharacter();
            fTokenStartXhtmlIndex = fCharacterBuffer.NextCharacterInfo.XhtmlIndex;
        }

        private void ProcessTags()
        {
            if (IsLastCharacterInBuffer)
            {
                fXhtmlTagger.ProcessTagsBuffer(ProcessingXhtmlIndex, CurrentTokenIndex);
            }
        }        

        private bool NextCharacter()
        {
            bool hasNext = fCharacterBuffer.NextCharacter() || ReadXhtmlToPlainText();
            if (hasNext)
            {
                char c = fCharacterBuffer.NextOfNextCharacterInfo.Character;
                WordBreak wordBreak = WordBreakTable.GetCharacterWordBreak(c);
                fWordBreaker.AddWordBreak(wordBreak);
            }
            else if (HasCharacters())
            {
                MoveNext();
                hasNext = true;
            }
            return hasNext;
        }
        
        private bool IsBreak()
        {
            return fWordBreaker.IsBreak() || (IsLastCharacterInBuffer && fXhtmlTagger.IsBreak(ProcessingCharacterIndex, ProcessingXhtmlIndex));
        }

        private bool ReadXhtmlToPlainText()
        {
            bool plainTextFound = false;
            while (!plainTextFound && !fEndOfFile && fXmlReader.Read())
            {                
                switch (fXmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                        ProcessElement();
                        break;
                    case XmlNodeType.Text:
                        plainTextFound = true;
                        ProcessText();
                        break;                    
                }
            }
            return plainTextFound;
        }

        private void ProcessElement()
        {
            string tagName = fXmlReader.Name;
            TagKind tagKind = GetCurrentTagKind();
            string elementRepresentation = ElementRepresentation(tagKind, tagName);
            SaveXhtmlElement(elementRepresentation);
            if (tagKind != TagKind.Empty)
                fXhtmlTagger.ProcessXhtmlTag(tagName, tagKind, fPlainTextXhtmlIndex, fCharacterBuffer.NextOfNextCharacterInfo.StringPosition);
            if (fDocumentTagsMode)
                fEndOfFile = (tagKind == TagKind.Close) && tagName.Equals(BodyTag);
        }

        private TagKind GetCurrentTagKind()
        {
            TagKind result;
            if (fXmlReader.NodeType == XmlNodeType.Element)
                result = (fXmlReader.IsEmptyElement) ? TagKind.Empty : TagKind.Open;
            else
                result = TagKind.Close;
            return result;
        }

        private void ProcessText()
        {
            string plainText = fXmlReader.Value;
            fParsedText.AddPlainTextElement(plainText);
            fCharacterBuffer.SetBuffer(plainText, XhtmlIndex);
            fPlainTextXhtmlIndex = XhtmlIndex;
        }

        private void MoveNext()
        {
            fCharacterBuffer.MoveNext();
            fWordBreaker.NextWordBreak();
        }

        private bool HasCharacters()
        {
            return !fWordBreaker.IsEmptyBuffer();
        }

        private void SaveToken(int currentTokenLength)
        {
            var token = new TokenReference
            {
                TokenKind = fTokenClassifier.TokenKind,
                XhtmlIndex = fTokenStartXhtmlIndex,
                StringPosition = fTokenStartPosition,
                StringLength = currentTokenLength,
                IsHexadecimal = fTokenClassifier.IsHexadecimal,
            };
            fParsedText.AddToken(token);
            fTokenStartPosition = fCharacterBuffer.NextCharacterInfo.StringPosition;
            fTokenStartXhtmlIndex = fCharacterBuffer.NextCharacterInfo.XhtmlIndex;
            fTokenClassifier.Reset();
        }

        private void SaveXhtmlElement(string element)
        {
            fParsedText.AddXhtmlElement(element);
        }

        private static bool IsIgnoreableNode(XmlNodeType nodeType)
        {
            return (nodeType == XmlNodeType.Comment) || (nodeType == XmlNodeType.Whitespace) || (nodeType == XmlNodeType.XmlDeclaration);
        }

        private static string RepresentOpenTag(string elementName)
        {
            return $"<{elementName}>";
        }

        private static string RepresentEndTag(string elementName)
        {
            return $"</{elementName}>";
        }

        private static string RepresentEmptyTag(string elementName)
        {
            return $"<{elementName}/>";
        }

        private static string ElementRepresentation(TagKind tagKind, string tagName)
        {
            string result = string.Empty;
            switch (tagKind)
            {
                case TagKind.Open:
                    result = RepresentOpenTag(tagName);
                    break;
                case TagKind.Empty:
                    result = RepresentEmptyTag(tagName);
                    break;
                case TagKind.Close:
                    result = RepresentEndTag(tagName);
                    break;
            }
            return result;
        }        
    }

    internal class CharacterBuffer
    {
        private CharacterPosition fCurrentCharacterPosition;
        private CharacterPosition fNextCharacterPosition;
        private CharacterPosition fPosition;

        // Public

        public CharacterPosition CurrentCharacterInfo => fCurrentCharacterPosition;
        public CharacterPosition NextCharacterInfo => fNextCharacterPosition;
        public CharacterPosition NextOfNextCharacterInfo => fPosition;

        public void SetBuffer(string buffer, int xhtmlIndex)
        {
            MoveNext();
            fPosition.Buffer = buffer;
            fPosition.StringPosition = 0;
            fPosition.XhtmlIndex = xhtmlIndex;
        }

        public bool NextCharacter()
        {
            bool result;
            if ((!string.IsNullOrEmpty(fPosition.Buffer)) && (fPosition.StringPosition < fPosition.Buffer.Length - 1))
            {
                result = true;
                MoveNext();
                fPosition.StringPosition++;
            }
            else
                result = false;
            return result;
        }

        public void MoveNext()
        {
            fCurrentCharacterPosition = NextCharacterInfo;
            fNextCharacterPosition = fPosition;
        }
    }

    internal struct CharacterPosition
    {
        public string Buffer;
        public int XhtmlIndex;
        public int StringPosition;
        public char Character => Buffer[StringPosition];
        public bool IsLastCharacterInBuffer => Buffer == null || StringPosition == Buffer.Length - 1;
    }
}
