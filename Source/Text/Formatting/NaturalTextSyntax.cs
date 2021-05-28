//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Text.Formatting
{
    // General note: comparison of terms should depend on their stemmed length

    public enum NaturalTextTokenKind { Unknown = 0, Space, Symbol, Word, Number, Cipher }

    public class NaturalLanguageSyntax : CustomSyntax<NaturalTextTokenKind>
    {
        public NaturalLanguageSyntax()
        {
            Predicates.ClearAndAdd(Space, WordOrNumberOrCipher, Symbol); // Symbol predicate is assumed to be the last in the list
        }

        public virtual bool Space(char nextChar, Slice literal, ref NaturalTextTokenKind kind)
        {
            var result = char.IsWhiteSpace(nextChar) && literal.Length == 0;
            if (result)
                kind = NaturalTextTokenKind.Space;
            return result;
        }

        public virtual bool WordOrNumberOrCipher(char nextChar, Slice literal, ref NaturalTextTokenKind kind)
        {
            var result = true;
            if (char.IsLetter(nextChar))
            {
                if (literal.Length == 0)
                    kind = NaturalTextTokenKind.Word;
                else if (kind == NaturalTextTokenKind.Number)
                    kind = NaturalTextTokenKind.Cipher;
            }
            else if (char.IsDigit(nextChar))
            {
                if (literal.Length == 0)
                    kind = NaturalTextTokenKind.Number;
                else if (kind == NaturalTextTokenKind.Word)
                    kind = NaturalTextTokenKind.Cipher;
            }
            else
                result = false;
            return result;
        }

        public virtual bool Symbol(char nextChar, Slice literal, ref NaturalTextTokenKind kind)
        {
            var result = literal.Length == 0; // Symbol predicate is assumed to be applied last
            if (result)
                kind = NaturalTextTokenKind.Symbol;
            return result;
        }
    }
}
