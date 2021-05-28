//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public struct ImmutableArray<T>
    {
        private T[] fItems;

        public ImmutableArray(ImmutableArray<T> array)
        {
            fItems = array.fItems;
        }

        public ImmutableArray(T item)
        {
            fItems = new T[] { item };
        }

        public ImmutableArray(ImmutableArray<T> array, T item)
        {
            if (array.fItems != null)
            {
                fItems = array.fItems;
                int existingLength = fItems.Length;
                Array.Resize(ref fItems, existingLength + 1);
                fItems[existingLength] = item;
            }
            else
                fItems = new T[] { item };
        }

        public ImmutableArray(ImmutableArray<T> first, ImmutableArray<T> second)
        {
            fItems = first.fItems;
            if (fItems != null)
            {
                if (second.fItems != null)
                {
                    int thisItemCount = fItems.Length;
                    int sourceItemCount = second.fItems.Length;
                    Array.Resize(ref fItems, thisItemCount + sourceItemCount);
                    Array.Copy(second.fItems, 0, fItems, thisItemCount, sourceItemCount);
                }
            }
            else
                fItems = second.fItems;
        }

        public T this[int index]
        {
            get { return fItems[index]; }
        }

        public int Count 
        { 
            get { return fItems.Length; } 
        }

        public bool IsNullOrEmpty()
        {
            return fItems == null || fItems.Length == 0;
        }
    }
}
