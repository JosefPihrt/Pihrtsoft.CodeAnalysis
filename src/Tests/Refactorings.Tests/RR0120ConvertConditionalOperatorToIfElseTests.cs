// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0120ConvertConditionalOperatorToIfElseTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertConditionalOperatorToIfElse;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, string x, string y)
    {
        string s = [||](f) ? x : y;
    }
}
", @"
class C
{
    void M(bool f, string x, string y)
    {
        string s;
        if (f)
        {
            s = x;
        }
        else
        {
            s = y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_LocalDeclaration_Multiline()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, string x, string y)
    {
        string s = [||](f)
            ? x
            : y;
    }
}
", @"
class C
{
    void M(bool f, string x, string y)
    {
        string s;
        if (f)
        {
            s = x;
        }
        else
        {
            s = y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_LocalDeclaration_Recursive()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = [||](f) ? ((f2) ? y : z) : x;
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s;
        if (f)
        {
            if (f2)
            {
                s = y;
            }
            else
            {
                s = z;
            }
        }
        else
        {
            s = x;
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_LocalDeclaration_Recursive2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = [||](f) ? x : ((f2) ? y : z);
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s;
        if (f)
        {
            s = x;
        }
        else if (f2)
        {
            s = y;
        }
        else
        {
            s = z;
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_SimpleAssignment()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, string x, string y)
    {
        string s = null;
        s = [||](f) ? x : y;
    }
}
", @"
class C
{
    void M(bool f, string x, string y)
    {
        string s = null;
        if (f)
        {
            s = x;
        }
        else
        {
            s = y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_SimpleAssignment_Multiline()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, string x, string y)
    {
        string s = null;
        s = [||](f)
            ? x
            : y;
    }
}
", @"
class C
{
    void M(bool f, string x, string y)
    {
        string s = null;
        if (f)
        {
            s = x;
        }
        else
        {
            s = y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_SimpleAssignment_Recursive()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        s = [||](f) ? ((f2) ? y : z) : x;
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        if (f)
        {
            if (f2)
            {
                s = y;
            }
            else
            {
                s = z;
            }
        }
        else
        {
            s = x;
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_SimpleAssignment_Recursive2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        s = [||](f) ? x : ((f2) ? y : z);
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        if (f)
        {
            s = x;
        }
        else if (f2)
        {
            s = y;
        }
        else
        {
            s = z;
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_ReturnStatement()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string M(bool f, string x, string y)
    {
        return [||](f) ? x : y;
    }
}
", @"
class C
{
    string M(bool f, string x, string y)
    {
        if (f)
        {
            return x;
        }
        else
        {
            return y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_ReturnStatement_Multiline()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string M(bool f, string x, string y)
    {
        return [||](f)
            ? x
            : y;
    }
}
", @"
class C
{
    string M(bool f, string x, string y)
    {
        if (f)
        {
            return x;
        }
        else
        {
            return y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_ReturnStatement_Recursive()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        return [||](f) ? ""a"" : ((f2) ? (f3) ? ""c"" : ""d"" : ""b"");
    }
}
", @"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            return ""a"";
        }
        else if (f2)
        {
            if (f3)
            {
                return ""c"";
            }
            else
            {
                return ""d"";
            }
        }
        else
        {
            return ""b"";
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_ReturnStatement_Recursive2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        return [||](f) ? ""a"" : ((f2) ? ""b"" : (f3) ? ""c"" : ""d"");
    }
}
", @"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            return ""a"";
        }
        else if (f2)
        {
            return ""b"";
        }
        else if (f3)
        {
            return ""c"";
        }
        else
        {
            return ""d"";
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_YieldReturnStatement()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, string x, string y)
    {
        yield return [||](f) ? x : y;
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, string x, string y)
    {
        if (f)
        {
            yield return x;
        }
        else
        {
            yield return y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_YieldReturnStatement_Multiline()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, string x, string y)
    {
        yield return [||](f)
            ? x
            : y;
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, string x, string y)
    {
        if (f)
        {
            yield return x;
        }
        else
        {
            yield return y;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_YieldReturnStatement_Recursive()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        yield return [||](f) ? ""a"" : ((f2) ? (f3) ? ""c"" : ""d"" : ""b"");
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            yield return ""a"";
        }
        else if (f2)
        {
            if (f3)
            {
                yield return ""c"";
            }
            else
            {
                yield return ""d"";
            }
        }
        else
        {
            yield return ""b"";
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse)]
        public async Task Test_YieldReturnStatement_Recursive2()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        yield return [||](f) ? ""a"" : ((f2) ? ""b"" : (f3) ? ""c"" : ""d"");
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            yield return ""a"";
        }
        else if (f2)
        {
            yield return ""b"";
        }
        else if (f3)
        {
            yield return ""c"";
        }
        else
        {
            yield return ""d"";
        }
    }
}
", equivalenceKey: ConditionalExpressionRefactoring.ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey);
        }
    }
}
