// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1246UseElementAccessTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, OptimizeLinqMethodCallCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseElementAccess;

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        [InlineData("((List<object>)x).[|First()|]", "((List<object>)x)[0]")]
        [InlineData("((IList<object>)x).[|First()|]", "((IList<object>)x)[0]")]
        [InlineData("((IReadOnlyList<object>)x).[|First()|]", "((IReadOnlyList<object>)x)[0]")]
        [InlineData("((Collection<object>)x).[|First()|]", "((Collection<object>)x)[0]")]
        [InlineData("((ImmutableArray<object>)x).[|First()|]", "((ImmutableArray<object>)x)[0]")]
        [InlineData("((object[])x).[|First()|]", "((object[])x)[0]")]
        [InlineData("((string)x).[|First()|]", "((string)x)[0]")]
        public async Task Test_UseElementAccessInsteadOfFirst(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

class C
{
    void M()
    {
        object x = null;
        var y = [||];
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        [InlineData("((List<object>)x).[|ElementAt(1)|]", "((List<object>)x)[1]")]
        [InlineData("((IList<object>)x).[|ElementAt(1)|]", "((IList<object>)x)[1]")]
        [InlineData("((IReadOnlyList<object>)x).[|ElementAt(1)|]", "((IReadOnlyList<object>)x)[1]")]
        [InlineData("((Collection<object>)x).[|ElementAt(1)|]", "((Collection<object>)x)[1]")]
        [InlineData("((ImmutableArray<object>)x).[|ElementAt(1)|]", "((ImmutableArray<object>)x)[1]")]
        [InlineData("((object[])x).[|ElementAt(1)|]", "((object[])x)[1]")]
        [InlineData("((string)x).[|ElementAt(1)|]", "((string)x)[1]")]
        public async Task Test_UseElementAccessInsteadOfElementAt(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

class C
{
    void M()
    {
        object x = null;
        var y = [||];
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task Test_UseElementAccessOnInvocationExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        object x = null;

        x = ((List<object>)x).ToList().[|First()|];
        x = ((object[])x).ToArray().[|First()|];
        x = ((ImmutableArray<object>)x).ToImmutableArray().[|First()|];
        x = ((string)x).ToString().[|First()|];

        x = ((List<object>)x).ToList().[|ElementAt(1)|];
        x = ((object[])x).ToArray().[|ElementAt(1)|];
        x = ((ImmutableArray<object>)x).ToImmutableArray().[|ElementAt(1)|];
        x = ((string)x).ToString().[|ElementAt(1)|];
    }
}
",
@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        object x = null;

        x = ((List<object>)x).ToList()[0];
        x = ((object[])x).ToArray()[0];
        x = ((ImmutableArray<object>)x).ToImmutableArray()[0];
        x = ((string)x).ToString()[0];

        x = ((List<object>)x).ToList()[1];
        x = ((object[])x).ToArray()[1];
        x = ((ImmutableArray<object>)x).ToImmutableArray()[1];
        x = ((string)x).ToString()[1];
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task TestNoDiagnostic_UseElementAccessInsteadOfElementAt()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

class C
{
    void M()
    {
        object x = null;

        x = ((ICollection<object>)x).ElementAt(1);
        x = ((IReadOnlyCollection<object>)x).ElementAt(1);
        x = ((IEnumerable<object>)x).ElementAt(1);

        x = ((Dictionary<object, object>)x).ElementAt(1);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task TestNoDiagnostic_UseElementAccessInsteadOfFirst()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

class C
{
    void M()
    {
        object x = null;

        x = ((ICollection<object>)x).First();
        x = ((IReadOnlyCollection<object>)x).First();
        x = ((IEnumerable<object>)x).First();

        x = ((Dictionary<object, object>)x).First();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task TestNoDiagnostic_UseElementAccessOnInvocation()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        object x = null;

        x = ((List<object>)x).ToList().First();
        x = ((object[])x).ToArray().First();
        x = ((ImmutableArray<object>)x).ToImmutableArray().First();
        x = ((string)x).ToString().First();

        x = ((List<object>)x).ToList().ElementAt(1);
        x = ((object[])x).ToArray().ElementAt(1);
        x = ((ImmutableArray<object>)x).ToImmutableArray().ElementAt(1);
        x = ((string)x).ToString().ElementAt(1);
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.DoNotUseElementAccessWhenExpressionIsInvocation));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task TestNoDiagnostic_UseElementAccessOnElementAccess()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    List<object> this[int index] => null;

    void M()
    {
        C x = default;

        var first = x[0].First();
        var second = x[0].ElementAt(1);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task TestNoDiagnostic_UseElementAccess_ExpressionStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        object x = null;

        ((List<object>)x).First();
        ((List<object>)x).ElementAt(1);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseElementAccess)]
        public async Task TestNoDiagnostic_UseElementAccessInsteadOfElementAt_InfiniteRecursion()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class C : IReadOnlyList<int>
{
    public int this[int index] => this.ElementAt(index);

    public int Count => throw new NotImplementedException();

    public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
");
        }
    }
}
