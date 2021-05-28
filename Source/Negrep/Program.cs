//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using Nezaboodka.Nevod.Negrep.Consoles;

namespace Nezaboodka.Nevod.Negrep
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            IConsole console = new StandardConsole();
            var negrep = new Negrep(args, console, isStreamModeEnabled: true);
            return await negrep.Execute();
        }
    }
}
