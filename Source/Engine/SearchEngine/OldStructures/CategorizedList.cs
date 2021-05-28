//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public class CategorizedList<TValue> where TValue : class
    {
        public const int DefaultCategoryElementsShrinkThreshold = 4;
        public const int DefaultCategoryElementsShrinkReallocationThreshold = 100;
        public const int DefaultActualCategoriesShrinkThreshold = 4;
        public const int DefaultActualCategoriesShrinkReallocationThreshold = 100;

        public CategoryList[] ElementsPerCategory;
        public int CategoryListCapacity;
        public int[] ActualCategories;
        public int ActualCategoryCount;
        public int ActualCategoryCountSnapshot;
        public int RemovedActualCategoryCount;

        public int CategoryElementsShrinkThreshold;
        public int CategoryElementsShrinkReallocationThreshold;
        public int ActualCategoriesShrinkThreshold;
        public int ActualCategoriesShrinkReallocationThreshold;

        public class CategoryList
        {
            public TValue[] Elements;
            public int Count;
            public int CountSnapshot;
            public int RemovedCount;
            public int ActualCategoriesIndex;
            public int ShrinkThreshold;
            public int ShrinkReallocationThreshold;

            public CategoryList(int capacity, int actualCategoriesIndex, int shrinkThreshold,
                int shrinkReallocationThreshold)
            {
                Elements = new TValue[capacity];
                ActualCategoriesIndex = actualCategoriesIndex;
                ShrinkThreshold = shrinkThreshold;
                ShrinkReallocationThreshold = shrinkReallocationThreshold;
            }

            public bool TryAdd(TValue element)
            {
                if (Count < Elements.Length)
                {
                    Elements[Count] = element;
                    Count++;
                    return true;
                }
                return false;
            }

            public void MarkRemoved(int elementIndex)
            {
                Elements[elementIndex] = null;
                RemovedCount++;
            }

            public void Shrink()
            {
                if (RemovedCount == Count)
                    Count = 0;
                else if (RemovedCount >= ShrinkThreshold)
                {
                    if (RemovedCount < ShrinkReallocationThreshold)
                        RemoveNullElements();
                    else
                        ReallocateWithoutNullElements();
                }
            }

            private void RemoveNullElements()
            {
                int i = 0;
                while (i < Count && Elements[i] != null)
                    i++;
                int j = i;
                while (i < Count && j < Count)
                {
                    while (j < Count && Elements[j] == null)
                        j++;
                    while (j < Count && Elements[j] != null)
                    {
                        Elements[i] = Elements[j];
                        i++;
                        j++;
                    }
                }
                Count = i;
                RemovedCount = 0;
            }

            private void ReallocateWithoutNullElements()
            {
                var elementsCopy = new TValue[Elements.Length];
                int newCount = 0;
                for (int i = 0; i < Count; i++)
                {
                    TValue element = Elements[i];
                    if (element != null)
                    {
                        elementsCopy[newCount] = element;
                        newCount++;
                    }
                }
                Elements = elementsCopy;
                Count = newCount;
                RemovedCount = 0;
            }
        }

        public CategorizedList(int categoryCount, int categoryListCapacity)
        {
            ElementsPerCategory = new CategoryList[categoryCount];
            CategoryListCapacity = categoryListCapacity;
            ActualCategories = new int[categoryCount * 2];
            CategoryElementsShrinkThreshold = DefaultCategoryElementsShrinkThreshold;
            CategoryElementsShrinkReallocationThreshold = DefaultCategoryElementsShrinkReallocationThreshold;
            ActualCategoriesShrinkThreshold = DefaultActualCategoriesShrinkThreshold;
            ActualCategoriesShrinkReallocationThreshold = DefaultActualCategoriesShrinkReallocationThreshold;
        }

        public bool TryAdd(int category, TValue value)
        {
            bool result;
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList == null)
            {
                result = ActualCategoryCount < ActualCategories.Length;
                if (result)
                {
                    categoryList = new CategoryList(CategoryListCapacity, ActualCategoryCount,
                        CategoryElementsShrinkThreshold, CategoryElementsShrinkReallocationThreshold);
                    ElementsPerCategory[category] = categoryList;
                    ActualCategories[ActualCategoryCount] = category;
                    ActualCategoryCount++;
                    result = categoryList.TryAdd(value);
                }
            }
            else
            {
                if (categoryList.Count == 0 && categoryList.ActualCategoriesIndex < 0)
                {
                    result = ActualCategoryCount < ActualCategories.Length;
                    if (result)
                    {
                        categoryList.ActualCategoriesIndex = ActualCategoryCount;
                        ActualCategories[ActualCategoryCount] = category;
                        ActualCategoryCount++;
                        result = categoryList.TryAdd(value);
                    }
                }
                else
                    result = categoryList.TryAdd(value);
            }
            return result;
        }

        public void Remove(int category, int valueIndex)
        {
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList != null)
            {
                if (valueIndex < categoryList.Count)
                {
                    categoryList.Elements[valueIndex] = null;
                    categoryList.RemovedCount++;
                }
            }
        }

        public void RemoveCategory(int category)
        {
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList != null && categoryList.Count > 0)
            {
                categoryList.Count = 0;
                int actualCategoriesIndex = categoryList.ActualCategoriesIndex;
                if (actualCategoriesIndex >= 0)
                {
                    ActualCategories[actualCategoriesIndex] = -1;
                    categoryList.ActualCategoriesIndex = -1;
                    RemovedActualCategoryCount++;
                }
            }
        }

        public void MakeSnapshot()
        {
            ActualCategoryCountSnapshot = ActualCategoryCount;
            for (int i = 0, n = ActualCategoryCountSnapshot; i < n; i++)
            {
                int category = ActualCategories[i];
                if (category >= 0)
                {
                    var categoryList = ElementsPerCategory[category];
                    if (categoryList != null)
                        categoryList.CountSnapshot = categoryList.Count;
                }
            }
        }

        public void Clear()
        {
            ElementsPerCategory = new CategoryList[ElementsPerCategory.Length];
            ActualCategoryCount = 0;
            RemovedActualCategoryCount = 0;
            ActualCategoryCountSnapshot = 0;
        }

        public void Shrink()
        {
            for (int i = 0, n = ActualCategoryCount; i < n; i++)
            {
                int category = ActualCategories[i];
                if (category >= 0)
                    ShrinkCategory(category);
            }
            if (RemovedActualCategoryCount >= ActualCategoriesShrinkThreshold)
            {
                if (RemovedActualCategoryCount < ActualCategoriesShrinkReallocationThreshold)
                    RemoveNegativeCategories();
                else
                    ReallocateWithoutNegativeCategories();
            }
        }

        public void ShrinkCategory(int category)
        {
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList != null)
            {
                categoryList.Shrink();
                if (categoryList.Count == 0)
                {
                    ActualCategories[categoryList.ActualCategoriesIndex] = -1;
                    categoryList.ActualCategoriesIndex = -1;
                    RemovedActualCategoryCount++;
                }
            }
        }

        public void ForEach(Action<int, int, TValue> action)
        {
            MakeSnapshot();
            for (int i = 0, n = ActualCategoryCountSnapshot; i < n; i++)
            {
                int actualCategory = ActualCategories[i];
                if (actualCategory >= 0)
                    ForEachInCategory(ActualCategories[i], action);
            }
        }

        public void ForEachInCategory(int category, Action<int, int, TValue> action)
        {
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList != null)
            {
                TValue[] elements = categoryList.Elements;
                for (int i = 0, n = categoryList.CountSnapshot; i < n; i++)
                {
                    TValue element = elements[i];
                    if (element != null)
                        action(category, i, element);
                }
            }
        }

        public void Filter(Func<int, int, TValue, bool> predicate)
        {
            MakeSnapshot();
            for (int i = 0, n = ActualCategoryCountSnapshot; i < n; i++)
            {
                int actualCategory = ActualCategories[i];
                if (actualCategory >= 0)
                    FilterInCategory(ActualCategories[i], predicate);
            }
        }

        public void FilterInCategory(int category, Func<int, int, TValue, bool> predicate)
        {
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList != null)
            {
                TValue[] elements = categoryList.Elements;
                for (int i = 0, n = categoryList.CountSnapshot; i < n; i++)
                {
                    TValue element = elements[i];
                    if (element != null)
                    {
                        if (!predicate(category, i, element))
                            Remove(category, i);
                    }
                }
            }
        }

        public TValue FirstOrNull()
        {
            TValue result = null;
            if (ActualCategoryCount > 0)
            {
                int category = -1;
                for (int i = 0, n = ActualCategoryCount; i < n; i++)
                {
                    category = ActualCategories[i];
                    if (category >= 0)
                    {
                        result = FirstOrNullInCategory(category);
                        if (result != null)
                            break;
                    }
                }
            }
            return result;
        }

        public TValue FirstOrNullInCategory(int category)
        {
            TValue result = null;
            CategoryList categoryList = ElementsPerCategory[category];
            if (categoryList != null)
            {
                TValue[] elements = categoryList.Elements;
                if (categoryList.Count > 0)
                {
                    for (int i = 0, n = categoryList.Count; i < n; i++)
                    {
                        result = elements[i];
                        if (result != null)
                            break;
                    }
                }
            }
            return result;
        }

        private void RemoveNegativeCategories()
        {
            int i = 0;
            int n = ActualCategoryCount;
            while (i < n && ActualCategories[i] >= 0)
                i++;
            int j = i;
            while (i < n && j < n)
            {
                while (j < n && ActualCategories[i] < 0)
                    j++;
                while (j < n && ActualCategories[i] >= 0)
                {
                    int category = ActualCategories[j];
                    ActualCategories[i] = category;
                    ElementsPerCategory[category].ActualCategoriesIndex = i;
                    i++;
                    j++;
                }
            }
            ActualCategoryCount = i;
            RemovedActualCategoryCount = 0;
        }

        private void ReallocateWithoutNegativeCategories()
        {
            var actualCategoriesCopy = new int[ActualCategories.Length];
            int newActualCategoryCount = 0;
            for (int i = 0, n = ActualCategoryCount; i < n; i++)
            {
                int category = ActualCategories[i];
                if (category >= 0)
                {
                    CategoryList categoryList = ElementsPerCategory[category];
                    if (categoryList.Count > 0)
                    {
                        categoryList.ActualCategoriesIndex = newActualCategoryCount;
                        actualCategoriesCopy[newActualCategoryCount] = category;
                        newActualCategoryCount++;
                    }
                }
            }
            ActualCategories = actualCategoriesCopy;
            ActualCategoryCount = newActualCategoryCount;
            RemovedActualCategoryCount = 0;
        }
    }
}
