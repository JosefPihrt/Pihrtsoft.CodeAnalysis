// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0127InlineConstantValueTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InlineConstantValue;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_Field_Null()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const string K = null;

    void M(string s)
    {
        s = [|K|];
    }
}
", @"
class C
{
    public const string K = null;

    void M(string s)
    {
        s = null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_Field_VerbatimLiteral()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const string K = @""x"";

    void M(string s)
    {
        s = [|K|];
    }
}
", @"
class C
{
    public const string K = @""x"";

    void M(string s)
    {
        s = @""x"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_Field_VerbatimLiteral_MultipleDefinitions()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const string K = @""x"";
    public const string K2 = K;
    public const string K3 = K2;

    void M(string s)
    {
        s = [|K3|];
    }
}
", @"
class C
{
    public const string K = @""x"";
    public const string K2 = K;
    public const string K3 = K2;

    void M(string s)
    {
        s = @""x"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_Field_QualifiedWithClassName()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const string K = @""x"";

    void M(string s)
    {
        s = [|C.K|];
    }
}
", @"
class C
{
    public const string K = @""x"";

    void M(string s)
    {
        s = @""x"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_Field_QualifiedWithNamespaceName()
        {
            await VerifyRefactoringAsync(@"
namespace A.B
{
    class C
    {
        public const string K = @""x"";

        void M(string s)
        {
            s = [|A.B.C.K|];
        }
    }
}
", @"
namespace A.B
{
    class C
    {
        public const string K = @""x"";

        void M(string s)
        {
            s = @""x"";
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_BoolField()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const bool KB = true;

    void M()
    {
        bool b = [|KB|];
    }
}
", @"
class C
{
    public const bool KB = true;

    void M()
    {
        bool b = true;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_CharFieldAsync()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const char KC = '\n';

    void M()
    {
        char c = [|KC|];
    }
}
", @"
class C
{
    public const char KC = '\n';

    void M()
    {
        char c = '\n';
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_IntFieldAsync()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const int KI = int.MaxValue;

    void M()
    {
        int i = [|KI|];
    }
}
", @"
class C
{
    public const int KI = int.MaxValue;

    void M()
    {
        int i = 2147483647;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_LongFieldAsync()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const long KL = 1;

    void M()
    {
        long l = [|KL|];
    }
}
", @"
class C
{
    public const long KL = 1;

    void M()
    {
        long l = 1;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_Field_AddExpression()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public const string K = @""a"";
    public const string K2 = K + K;

    void M()
    {
        string s = [|K2|];
    }
}
", @"
class C
{
    public const string K = @""a"";
    public const string K2 = K + K;

    void M()
    {
        string s = ""aa"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task Test_MultipleDocumentsAsync()
        {
            await VerifyRefactoringAsync(@"
namespace A.B
{
    class C
    {
        public const string K = C2.K2;

        void M(string s)
        {
            s = [|K|];
        }
    }
}
", @"
namespace A.B
{
    class C
    {
        public const string K = C2.K2;

        void M(string s)
        {
            s = @""x"";
        }
    }
}
", additionalFiles: new[] { @"
namespace A.B
{
    class C2
    {
        public const string K2 = C3.K3;
    }
}
", @"
namespace A.B
{
    class C3
    {
        public const string K3 = @""x"";
    }
}
", }, equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineConstantValue)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
using System;

class C
{
    public readonly string F = null;

    void M(string s)
    {
        s = [|""x""|];
        s = [|""x"" + ""x""|];
        s = [|F|];
        s = [|string.Empty|];
        var options = [|StringSplitOptions.None|];
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
