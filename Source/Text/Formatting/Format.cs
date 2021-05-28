//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nezaboodka.Text.Formatting
{
    public struct FormatToken
    {
        public Slice Text;
        public Slice Name;
        public FormatTokenOptions Options;
    }

    public struct FormatTokenOptions
    {
        public Type TargetType;
        public bool MaxGrabMode;
        public char QuoteChar;
        public bool Quoted;
        public Slice Specifiers;
        public int Width;
    }

    public class Format
    {
        // TryParseText

        public static bool TryParseText(string format, string text, object target)
        {
            return TryParseText(format, text.Slice(), target);
        }

        public static bool TryParseText(string format, Slice textSlice, object target)
        {
            return TryParseText(format, textSlice, target, MemberTypes.All, false);
        }

        public static bool TryParseText(string format, string text, object target, MemberTypes members,
            bool ignoreUnknownTokens)
        {
            return TryParseText(format, text.Slice(), target, members, ignoreUnknownTokens);
        }

        public static bool TryParseText(string format, Slice textSlice, object target, MemberTypes members,
            bool ignoreUnknownTokens)
        {
            return TryParseText(format, textSlice, target, true,
                (obj, token) => SetMemberValue(obj, token, members, ignoreUnknownTokens));
        }

        public static bool TryParseText(string format, string text, object target, bool fullMatch)
        {
            return TryParseText(format, text.Slice(), target, fullMatch);
        }

        public static bool TryParseText(string format, Slice textSlice, object target, bool fullMatch)
        {
            var setter = (Action<object, FormatToken>)delegate(object obj, FormatToken token)
            {
                SetMemberValue(obj, token, MemberTypes.All, true);
            };
            return TryParseText(format, textSlice, target, false, setter);
        }

        public static bool TryParseText(string format, string text, object target, bool fullMatch,
            Action<object, FormatToken> setter)
        {
            return TryParseText(format, text.Slice(), target, fullMatch, setter);
        }

        public static bool TryParseText(string format, Slice textSlice, object target, bool fullMatch,
            Action<object, FormatToken> setter)
        {
            bool result = true;
            try
            {
                var tokens = ParseText(format, textSlice, fullMatch, false);
                foreach (var token in tokens)
                    setter(target, token);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        // ParseText

        public static void ParseText(string format, string text, object target)
        {
            ParseText(format, text.Slice(), target);
        }

        public static void ParseText(string format, Slice textSlice, object target)
        {
            ParseText(format, textSlice, target, MemberTypes.All, false);
        }

        public static void ParseText(string format, string text, object target, MemberTypes members,
            bool ignoreUnknownTokens)
        {
            ParseText(format, text.Slice(), target, members, ignoreUnknownTokens);
        }

        public static void ParseText(string format, Slice textSlice, object target, MemberTypes members,
            bool ignoreUnknownTokens)
        {
            ParseText(format, textSlice, target, true,
                (obj, token) => SetMemberValue(obj, token, members, ignoreUnknownTokens));
        }

        public static void ParseText(string format, string text, object target, bool fullMatch)
        {
            ParseText(format, text.Slice(), target, fullMatch);
        }

        public static void ParseText(string format, Slice textSlice, object target, bool fullMatch)
        {
            var setter = (Action<object, FormatToken>)delegate(object obj, FormatToken token)
            {
                SetMemberValue(obj, token, MemberTypes.All, true);
            };
            ParseText(format, textSlice, target, false, setter);
        }

        public static void ParseText(string format, string text, object target, bool fullMatch,
            Action<object, FormatToken> setter)
        {
            ParseText(format, text.Slice(), target, fullMatch, setter);
        }

        public static void ParseText(string format, Slice textSlice, object target, bool fullMatch,
            Action<object, FormatToken> setter)
        {
            var tokens = ParseText(format, textSlice, fullMatch, false);
            foreach (var token in tokens)
                setter(target, token);
        }

        public static IEnumerable<FormatToken> ParseText(string format, string text, bool fullMatch,
            bool yieldTerms)
        {
            return ParseText(format, text.Slice(), fullMatch, yieldTerms);
        }

        public static IEnumerable<FormatToken> ParseText(string format, Slice textSlice, bool fullMatch,
            bool yieldTerms)
        {
            var position = 0;
            var read = ReaderFactory.GetReader(ParseFormat(format));
            var token = default(FormatToken);
            while (read(out token))
            {
                if (Slice.IsNull(token.Name)) // if token is a term
                {
                    if (Slice.Compare(textSlice, position, token.Text, 0, token.Text.Length) == 0)
                    {
                        position += token.Text.Length;
                        if (yieldTerms)
                            yield return token;
                    }
                    else
                    {
                        if (fullMatch)
                            throw new ArgumentException(string.Format(
                                "'{0}' is expected at position {1}", token.Text, position));
                        else
                            break;
                    }
                }
                else
                {
                    if (token.Options.Width > 0)
                    {
                        token.Text = textSlice.SubSlice(position, token.Options.Width);
                        position = token.Text.Length;
                        yield return token;
                    }
                    else
                    {
                        var stop = default(FormatToken);
                        read(out stop); // stop = default(FormatToken) if end of text is reached
                        if (!Slice.IsNull(stop.Name))
                            throw new NotImplementedException();
                        token.Text = textSlice.SliceUntil(position, fullMatch, token.Options.MaxGrabMode, stop.Text);
                        if (token.Text.IsUndefined)
                            break;
                        position += token.Text.Length + (!Slice.IsNull(stop.Text) ? stop.Text.Length : 0);
                        yield return token;
                        if (yieldTerms)
                            yield return stop;
                    }
                }
            }
            if (position < textSlice.Length && fullMatch)
                throw new ArgumentException(string.Format(
                    "'{0}' is unexpected", textSlice.SubSlice(position)));
        }

        // EmitText

        public static string EmitText(object source, string format)
        {
            return EmitText(source, MemberTypes.All, format);
        }

        public static string EmitText(object source, MemberTypes members, string format)
        {
            return EmitText(token => GetMemberValue(source, token, members), format);
        }

        public static string EmitText(Func<FormatToken, object> source, string format)
        {
            var result = new StringBuilder();
            EmitText(source, format, result);
            return result.ToString();
        }

        public static void EmitText(object source, string format, StringBuilder result)
        {
            EmitText(source, MemberTypes.All, format, result);
        }

        public static void EmitText(object source, MemberTypes members, string format, StringBuilder result)
        {
            EmitText(token => GetMemberValue(source, token, members), format, result);
        }

        public static void EmitText(Func<FormatToken, object> source, string format, StringBuilder result)
        {
            var f = new StringBuilder();
            var args = new List<object>();
            PrepareForStringFormat(source, format, f, args);
            result.AppendFormat(f.ToString(), args.ToArray());
        }

        public static void EmitText<T>(IEnumerable<T> items, string itemFormat, string delimiter,
            MemberTypes members, StringBuilder result)
        {
            var first = true;
            foreach (var x in items)
            {
                if (!first)
                    result.Append(delimiter);
                else
                    first = false;
                Format.EmitText(x, members, itemFormat, result);
            }
        }

        // ParseFormat

        public static IEnumerable<FormatToken> ParseFormat(string format)
        {
            var tok = new Tokenizer(gSyntax) { SourceText = format };
            var completed = !tok.MoveNext();
            while (!completed)
            {
                var result = new FormatToken();
                if (tok.SkipAndSetSyntax(FormatSyntax.TkBraceLeft, gParameterSyntax))
                {
                    // Parse name, data type, and attributes
                    result.Name = tok.Take(FormatParameterSyntax.TkIdentifier);
                    if (tok.Skip(FormatParameterSyntax.TkColon))
                        result.Options.TargetType = GetTypeByName(tok.Take(FormatParameterSyntax.TkIdentifier));
                    while (tok.Skip(FormatParameterSyntax.TkSobaka))
                        GetAttribute(tok.Take(FormatParameterSyntax.TkIdentifier), ref result);
                    // Parse platform-specific format specifiers
                    tok.Syntax = gSyntax; // restore main syntax
                    if (tok.Skip(FormatParameterSyntax.TkSemicolon))
                        result.Options.Specifiers = tok.Take(FormatParameterSyntax.TkTextBlock);
                    tok.Take(FormatSyntax.TkBraceRight);
                    // Determine quotation character if needed
                    if (result.Options.Quoted)
                    {
                        var t = tok.Take(FormatSyntax.TkTextBlock);
                        result.Options.QuoteChar = t[0];
                        yield return result;
                        result = new FormatToken() { Text = t };
                    }
                }
                else
                    result.Text = tok.Take(FormatSyntax.TkTextBlock);
                yield return result;
                completed = tok.Eof;
            }
        }

        public static void PrepareForStringFormat(Func<FormatToken, object> lookup, string format, StringBuilder result, List<object> args)
        {
            foreach (var token in ParseFormat(format))
            {
                if (!Slice.IsNull(token.Name)) // if token is a parameter (variable)
                {
                    var f = token.Options.Specifiers;
                    result.Append("{");
                    result.Append(args.Count.ToString());
                    if (token.Options.Width > 0)
                        result.AppendFormat(",{0}", token.Options.Width);
                    if (!Slice.IsNull(f) && f.Length > 0)
                        { result.Append(":"); result.Append(f.Source, f.Position, f.Length); }
                    result.Append("}");
                    args.Add(lookup(token));
                }
                else // if token is a term
                {
                    var t = token.Text;
                    result.Append(t.Source, t.Position, t.Length);
                    if (t.Length == 1 && gBraces.Contains(t[0]))
                        result.Append(t.Source, t.Position, t.Length); // quote '{' and '}'
                }
            }
        }

        public static object NewObjectFromString(string format, Slice text, Type type)
        {
            var result = default(object);
            if (type == typeof(Slice))
                result = text;
            else if (type == typeof(string))
                result = text.ToString();
            else if (type == typeof(int))
            {
                if (format == "X" || format == "x")
                    result = int.Parse(text.ToString(), NumberStyles.HexNumber);
                else
                    result = int.Parse(text.ToString());
            }
            else if (type == typeof(long))
            {
                if (format == "X" || format == "x")
                    result = long.Parse(text.ToString(), NumberStyles.HexNumber);
                else
                    result = long.Parse(text.ToString());
            }
            else if (type == typeof(DateTime))
            {
                if (string.IsNullOrEmpty(format))
                    result = DateTime.Parse(text.ToString(), null, DateTimeStyles.AdjustToUniversal);
                else
                    result = DateTime.ParseExact(text.ToString(), format, null, DateTimeStyles.AdjustToUniversal);
            }
            else if (type == typeof(DateTimeOffset))
            {
                if (string.IsNullOrEmpty(format))
                    result = DateTimeOffset.Parse(text.ToString(), null, DateTimeStyles.AssumeUniversal);
                else
                    result = DateTimeOffset.ParseExact(text.ToString(), format, null, DateTimeStyles.AssumeUniversal);
            }
            else if (type == typeof(double))
                result = double.Parse(text.ToString(), NumberStyles.Any);
            else if (type == typeof(char))
                result = char.Parse(text.ToString());
            else if (type == typeof(bool))
                result = bool.Parse(text.ToString());
            else if (type == typeof (Guid))
            {
                if (string.IsNullOrEmpty(format))
                    result = Guid.Parse(text.ToString());
                else
                    result = Guid.ParseExact(text.ToString(), format);
            }
            else
            {
                MethodInfo method;
                object[] args = null;
                if (format == null)
                {
                    method = type.GetMethod("Parse", new Type[] { typeof(string) });
                    args = new object[] { text.ToString() };
                }
                else
                {
                    method = type.GetMethod("ParseExact", new Type[] { typeof(string), typeof(string) });
                    args = new object[] { text.ToString(), format };
                }
                if (method != null)
                    result = method.Invoke(null, args);
                else
                    throw new ArgumentException(string.Format(
                        "'{0}' type is not supported: cannot parse '{1}'", type.FullName, text.ToString()));
            }
            return result;
        }

        public static void SetMemberValue(object target, FormatToken token, MemberTypes members, bool ignoreUnknown)
        {
            Type type = target.GetType();
            string name = token.Name.ToString();
            string format = !Slice.IsNull(token.Options.Specifiers) ? token.Options.Specifiers.ToString() : null;
            PropertyInfo prop = (members & MemberTypes.Property) != 0 ? type.GetProperty(name) : null;
            if (prop == null)
            {
                var field = (members & MemberTypes.Field) != 0 ? type.GetField(name) : null;
                if (field == null)
                {
                    var method = (members & MemberTypes.Method) != 0 && token.Options.TargetType != null ?
                        type.GetMethod(name, new Type[] { token.Options.TargetType }) : null;
                    if (method == null)
                    {
                        if (!ignoreUnknown)
                            throw new ArgumentException(string.Format(
                                "'{0}' is not a member of '{1}' type", name, type.FullName));
                    }
                    else
                        method.Invoke(target, new object[] { Format.NewObjectFromString(format, token.Text,
                            token.Options.TargetType) });
                }
                else
                    field.SetValue(target, Format.NewObjectFromString(format, token.Text, field.FieldType));
            }
            else
                prop.SetValue(target, Format.NewObjectFromString(format, token.Text, prop.PropertyType), null);
        }

        public static object GetMemberValue(object source, FormatToken token, MemberTypes members)
        {
            var result = default(object);
            var type = source.GetType();
            var name = token.Name.ToString();
            var prop = (members & MemberTypes.Property) != 0 ? type.GetProperty(name) : null;
            if (prop == null)
            {
                var field = (members & MemberTypes.Field) != 0 ? type.GetField(name) : null;
                if (field == null)
                {
                    var method = (members & MemberTypes.Method) != 0 ? type.GetMethod(name, new Type[] { token.Options.TargetType }) : null;
                    if (method != null)
                        result = method.Invoke(source, null);
                }
                else
                    result = field.GetValue(source);
            }
            else
                result = prop.GetValue(source, null);
            return result;
        }

        // Implementation

        private static void GetAttribute(Slice option, ref FormatToken token)
        {
            if (string.Compare(option.Source, option.Position, "quoted", 0, option.Length, true) == 0)
                token.Options.Quoted = true;
            else if (string.Compare(option.Source, option.Position, "maxgrab", 0, option.Length, true) == 0)
                token.Options.MaxGrabMode = true;
            else
                throw new ArgumentException(string.Format(
                    "unknown option '{0}'", option.ToString()));
        }

        private static Type GetTypeByName(Slice name)
        {
            if (name.Length == 0)
                return null;
            else if (string.Compare(name.Source, name.Position, "string", 0, name.Length) == 0)
                return typeof(string);
            else if (string.Compare(name.Source, name.Position, "int", 0, name.Length) == 0)
                return typeof(int);
            else if (string.Compare(name.Source, name.Position, "long", 0, name.Length) == 0)
                return typeof(long);
            else if (string.Compare(name.Source, name.Position, "double", 0, name.Length) == 0)
                return typeof(double);
            else if (string.Compare(name.Source, name.Position, "char", 0, name.Length) == 0)
                return typeof(char);
            else if (string.Compare(name.Source, name.Position, "DateTime", 0, name.Length) == 0)
                return typeof(DateTime);
            else if (string.Compare(name.Source, name.Position, "DateTimeOffset", 0, name.Length) == 0)
                return typeof(DateTimeOffset);
            else if (string.Compare(name.Source, name.Position, "Guid", 0, name.Length) == 0)
                return typeof(Guid);
            else
                throw new ArgumentException(string.Format(
                    "unknown data type name '{0}'", name.ToString()));
        }

        // Fields
        private static FormatSyntax gSyntax = new FormatSyntax();
        private static FormatParameterSyntax gParameterSyntax = new FormatParameterSyntax();
        private static char[] gBraces = new char[] { '{', '}' };
    }

    public class ParsedValues
    {
        public string Value1;
        public string Value2;
        public string Value3;
        public string Value4;
        public string Value5;
        public string Value6;
        public string Value7;
        public string Value8;
        public string Value9;
    }

    public static class FormatSample
    {
        public class Properties
        {
            public string Author { get; set; }
            public DateTimeOffset Timestamp { get; set; }
            public int Age { get; set; }
            public string AgeMeasure { get; set; }
        }

        public static void TestFormat()
        {
            var format = "Note: {Author @MaxGrab} // {Timestamp: DateTimeOffset;o} ({Age: int;X} {AgeMeasure} ago)";
            var input = "Note: Yury Chetyrko // 2009-09-11T10:28:20.1230000+02:00 (2 months ago)";
            // Parse values of object properties from text
            var props = new Properties();
            Format.ParseText(format, input, props);
            var ok = props.Author == "Yury Chetyrko";
            ok = ok && props.Timestamp == new DateTimeOffset(2009, 09, 11, 10, 28, 20, 123, TimeSpan.FromHours(2));
            ok = ok && props.Age == 2;
            ok = ok && props.AgeMeasure == "months";
            if (!ok)
                throw new Exception("text parsing mismatch");
            // Modify object properties and emit text using given format
            props.Author = props.Author.ToLower();
            props.Timestamp = props.Timestamp.ToUniversalTime();
            var output = Format.EmitText(props, format);
            if (output != "Note: yury chetyrko // 2009-09-11T08:28:20.1230000+00:00 (2 months ago)")
                throw new Exception("text emit mismatch");
        }

        public struct TestClock
        {
            public int NodeNumber;
            public long Value;

            public TestClock(int nodeNumber, long value)
            {
                NodeNumber = nodeNumber;
                Value = value;
            }

            public static TestClock Parse(string str)
            {
                var result = new TestClock();
                if (!string.IsNullOrEmpty(str))
                {
                    string[] t = str.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                    if (!string.IsNullOrWhiteSpace(t[0]))
                        result.Value = long.Parse(t[0], NumberStyles.AllowHexSpecifier);
                    if (!string.IsNullOrWhiteSpace(t[1]))
                        result.NodeNumber = int.Parse(t[1], NumberStyles.AllowHexSpecifier);
                }
                return result;
            }

            public override string ToString()
            {
                return string.Format("{0:X}{1}{2:X}", Value, '@', NodeNumber);
            }
        }

        public class CustomTypeProperties
        {
            public TestClock ConfigurationId;
            public TestClock Timestamp { get; set; }
            public int ObjectCount;
        };

        public static void TestFormat2()
        {
            var format = "{ConfigurationId} / {Timestamp} / {ObjectCount}";
            var input = "12@1 / 29@0 / 16";
            // Parse values of object properties from text
            var obj = new CustomTypeProperties
            {
                ConfigurationId = new TestClock(),
                Timestamp = new TestClock(),
                ObjectCount = (int)0
            };
            Format.ParseText(format, input, obj);
            var ok = obj.ConfigurationId.Value == 0x12 && obj.ConfigurationId.NodeNumber == 1;
            ok = ok && obj.Timestamp.Value == 0x29 && obj.Timestamp.NodeNumber == 0;
            ok = ok && obj.ObjectCount == 16;
            if (!ok)
                throw new Exception("text parsing mismatch");
            // Modify object properties and emit text using given format
            var obj2 = new CustomTypeProperties
            {
                ConfigurationId = obj.ConfigurationId,
                Timestamp = new TestClock(obj.Timestamp.NodeNumber + 1, obj.Timestamp.Value + 1),
                ObjectCount = obj.ObjectCount + 1
            };
            var output = Format.EmitText(obj2, format);
            if (output != "12@1 / 2A@1 / 17")
                throw new Exception("text emit mismatch");
        }
    }
}
