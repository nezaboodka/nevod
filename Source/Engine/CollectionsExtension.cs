//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
#if !NETCOREAPP2_0
    public static class StackExtension
    {
        public static bool TryPop<T>(this Stack<T> stack, out T value)
        {
            bool result;
            if (stack.Count != 0)
            {
                value = stack.Pop();
                result = true;
            }
            else
            {
                value = default;
                result = false;
            }
            return result;
        }

        public static bool TryPeek<T>(this Stack<T> stack, out T value)
        {
            bool result;
            if (stack.Count != 0)
            {
                value = stack.Peek();
                result = true;
            }
            else
            {
                value = default;
                result = false;
            }
            return result;
        }
    }
#endif
}
