//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    public interface ITextSource : IEnumerable<Token>
    {
        string GetText(TextLocation start, TextLocation end);
    }
}
