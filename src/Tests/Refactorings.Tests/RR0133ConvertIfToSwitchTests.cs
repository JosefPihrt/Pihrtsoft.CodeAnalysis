// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0133ConvertIfToSwitchTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertIfToSwitch;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertIfToSwitch)]
        public async Task Test_Constant()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        int x = 0;

        [||]if (x == 1)
        {
            return 1;
        }
        else if (x == 2 || (x == 3) || x == 4)
        {
            return 234;
        }
        else if ((x == 5))
        {
            M();
        }

        return 0;
    }
}
", @"
class C
{
    int M()
    {
        int x = 0;

        switch (x)
        {
            case 1:
                {
                    return 1;
                }

            case 2:
            case 3:
            case 4:
                {
                    return 234;
                }

            case 5:
                {
                    M();
                    break;
                }
        }

        return 0;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertIfToSwitch)]
        public async Task Test_Pattern()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        object x = null;

        [||]if (x is string s)
        {
            return 1;
        }
        else if (x is int i)
        {
            return 2;
        }
        else if (x is null)
        {
        }

        return 0;
    }
}
", @"
class C
{
    int M()
    {
        object x = null;

        switch (x)
        {
            case string s:
                {
                    return 1;
                }

            case int i:
                {
                    return 2;
                }

            case null:
                {
                    break;
                }
        }

        return 0;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertIfToSwitch)]
        public async Task Test_ConstantAndPattern()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        string s = null;

        [||]if (s == """")
        {
            return 1;
        }
        else if (s == ""a"" || s == ""b"")
        {
            return 2;
        }
        else if (s is object o)
        {
            return 3;
        }
        else if (s == null)
        {
            M();
        }

        return 0;
    }
}
", @"
class C
{
    int M()
    {
        string s = null;

        switch (s)
        {
            case """":
                {
                    return 1;
                }

            case ""a"":
            case ""b"":
                {
                    return 2;
                }

            case object o:
                {
                    return 3;
                }

            case null:
                {
                    M();
                    break;
                }
        }

        return 0;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertIfToSwitch)]
        public async Task TestNoRefactoring_ExpressionsAreNotEqual()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    int M()
    {
        int x = 0;
        int y = 0;

        [||]if (x == 1)
        {
            return 1;
        }
        else if (y == 2)
        {
            return 2;
        }

        return 0;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
