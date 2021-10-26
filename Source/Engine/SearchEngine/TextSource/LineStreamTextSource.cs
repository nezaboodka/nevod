//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nezaboodka.Text.Parsing;

using NTTokenKind = Nezaboodka.Text.Parsing.TokenKind;

namespace Nezaboodka.Nevod
{
    public class LineStreamTextSource : TextSource
    {
        private char[] fBuffer;
        private TextReader fReader;
        private long fStringPositionBase;
        private long fTokenNumberBase;
        private ParsedText fCurrentParsedText;

        private long fPreviousStringPositionBase;
        private long fPreviousTokenNumberBase;
        private ParsedText fPreviousParsedText;

        public bool ShouldCloseReader { get; }
        public int BufferSizeInChars { get; }

        public LineStreamTextSource(Stream textStream, int bufferSizeInChars = DefaultBufferSizeInChars)
            : this(new StreamReader(textStream), shouldCloseReader: true, bufferSizeInChars)
        {
        }

        public LineStreamTextSource(TextReader reader, bool shouldCloseReader = false,
            int bufferSizeInChars = DefaultBufferSizeInChars)
        {
            fReader = reader;
            ShouldCloseReader = shouldCloseReader;
            BufferSizeInChars = bufferSizeInChars;
            fBuffer = new char[bufferSizeInChars];
        }

