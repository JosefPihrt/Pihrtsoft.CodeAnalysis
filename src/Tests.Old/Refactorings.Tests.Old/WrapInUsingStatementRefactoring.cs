// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

#pragma warning disable CS0219

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class WrapInUsingStatementRefactoring
    {
        public void SomeMethod()
        {
            Stream stream = null;

            var sm = new StreamReader(stream);

#if DEBUG
            var sr = new StreamReader(stream); //0
       /*1*/sr.Read(); //1
            sr.Read(); //2
#endif

            switch (0)
            {
                case 0:
                    var sm2 = new StreamReader(stream);
                    break;
            }

            //n

            StreamReader sm3 = null;
            StreamReader sm4 = default(StreamReader);
        }
    }
}
