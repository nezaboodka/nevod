//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    internal class SyntaxStringBuilder : SyntaxVisitor
    {
        private StringBuilder fOutput;
        private Stack<Syntax> fParents;
        private string fLastNamespace;
        private int fWrittenNewLines;
        private int fRequiredNewLines;
        private int fIndent;
        private string fIndentationText;

        public static string NewLine = "\n";

        internal SyntaxStringBuilder()
        {
            fOutput = new StringBuilder();
            fParents = new Stack<Syntax>();
            fLastNamespace = string.Empty;
            fWrittenNewLines = 2;
            fIndentationText = "    ";
        }

        public override string ToString()
        {
            return fOutput.ToString();
        }

        internal static string SyntaxToString(Syntax node)
        {
            var sb = new SyntaxStringBuilder();
            sb.Visit(node);
            if (sb.fRequiredNewLines > sb.fWrittenNewLines)
                sb.WriteLineBreak();
            return sb.ToString();
        }

        protected internal override Syntax VisitPackage(PackageSyntax node)
        {
            fParents.Push(node);
            if (node.RequiredPackages.Count > 0)
            {
                RequireBeginningOfParagraph();
                Visit(node.RequiredPackages);
            }
            if (node.SearchTargets.Count > 0)
            {
                RequireBeginningOfParagraph();
                WriteChildrenLines(node.SearchTargets);
                WriteLineBreak();
            }
            if (node.Patterns.Count > 0)
            {
                RequireBeginningOfParagraph();
                WriteChildrenLines(node.Patterns);
                WriteLineBreak();
                WriteNamespaceIfDifferentFromLast(string.Empty);
            }
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitLinkedPackage(LinkedPackageSyntax node)
        {
            VisitPackage(node);
            return node;
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            RequireBeginningOfLine();
            Write("@require ");
            Write('"');
            Write(node.RelativePath);
            Write('"');
            WriteLineBreak();
            return node;
        }

        protected internal override Syntax VisitPatternSearchTarget(PatternSearchTargetSyntax node)
        {
            if (fParents.TryPeek(out Syntax parent))
            {
                PackageSyntax package = (PackageSyntax)parent;
                if (!package.Patterns.Any(x => ((PatternSyntax)x).IsSearchTarget && ((PatternSyntax)x).FullName == node.SearchTarget))
                    WriteSearchTarget(node);
            }
            else
                WriteSearchTarget(node);
            return node;
        }

        protected internal override Syntax VisitNamespaceSearchTarget(NamespaceSearchTargetSyntax node)
        {
            WriteSearchTarget(node);
            return node;
        }

        private void WriteSearchTarget(SearchTargetSyntax node)
        {
            Write("@search ");
            Write(node.SearchTarget);
            Write(';');
            WriteLineBreak();
        }

        protected internal override Syntax VisitPattern(PatternSyntax node)
        {
            fParents.Push(node);
            WriteNamespaceIfDifferentFromLast(node.Namespace);
            RequireBeginningOfLine();
            Write("@pattern ");
            if (node.IsSearchTarget)
                Write('#');
            Write(node.Name);
            if (node.Fields.Count > 0)
            {
                Write('(');
                WriteChildren(node.Fields, ", ");
                Write(')');
            }
            Write(" = ");
            Visit(node.Body);
            if (node.NestedPatterns.Count > 0)
            {
                RequireBeginningOfLine();
                Write("@where");
                WriteLineBreak();
                Write("{");
                WriteLineBreak();
                AddIndent();
                Visit(node.NestedPatterns);
                RemoveIndent();
                RequireBeginningOfLine();
                Write("}");
            }
            Write(';');
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitField(FieldSyntax node)
        {
            Write(node.Name);
            return node;
        }

        protected internal override Syntax VisitPatternReference(PatternReferenceSyntax node)
        {
            fParents.Push(node);
            Write(node.PatternName);
            if (node.ExtractionFromFields.Count > 0)
            {
                Write('(');
                WriteChildren(node.ExtractionFromFields, ", ");
                Write(')');
            }
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitEmbeddedPatternReference(EmbeddedPatternReferenceSyntax node)
        {
            VisitPatternReference(node);
            return node;
        }

        protected internal override Syntax VisitFieldReference(FieldReferenceSyntax node)
        {
            Write(node.FieldName);
            return node;
        }

        protected internal override Syntax VisitSequence(SequenceSyntax node)
        {
            fParents.Push(node);
            WriteChildren(node.Elements, " + ");
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitWordSequence(WordSequenceSyntax node)
        {
            fParents.Push(node);
            WriteChildren(node.Elements, " _ ");
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitConjunction(ConjunctionSyntax node)
        {
            fParents.Push(node);
            WriteChildren(node.Elements, " & ");
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitVariation(VariationSyntax node)
        {
            fParents.Push(node);
            Write('{');
            WriteChildren(node.Elements, ", ");
            Write('}');
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitSpan(SpanSyntax node)
        {
            fParents.Push(node);
            Write('[');
            WriteChildren(node.Elements, ", ");
            Write(']');
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitRepetition(RepetitionSyntax node)
        {
            fParents.Push(node);
            WriteNumericRange(node.RepetitionRange);
            Write(" ");
            WriteChild(node.Body);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitOptionality(OptionalitySyntax node)
        {
            fParents.Push(node);
            Write("? ");
            WriteChild(node.Body);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitException(ExceptionSyntax node)
        {
            fParents.Push(node);
            Write('~');
            WriteChild(node.Body);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitWordSpan(WordSpanSyntax node)
        {
            fParents.Push(node);
            WriteChild(node.Left, isLeft: true);
            Write(" .. ");
            if (node.ExtractionOfSpan != null)
            {
                WriteChild(node.ExtractionOfSpan);
                Write(':');
            }
            Write('[');
            WriteNumericRange(node.SpanRange);
            Write(']');
            if (node.Exclusion != null)
            {
                Write(" ~");
                WriteChild(node.Exclusion);
            }
            Write(" .. ");
            WriteChild(node.Right, isLeft: false);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitAnySpan(AnySpanSyntax node)
        {
            fParents.Push(node);
            WriteChild(node.Left, isLeft: true);
            if (node.ExtractionOfSpan != null)
            {
                Write(" .. ");
                WriteChild(node.ExtractionOfSpan);
                Write(" .. ");
            }
            else
                Write(" ... ");
            WriteChild(node.Right, isLeft: false);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitInside(InsideSyntax node)
        {
            fParents.Push(node);
            WriteChild(node.Inner, isLeft: true);
            Write(" @inside ");
            WriteChild(node.Outer, isLeft: false);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitOutside(OutsideSyntax node)
        {
            fParents.Push(node);
            WriteChild(node.Body, isLeft: true);
            Write(" @outside ");
            WriteChild(node.Exception, isLeft: false);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitHaving(HavingSyntax node)
        {
            fParents.Push(node);
            WriteChild(node.Outer, isLeft: true);
            Write(" @having ");
            WriteChild(node.Inner, isLeft: false);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitExtraction(ExtractionSyntax node)
        {
            fParents.Push(node);
            Write(node.FieldName);
            if (node.Body != null)
            {
                Write(": ");
                WriteChild(node.Body);
            }
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitExtractionFromField(ExtractionFromFieldSyntax node)
        {
            fParents.Push(node);
            Write(node.FieldName);
            Write(": ");
            Write(node.FromFieldName);
            fParents.Pop();
            return node;
        }

        protected internal override Syntax VisitText(TextSyntax node)
        {
            WriteText(node.Text, node.IsCaseSensitive, node.TextIsPrefix, node.SuffixAttributes);
            return node;
        }

        protected internal override Syntax VisitToken(TokenSyntax node)
        {
            if (!string.IsNullOrEmpty(node.Text))
            {
                if (node.TokenKind == TokenKind.Word)
                    WriteText(node.Text, node.IsCaseSensitive, node.TextIsPrefix, node.TokenAttributes as WordAttributes);
                else
                    WriteText(node.Text, isCaseSensitive: false, textIsPrefix: false, node.TokenAttributes as WordAttributes);
            }
            else
            {
                if (node.TokenKind == TokenKind.Word)
                    WriteWord(node.TokenAttributes as WordAttributes);
                else
                    WriteToken(node.TokenKind, node.TokenAttributes);
            }
            return node;
        }

        internal void WriteStrings(ReadOnlyCollection<string> elements, string elementPrefix, string elementsSeparator)
        {
            if (elements != null && elements.Count > 0)
            {
                for (int i = 0, n = elements.Count; i < n; i++)
                {
                    Write(elementPrefix);
                    Write(elements[i]);
                    if (i < n - 1)
                        Write(elementsSeparator);
                }
            }
        }

        internal void WriteChildren<T>(ReadOnlyCollection<T> nodes, string elementPrefix, string elementsSeparator)
            where T : Syntax
        {
            if (nodes.Count > 0)
            {
                for (int i = 0, n = nodes.Count; i < n; i++)
                {
                    Write(elementPrefix);
                    WriteChild(nodes[i]);
                    if (i < n - 1)
                        Write(elementsSeparator);
                }
            }
        }

        internal void WriteChildren<T>(ReadOnlyCollection<T> nodes, string elementsSeparator) where T : Syntax
        {
            if (nodes.Count > 0)
            {
                for (int i = 0, n = nodes.Count; i < n; i++)
                {
                    WriteChild(nodes[i]);
                    if (i < n - 1)
                        Write(elementsSeparator);
                }
            }
        }

        internal void WriteChildrenLines<T>(ReadOnlyCollection<T> nodes) where T : Syntax
        {
            if (nodes.Count > 0)
            {
                for (int i = 0, n = nodes.Count; i < n; i++)
                {
                    WriteChild(nodes[i]);
                    if (i < n - 1)
                        WriteLineBreak();
                }
            }
        }

        internal void WriteChild(Syntax node)
        {
            bool requiresParenthesis = RequiresParenthesis(node);
            if (requiresParenthesis)
                Write('(');
            Visit(node);
            if (requiresParenthesis)
                Write(')');
        }

        internal void WriteChild(Syntax node, bool isLeft)
        {
            bool requiresParenthesis = RequiresParenthesis(node, isLeft);
            if (requiresParenthesis)
                Write('(');
            Visit(node);
            if (requiresParenthesis)
                Write(')');
        }

        private void WriteNamespaceIfDifferentFromLast(string name)
        {
            if (name != fLastNamespace)
            {
                if (!string.IsNullOrEmpty(fLastNamespace))
                {
                    RequireBeginningOfLine();
                    RemoveIndent();
                    Write("}");
                    WriteLineBreak();
                    RequireBeginningOfParagraph();
                }
                if (!string.IsNullOrEmpty(name))
                {
                    RequireBeginningOfParagraph();
                    Write("@namespace ");
                    Write(name);
                    WriteLineBreak();
                    Write("{");
                    WriteLineBreak();
                    AddIndent();
                }
                fLastNamespace = name;
            }
        }

        private void WriteText(string text, bool isCaseSensitive, bool textIsPrefix, WordAttributes attributes)
        {
            Write('"');
            Write(text);
            Write('"');
            if (isCaseSensitive)
                Write('!');
            if (textIsPrefix)
                Write('*');
            if (attributes != null)
                WriteWordAttributes(attributes, includeWordClass: true);
        }

        private void WriteNumericRange(Range range)
        {
            Write(range.LowBound.ToString());
            if (range.HighBound != range.LowBound)
            {
                if (range.HighBound != Range.Max)
                {
                    Write('-');
                    Write(range.HighBound.ToString());
                }
                else
                    Write('+');
            }
        }

        private void WriteWord(WordAttributes attributes)
        {
            if (attributes != null)
            {
                if (attributes.WordClass == WordClass.Any)
                {
                    string tokenKind = TokenKindToString(TokenKind.Word);
                    Write(tokenKind);
                }
                else
                {
                    string wordClass = WordClassToString(attributes.WordClass);
                    Write(wordClass);
                }
                WriteWordAttributes(attributes, includeWordClass: false);
            }
            else
            {
                string tokenKind = TokenKindToString(TokenKind.Word);
                Write(tokenKind);
            }
        }

        private void WriteWordAttributes(WordAttributes attributes, bool includeWordClass)
        {
            if (attributes != null)
            {
                bool requiresParenthesis = includeWordClass && attributes.WordClass != WordClass.Any ||
                    !attributes.LengthRange.IsZeroPlus() || attributes.CharCase != CharCase.Undefined;
                if (requiresParenthesis)
                    Write("(");
                bool needComma = false;
                if (includeWordClass && attributes.WordClass != WordClass.Any)
                {
                    var wordClass = WordClassToString(attributes.WordClass);
                    Write(wordClass);
                    needComma = true;
                }
                if (!attributes.LengthRange.IsZeroPlus())
                {
                    if (needComma)
                        Write(", ");
                    WriteNumericRange(attributes.LengthRange);
                    needComma = true;
                }
                if (attributes.CharCase != CharCase.Undefined)
                {
                    if (needComma)
                        Write(", ");
                    var charCase = CharCaseToString(attributes.CharCase);
                    Write(charCase);
                }
                if (requiresParenthesis)
                    Write(")");
            }
        }

        private void WriteToken(TokenKind tokenKind, TokenAttributes attributes)
        {
            string tokenName = TokenKindToString(tokenKind);
            Write(tokenName);
            WriteTokenAttributes(attributes);
        }

        private void WriteTokenAttributes(TokenAttributes attributes)
        {
            if (attributes != null && !attributes.LengthRange.IsZeroPlus())
            {
                Write("(");
                WriteNumericRange(attributes.LengthRange);
                Write(")");
            }
        }

        private void Write(char ch)
        {
            Write(ch.ToString());
        }

        private void Write(string text)
        {
            if (text.Length > 0)
            {
                if (fRequiredNewLines > 0)
                {
                    for (int i = 0; i < fRequiredNewLines; i++)
                    {
                        fOutput.Append(NewLine);
                        fWrittenNewLines++;
                    }
                    fRequiredNewLines = 0;
                }
                if (text != NewLine && fWrittenNewLines > 0)
                {
                    for (int i = 0; i < fIndent; i++)
                        fOutput.Append(fIndentationText);
                }
                fOutput.Append(text);
                if (text == NewLine)
                    fWrittenNewLines++;
                else
                    fWrittenNewLines = 0;
            }
        }

        private void WriteLineBreak()
        {
            Write(NewLine);
        }

        private void AddIndent()
        {
            fIndent++;
        }

        private void RemoveIndent()
        {
            if (fIndent > 0)
                fIndent--;
        }

        private void SetNoIndent()
        {
            fIndent = 0;
        }

        private void RequireBeginningOfLine()
        {
            if (fWrittenNewLines + fRequiredNewLines < 1)
                fRequiredNewLines++;
        }

        private void RequireBeginningOfParagraph()
        {
            if (fWrittenNewLines + fRequiredNewLines < 2)
            {
                RequireBeginningOfLine();
                fRequiredNewLines++;
            }
        }

        private bool RequiresParenthesis(Syntax node)
        {
            bool result = RequiresParenthesis(node, true);
            return result;
        }

        private bool RequiresParenthesis(Syntax node, bool isLeftChild)
        {
            bool result = false;
            if (fParents.TryPeek(out Syntax parent))
                result = RequiresParenthesisForChild(parent, node, isLeftChild);
            return result;
        }

        private bool RequiresParenthesisForChild(Syntax parent, Syntax child, bool isLeftChild)
        {
            bool result = false;
            switch (parent)
            {
                case ExceptionSyntax _:
                case VariationSyntax _:
                case SpanSyntax _:
                case RepetitionSyntax _:
                case InsideSyntax _:
                    break;
                default:
                    result = RequiresParenthesisForChildAccordingPriorityLevel(parent, child, isLeftChild);
                    break;
            }
            return result;
        }

        private bool RequiresParenthesisForChildAccordingPriorityLevel(Syntax parent, Syntax child, bool isLeftChild)
        {
            bool result = false;
            int parentPriority = GetPriorityLevel(parent);
            int childPriority = GetPriorityLevel(child);
            if (childPriority > 0 && parentPriority > 0)
                result = parentPriority < childPriority ||
                    parentPriority == childPriority && !isLeftChild;
            return result;
        }

        private int GetPriorityLevel(Syntax x)
        {
            int result;
            switch (x)
            {
                case VariationSyntax _:
                    result = 1;
                    break;
                case SpanSyntax _:
                    result = 1;
                    break;
                case ExtractionSyntax _:
                    result = 1;
                    break;
                case OptionalitySyntax _:
                    result = 1;
                    break;
                case SequenceSyntax _:
                    result = 2;
                    break;
                case WordSequenceSyntax _:
                    result = 2;
                    break;
                case WordSpanSyntax _:
                    result = 3;
                    break;
                case AnySpanSyntax _:
                    result = 3;
                    break;
                case ConjunctionSyntax _:
                    result = 4;
                    break;
                case HavingSyntax _:
                    result = 5;
                    break;
                case InsideSyntax _:
                    result = 6;
                    break;
                case RepetitionSyntax _:
                    result = 7;
                    break;
                case ExceptionSyntax _:
                    result = 7;
                    break;
                default:
                    result = 0;
                    break;
            }
            return result;
        }

        public static string TokenKindToString(TokenKind tokenKind)
        {
            string result;
            switch (tokenKind)
            {
                case TokenKind.Word:
                    result = "Word";
                    break;
                case TokenKind.Space:
                    result = "Space";
                    break;
                case TokenKind.Punctuation:
                    result = "Punct";
                    break;
                case TokenKind.Symbol:
                    result = "Symbol";
                    break;
                case TokenKind.LineBreak:
                    result = "LineBreak";
                    break;
                case TokenKind.Start:
                    result = "Start";
                    break;
                case TokenKind.End:
                    result = "End";
                    break;
                default:
                    result = "<Undefined>";
                    break;
            }
            return result;
        }

        public static string WordClassToString(WordClass wordClass)
        {
            string result;
            switch (wordClass)
            {
                case WordClass.Alpha:
                    result = "Alpha";
                    break;
                case WordClass.Num:
                    result = "Num";
                    break;
                case WordClass.AlphaNum:
                    result = "AlphaNum";
                    break;
                case WordClass.NumAlpha:
                    result = "NumAlpha";
                    break;
                default:
                    result = "<Any>";
                    break;
            }
            return result;
        }

        public static string CharCaseToString(CharCase charCase)
        {
            string result;
            switch (charCase)
            {
                case CharCase.Lowercase:
                    result = "Lowercase";
                    break;
                case CharCase.Uppercase:
                    result = "Uppercase";
                    break;
                case CharCase.TitleCase:
                    result = "TitleCase";
                    break;
                default:
                    result = "<Undefined>";
                    break;
            }
            return result;
        }
    }
}
