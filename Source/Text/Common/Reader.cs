//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Text
{
    public delegate bool Reader<T>(out T item);

    public static class ReaderFactory
    {
        public static Reader<T> GetReader<T>(IEnumerable<T> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            return delegate(out T item)
            {
                var result = enumerator.MoveNext();
                item = result ? item = enumerator.Current : default(T);
                return result;
            };
        }

        public static Reader<char> GetReader(string text)
        {
            var i = 0;
            return delegate(out char ch)
            {
                var result = i < text.Length;
                if (result)
                {
                    ch = text[i];
                    i += 1;
                }
                else
                    ch = default(char);
                return result;
            };
        }

        public static IEnumerable<T> GetEnumerable<T>(Reader<T> read)
        {
            T t;
            while (read(out t))
                yield return t;
        }

        public static Func<T> GetSimpleGetter<T>(Reader<T> read, T end)
        {
            return delegate()
            {
                T t;
                return read(out t) ? t : end;
            };
        }

        public static Reader<TResult> Select<T, TResult>(this Reader<T> read, Func<T, TResult> selector)
        {
            return delegate(out TResult item)
            {
                T t;
                var result = read(out t);
                item = result ? selector(t) : default(TResult);
                return result;
            };
        }

        public static Reader<T> Where<T>(this Reader<T> read, Func<T, bool> predicate)
        {
            return delegate(out T item)
            {
                bool result, more;
                do
                {
                    result = read(out item);
                    more = result && !predicate(item);
                }
                while (more);
                return result;
            };
        }
    }
}