        public override IEnumerator<Token> GetEnumerator()
        {
            // Разобрать пустую строку для получения ParsedText, который содержит Start
            // (для обработки ситуации, когда текст захватывается, начиная с самого первого токена).
            var parserOptions = new PlainTextParserOptions()
            {
                ProduceStartAndEndTokens = true,
                DetectParagraphs = false
            };
            fCurrentParsedText = PlainTextParser.Parse(string.Empty, parserOptions);
            int tokensCount = fCurrentParsedText.PlainTextTokens.Count;
            TextLocationContext tokenContext;
            int charsRead = fReader.Read(fBuffer, 0, BufferSizeInChars);
            bool isEndOfStream = (charsRead == 0);
            if (!isEndOfStream)
            {
                // Изъять End, чтобы добавить в конец последнего блока.
                TokenReference endTokenReference = fCurrentParsedText.PlainTextTokens[tokensCount - 1];
                fCurrentParsedText.PlainTextTokens.RemoveAt(tokensCount - 1);
                Token startToken = GetToken(fCurrentParsedText, 0);
                fTokenNumberBase = 1;
                fStringPositionBase = 0;
                // Отключить вывод парсером токенов Start и End при разборе блоков.
                parserOptions.ProduceStartAndEndTokens = false;
                var textBuilder = new StringBuilder();
                int previousLastTokenLength = 0;
                int lineStart;
                int lineEnd;
                int lastLineBreakIndex = -1;
                Token lineBreakToken;
                TextLocation previousLineBreakLocation;
                TextLocation currentLineBreakLocation = startToken.Location;
                while (!isEndOfStream || textBuilder.Length != 0)
                {
                    textBuilder.Append(fBuffer, 0, charsRead);
                    string textBlock = textBuilder.ToString();
                    textBuilder.Clear();
                    fPreviousParsedText = fCurrentParsedText;
                    fCurrentParsedText = PlainTextParser.Parse(textBlock, parserOptions);
                    tokensCount = fCurrentParsedText.PlainTextTokens.Count;
                    TokenReference lastToken = fCurrentParsedText.PlainTextTokens[tokensCount - 1];
                    int nextChar = fReader.Read();
                    isEndOfStream = (nextChar == -1);
                    if (!isEndOfStream)
                    {
                        // Отложить выдачу последнего токена, т.к. он может быть не полностью прочитан из входного
                        // потока в рамках текущего блока.
                        tokensCount--;
                        fCurrentParsedText.PlainTextTokens.RemoveAt(tokensCount);
                    }
                    else
                    {
                        // Конец потока => добавить End в конец последнего блока.
                        endTokenReference.StringPosition = textBlock.Length;
                        fCurrentParsedText.PlainTextTokens.Add(endTokenReference);
                        tokensCount++;
                    }
                    lineStart = 0;
                    lineEnd = GetNextLineBreakIndex(fCurrentParsedText, startIndex: lineStart);
                    if (lineEnd > -1)
                    {
                        lineBreakToken = GetToken(fCurrentParsedText, lineEnd);
                        lineBreakToken.Location.TokenNumber += fTokenNumberBase;
                        lineBreakToken.Location.Position += fStringPositionBase;
                        previousLineBreakLocation = currentLineBreakLocation;
                        currentLineBreakLocation = lineBreakToken.Location;
                        tokenContext = new TextLocationContext(previousLineBreakLocation, currentLineBreakLocation);
                        lineBreakToken.Location.Context = tokenContext;
                    }
                    else
                    {
                        // В текущем блоке нет перевода строки => невозможно указать контекст.
                        tokenContext = null;
                        lineBreakToken = null;
                    }
                    // Выдать токены из предыдущего блока, попавшие в строку, которая заканчивается в текущем блоке.
                    lineStart = lastLineBreakIndex + 1;
                    for (int i = lineStart, n = fPreviousParsedText.PlainTextTokens.Count; i < n; i++)
                    {
                        Token nextToken = GetToken(fPreviousParsedText, i);
                        nextToken.Location.TokenNumber += fPreviousTokenNumberBase;
                        nextToken.Location.Position += fPreviousStringPositionBase;
                        nextToken.Location.Context = tokenContext;
                        yield return nextToken;
                    }
                    lastLineBreakIndex = lineEnd;
                    lineStart = 0;
                    while (lineEnd > -1) // => lineBreakToken != null
                    {
                        for (int i = lineStart; i < lineEnd; i++)
                        {
                            Token nextToken = GetToken(fCurrentParsedText, i);
                            nextToken.Location.TokenNumber += fTokenNumberBase;
                            nextToken.Location.Position += fStringPositionBase;
                            nextToken.Location.Context = tokenContext;
                            yield return nextToken;
                        }
                        yield return lineBreakToken;
                        lineStart = lineEnd + 1;
                        lastLineBreakIndex = lineEnd;
                        lineEnd = GetNextLineBreakIndex(fCurrentParsedText, startIndex: lineStart);
                        if (lineEnd > -1)
                        {
                            lineBreakToken = GetToken(fCurrentParsedText, lineEnd);
                            lineBreakToken.Location.TokenNumber += fTokenNumberBase;
                            lineBreakToken.Location.Position += fStringPositionBase;
                            previousLineBreakLocation = new TextLocation(currentLineBreakLocation);
                            currentLineBreakLocation = lineBreakToken.Location;
                            tokenContext = new TextLocationContext(previousLineBreakLocation, currentLineBreakLocation);
                            lineBreakToken.Location.Context = tokenContext;
                        }
                    }
                    fPreviousTokenNumberBase = fTokenNumberBase;
                    fTokenNumberBase += tokensCount;
                    fPreviousStringPositionBase = fStringPositionBase;
                    fStringPositionBase += lastToken.StringPosition;
                    if (!isEndOfStream)
                    {
                        // Добавить текст последнего токена к следующему блоку.
                        int lastTokenStartIndex = lastToken.StringPosition - previousLastTokenLength;
                        textBuilder.Append(fBuffer, lastTokenStartIndex, lastToken.StringLength);
                        textBuilder.Append((char)nextChar);
                        previousLastTokenLength = lastToken.StringLength + 1;
                    }
                    else
                        previousLastTokenLength = lastToken.StringLength;
                    charsRead = fReader.Read(fBuffer, 0, BufferSizeInChars);
                    isEndOfStream = (charsRead == 0);
                }
                // Корректировка для GetText, т.к. End был добавлен к последнему блоку.
                fTokenNumberBase = fPreviousTokenNumberBase;
            }
            else // if (isEndOfStream == true)
            {
                // Поток пуст => выдать только Start и End.
                Token startToken = GetToken(fCurrentParsedText, 0);
                Token endToken = GetToken(fCurrentParsedText, 1);
                tokenContext = new TextLocationContext(startToken.Location, endToken.Location);
                startToken.Location.Context = tokenContext;
                endToken.Location.Context = tokenContext;
                yield return startToken;
                yield return endToken;
            }
            if (ShouldCloseReader)
                fReader.Close();
        }

