// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS1624BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceTypeTests : AbstractCSharpCompilerDiagnosticFixVerifier<MethodDeclarationOrLocalFunctionStatementCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceType;

        public override CSharpTestOptions Options
        {
            get { return base.Options.AddAllowedCompilerDiagnosticId(CompilerDiagnosticIdentifiers.CannotImplicitlyConvertType); }
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceType)]
        public async Task Test_Method_String()
        {
            await VerifyFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        yield return default(string);
        yield return DateTime.Now;
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    IEnumerable<string> M()
    {
        yield return default(string);
        yield return DateTime.Now;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "string"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceType)]
        public async Task Test_LocalFunction_String()
        {
            await VerifyFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        void LF()
        {
            yield return default(string);
            yield return DateTime.Now;
        }
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        IEnumerable<string> LF()
        {
            yield return default(string);
            yield return DateTime.Now;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "string"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceType)]
        public async Task Test_Method_DateTime()
        {
            await VerifyFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        yield return default(string);
        yield return DateTime.Now;
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    IEnumerable<DateTime> M()
    {
        yield return default(string);
        yield return DateTime.Now;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "DateTime"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceType)]
        public async Task Test_LocalFunction_DateTime()
        {
            await VerifyFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        void LF()
        {
            yield return default(string);
            yield return DateTime.Now;
        }
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        IEnumerable<DateTime> LF()
        {
            yield return default(string);
            yield return DateTime.Now;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "DateTime"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.BodyCannotBeIteratorBlockBecauseTypeIsNotIteratorInterfaceType)]
        public async Task TestNoFix()
        {
            await VerifyNoFixAsync(@"
class C
{
    void M()
    {
        yield return ;

        void LF()
        {
            yield return ;
        }
    }

    void M()
    {
        yield break;

        void LF()
        {
            yield break;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
