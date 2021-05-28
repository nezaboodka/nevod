//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nezaboodka.Text.Parsing
{
    internal class XhtmlTagger
    {
        private readonly ParsedText fParsedText;
        private readonly Queue<TagBufferItem> fTagsBuffer = new Queue<TagBufferItem>();
        private readonly Dictionary<string, TagState> fTagsInfo = new Dictionary<string, TagState>()
        {
            {"p", new TagState("Paragraph") },
            {"h1", new TagState("Heading") },
            {"h2", new TagState("Heading") },
            {"h3", new TagState("Heading") },
            {"h4", new TagState("Heading") },
            {"h5", new TagState("Heading") },
            {"h6", new TagState("Heading") }
        };
        private bool IsEmptyBuffer => fTagsBuffer.Count == 0;

        // Public

        public XhtmlTagger(ParsedText parsedText)
        {
            fParsedText = parsedText;
        }

        public void ProcessXhtmlTag(string xhtmlTagName, TagKind tagKind, int plainTextXhtmlIndex, int characterIndex)
        {
            TagState tagState;
            if (fTagsInfo.TryGetValue(xhtmlTagName, out tagState))
            {
                var tagBufferItem = new TagBufferItem(tagState, tagKind, plainTextXhtmlIndex);
                fTagsBuffer.Enqueue(tagBufferItem);
            }            
        }
        
        public void ProcessTagsBuffer(int xhtmlIndex, int tokenPosition)
        {
            bool hasUnprocessedTags = true;
            while (!IsEmptyBuffer && hasUnprocessedTags)
            {
                TagBufferItem tagBufferItem = fTagsBuffer.Peek();
                if (tagBufferItem.PlainTextXhtmlIndex == xhtmlIndex)
                {
                    TagState tagState = tagBufferItem.TagState;
                    switch (tagBufferItem.TagKind)
                    {
                        case TagKind.Open:                            
                            tagState.StartTokenPosition = tokenPosition + 1;
                            break;
                        case TagKind.Close:
                            int tokenLength = tokenPosition - tagState.StartTokenPosition + 1;
                            if (tokenLength > 0)
                            {
                                SaveTag(tagState, tokenLength);
                            }
                            break;
                    }
                    fTagsBuffer.Dequeue();
                }
                else
                {
                    hasUnprocessedTags = false;
                }
            }
        }

        public bool IsBreak(int characterIndex, int plainTextXhtmlIndex)
        {
            bool result = false;
            if (!IsEmptyBuffer)
            {
                TagBufferItem lastBufferItem = fTagsBuffer.Peek();
                result = (lastBufferItem.PlainTextXhtmlIndex == plainTextXhtmlIndex);
            }
            return result;
        }

        // Internal        

        private void SaveTag(TagState tagState, int tokenLength)
        {            
            var tag = new FormattingTag
            {
                TagName = tagState.TagName,
                TokenPosition = tagState.StartTokenPosition,
                TokenLength = tokenLength
            };
            fParsedText.AddTag(tag);
            tagState.Close();
        }               
    }

    internal class TagBufferItem
    {
        public TagState TagState { get; }
        public int PlainTextXhtmlIndex { get; }
        public TagKind TagKind { get; }

        public TagBufferItem(TagState tagState, TagKind tagKind, int plainTextXhtmlIndex)
        {
            TagState = tagState;
            TagKind = tagKind;
            PlainTextXhtmlIndex = plainTextXhtmlIndex;
        }
    }

    internal enum TagKind
    {
        Open,
        Close,
        Empty
    }

    internal class TagState
    {
        public string TagName { get; }
        public int StartTokenPosition { get; set; }
        public bool IsOpen => StartTokenPosition != -1;

        public TagState(string tagName)
        {
            TagName = tagName;
            StartTokenPosition = -1;
        }

        public void Close()
        {
            StartTokenPosition = -1;
        }
    }
}