        public override string GetText(TextLocation start, TextLocation end)
        {
            string result;
            long startTokenNumber = start.TokenNumber;
            if (startTokenNumber < 0)
                startTokenNumber = ~startTokenNumber;
            long endTokenNumber;
            if (start != end)
            {
                endTokenNumber = end.TokenNumber;
                if (endTokenNumber < 0)
                    endTokenNumber = ~endTokenNumber;
            }
            else
                endTokenNumber = startTokenNumber;
            if (endTokenNumber >= startTokenNumber)
            {
                // Проверка на вхождение текста в буфер.
                if (startTokenNumber >= fTokenNumberBase)
                {
                    // Целиком в текущем блоке.
                    int internalStartTokenNumber = (int)(startTokenNumber - fTokenNumberBase);
                    int internalEndTokenNumber = (int)(endTokenNumber - fTokenNumberBase);
                    result = GetTextInternal(fCurrentParsedText, internalStartTokenNumber, internalEndTokenNumber);
                }
                else if (startTokenNumber >= fPreviousTokenNumberBase) // && (startTokenNumber < fTokenNumberBase)
                {
                    // Начало в предыдущем блоке.
                    int internalStartTokenNumber = (int)(startTokenNumber - fPreviousTokenNumberBase);
                    if (endTokenNumber < fTokenNumberBase)
                    {
                        // Конец в предыдущем блоке => целиком в предыдущем блоке.
                        int internalEndTokenNumber = (int)(endTokenNumber - fPreviousTokenNumberBase);
                        result = GetTextInternal(fPreviousParsedText, internalStartTokenNumber, internalEndTokenNumber);
                    }
                    else // if (endTokenNumber >= fTokenNumberBase)
                    {
                        // Конец в текущем блоке => пересечение предыдущего и текущего блока.
                        int previousLastTokenNumber = fPreviousParsedText.PlainTextTokens.Count - 1;
                        string first = GetTextInternal(fPreviousParsedText, internalStartTokenNumber, previousLastTokenNumber);
                        endTokenNumber -= fTokenNumberBase;
                        int internalEndTokenNumber = (int)(endTokenNumber - fTokenNumberBase);
                        string second = GetTextInternal(fCurrentParsedText, 0, internalEndTokenNumber);
                        if (second != null) // endTokenNumber не выходит за границу текущего блока
                            result = string.Concat(first, second);
                        else
                            result = null;
                    }
                }
                else
                    result = null;
            }
            else
                throw new ArgumentException($"'{nameof(end)}' should be greater than '{nameof(start)}'.");
            return result;
        }

        internal static LineStreamTextSource FromString(string text, bool withReader,
            int bufferSizeInChars = DefaultBufferSizeInChars)
        {
            byte[] textBytes = Encoding.ASCII.GetBytes(text);
            var textStream = new MemoryStream(textBytes);
            LineStreamTextSource result;
            if (withReader)
            {
                var reader = new StreamReader(textStream);
                result = new LineStreamTextSource(reader, shouldCloseReader: true, bufferSizeInChars);
            }
            else
                result = new LineStreamTextSource(textStream, bufferSizeInChars);
            return result;
        }

        public const int DefaultBufferSizeInChars = 500_000;

        // Internal

        private int GetNextLineBreakIndex(ParsedText parsedText, int startIndex)
        {
            int count = parsedText.PlainTextTokens.Count;
            int i = startIndex;
            while (i < count
                && parsedText.PlainTextTokens[i].TokenKind != NTTokenKind.LineFeed
                && parsedText.PlainTextTokens[i].TokenKind != NTTokenKind.End)
            {
                i++;
            }
            int result;
            if (i < count)
                result = i;
            else
                result = -1;
            return result;
        }

        private string GetTextInternal(ParsedText parsedText, int startTokenNumber, int endTokenNumber)
        {
            string result;
            int count = parsedText.PlainTextTokens.Count;
            if (startTokenNumber < count && endTokenNumber < count)
            {
                if (startTokenNumber != endTokenNumber)
                {
                    var sb = new StringBuilder();
                    for (int i = startTokenNumber; i <= endTokenNumber; i++)
                        sb.Append(parsedText.GetTokenText(parsedText.PlainTextTokens[i]));
                    result = sb.ToString();
                }
                else
                    result = parsedText.GetTokenText(parsedText.PlainTextTokens[startTokenNumber]);
            }
            else
                result = null;
            return result;
        }

        private Token GetToken(ParsedText parsedText, int tokenNumber)
        {
            TokenReference tokenReference = parsedText.PlainTextTokens[tokenNumber];
            Token result = new Token(TokenKindByTokenReferenceKind[(int)tokenReference.TokenKind],
                WordClassByTokenReferenceKind[(int)tokenReference.TokenKind],
                parsedText.GetTokenText(tokenReference),
                new TextLocation(tokenNumber, tokenReference.StringPosition, tokenReference.StringLength));
            return result;
        }
    }
}
