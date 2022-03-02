//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class PatternSyntax : Syntax
    {
        public string Namespace { get; private set; }
        public string MasterPatternName { get; private set; }
        public bool IsSearchTarget { get; }
        public string Name { get; }
        public string FullName { get; private set; }
        public ReadOnlyCollection<FieldSyntax> Fields { get; }
        public bool IsFieldListExplicit { get; }
        public Syntax Body { get; }
        public ReadOnlyCollection<PatternSyntax> NestedPatterns { get; private set; }
        public virtual bool IsSystem => false;
        public virtual bool IsFundamental => false;

        public FieldSyntax FindFieldByName(string fieldName)
        {
            FieldSyntax result = Fields.FirstOrDefault(x => x.Name == fieldName);
            return result;
        }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                int rangeStart = TextRange.Start;
                if (Fields.Count != 0)
                {
                    int rangeEnd = Fields[0].TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.AddForElements(Fields);
                    rangeStart = Fields[Fields.Count - 1].TextRange.End;
                }
                if (Body != null)
                {
                    int rangeEnd = Body.TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.Add(Body);
                    rangeStart = Body.TextRange.End;
                }
                if (NestedPatterns.Count != 0)
                {
                    int rangeEnd = NestedPatterns[0].TextRange.Start;
                    childrenBuilder.AddInsideRange(rangeStart, rangeEnd);
                    childrenBuilder.AddForElements(NestedPatterns);
                    rangeStart = NestedPatterns[NestedPatterns.Count - 1].TextRange.End;
                }
                childrenBuilder.AddInsideRange(rangeStart, TextRange.End);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal PatternSyntax(string nameSpace, string masterPatternName, bool isSearchTarget, string name,
            IList<FieldSyntax> fields, Syntax body, IList<PatternSyntax> nestedPatterns)
        {
            Namespace = nameSpace != null ? nameSpace : string.Empty;
            MasterPatternName = masterPatternName;
            IsSearchTarget = isSearchTarget;
            Name = name;
            FullName = GetFullName(GetFullName(nameSpace, masterPatternName), name);
            if (fields == null)
                fields = EmptyFieldList();
            else
                IsFieldListExplicit = true;
            Fields = new ReadOnlyCollection<FieldSyntax>(fields);
            Body = body;
            if (nestedPatterns == null)
                nestedPatterns = Array.Empty<PatternSyntax>();
            NestedPatterns = new ReadOnlyCollection<PatternSyntax>(nestedPatterns);
        }

        internal void SetMasterPatternName(string nameSpace, string masterPatternName)
        {
            Namespace = nameSpace;
            MasterPatternName = masterPatternName;
            FullName = GetFullName(GetFullName(nameSpace, masterPatternName), Name);
            string masterPatternNameForNestedPatterns = GetFullName(masterPatternName, Name);
            foreach (PatternSyntax p in NestedPatterns)
                p.SetMasterPatternName(Namespace, masterPatternNameForNestedPatterns);
        }

        internal PatternSyntax Update(Syntax body, ReadOnlyCollection<FieldSyntax> fields,
            ReadOnlyCollection<PatternSyntax> nestedPatterns)
        {
            PatternSyntax result = this;
            if (body != Body || fields != Fields || nestedPatterns != NestedPatterns)
                result = new PatternSyntax(Namespace, MasterPatternName, IsSearchTarget, Name,
                    fields, body, nestedPatterns);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitPattern(this);
        }
    }

    internal class SystemPatternSyntax : PatternSyntax
    {
        public override bool IsSystem => true;

        internal SystemPatternSyntax(string name, Syntax body)
            : base(nameSpace: string.Empty, masterPatternName: string.Empty, isSearchTarget: false,
                name, EmptyFieldList(), body, EmptyPatternList())
        {
        }
    }

    internal class FundamentalPatternSyntax : SystemPatternSyntax
    {
        public override bool IsFundamental => true;

        internal FundamentalPatternSyntax(string name, TokenSyntax body)
            : base(name, body)
        {
        }
    }

    public partial class Syntax
    {
        public static string GetFullName(string nameSpace, string name)
        {
            string result;
            if (string.IsNullOrEmpty(nameSpace))
            {
                if (string.IsNullOrEmpty(name))
                    result = string.Empty;
                else
                    result = name;
            }
            else
            {
                if (string.IsNullOrEmpty(name))
                    result = nameSpace;
                else
                    result = nameSpace + '.' + name;
            }
            return result;
        }

        public static PatternSyntax[] EmptyPatternList()
        {
            return Array.Empty<PatternSyntax>();
        }

        public static FieldSyntax[] EmptyFieldList()
        {
            return Array.Empty<FieldSyntax>();
        }

        public static PatternSyntax Pattern(bool isSearchTarget, string name, Syntax body)
        {
            PatternSyntax result = new PatternSyntax(nameSpace: string.Empty, masterPatternName: string.Empty,
                isSearchTarget, name, EmptyFieldList(), body, EmptyPatternList());
            return result;
        }

        public static PatternSyntax Pattern(bool isSearchTarget, string name, Syntax body,
            params FieldSyntax[] fields)
        {
            PatternSyntax result = new PatternSyntax(nameSpace: string.Empty, masterPatternName: string.Empty,
                isSearchTarget, name, fields, body, EmptyPatternList());
            return result;
        }

        public static PatternSyntax Pattern(bool isSearchTarget, string name, IList<FieldSyntax> fields, Syntax body)
        {
            PatternSyntax result = new PatternSyntax(nameSpace: string.Empty, masterPatternName: string.Empty,
                isSearchTarget, name, fields, body, EmptyPatternList());
            return result;
        }

        public static PatternSyntax Pattern(string nameSpace, bool isSearchTarget, string name, Syntax body)
        {
            PatternSyntax result = new PatternSyntax(nameSpace, masterPatternName: string.Empty,
                isSearchTarget, name, EmptyFieldList(), body, EmptyPatternList());
            return result;
        }

        public static PatternSyntax Pattern(string nameSpace, bool isSearchTarget, string name,
            IList<FieldSyntax> fields, Syntax body)
        {
            PatternSyntax result = new PatternSyntax(nameSpace, masterPatternName: string.Empty,
                isSearchTarget, name, fields, body, EmptyPatternList());
            return result;
        }

        public static PatternSyntax Pattern(string nameSpace, bool isSearchTarget, string name,
            IList<FieldSyntax> fields, Syntax body, IList<PatternSyntax> nestedPatterns)
        {
            PatternSyntax result = new PatternSyntax(nameSpace, masterPatternName: string.Empty,
                isSearchTarget, name, fields, body, nestedPatterns);
            foreach (PatternSyntax p in result.NestedPatterns)
                p.SetMasterPatternName(nameSpace, name);
            return result;
        }
    }
}
