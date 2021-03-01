// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1077OptimizeLinqMethodCallTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.OptimizeLinqMethodCall;

        protected override DiagnosticAnalyzer Analyzer { get; } = new InvocationExpressionAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new OptimizeLinqMethodCallCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallFirstAfterKeys_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|First(_ => true).Key|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Keys.First(_ => true);
    }
}
");
        }
        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallFirstAfterValues_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|First(_ => true).Value|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Values.First(_ => true);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallLastAfterValues_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|Last(_ => true).Value|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Values.Last(_ => true);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallSingleAfterValues_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|Single(_ => true).Value|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Values.Single(_ => true);
    }
}
");
        }
      
        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallSingleOrDefaultAfterValues_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|SingleOrDefault(_ => true).Value|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Values.SingleOrDefault(_ => true);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallElementAtAfterValues_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|ElementAt(1).Value|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Values.ElementAt(1);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallElementAtOrDefaultAfterValues_Dictionary()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.[|ElementAtOrDefault(1).Value|];
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Dictionary<int, string>();

        var x = items.Values.ElementAtOrDefault(1);
    }
}
");
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("Where(_ => true).Any()", "Any(_ => true)")]
        [InlineData("Where(_ => true).Count()", "Count(_ => true)")]
        [InlineData("Where(_ => true).First()", "First(_ => true)")]
        [InlineData("Where(_ => true).FirstOrDefault()", "FirstOrDefault(_ => true)")]
        [InlineData("Where(_ => true).Last()", "Last(_ => true)")]
        [InlineData("Where(_ => true).LastOrDefault()", "LastOrDefault(_ => true)")]
        [InlineData("Where(_ => true).LongCount()", "LongCount(_ => true)")]
        [InlineData("Where(_ => true).Single()", "Single(_ => true)")]
        public async Task Test_Where(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>();

        var x = items.[||];
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_Where_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        var x = items
            .[|Where(_ => true)
            .Any()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        var x = items
            .Any(_ => true);
    }
}
");
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("Where(_ => true).Any()", "Any(_ => true)")]
        [InlineData("Where(_ => true).Count()", "Count(_ => true)")]
        [InlineData("Where(_ => true).First()", "First(_ => true)")]
        [InlineData("Where(_ => true).FirstOrDefault()", "FirstOrDefault(_ => true)")]
        [InlineData("Where(_ => true).Last()", "Last(_ => true)")]
        [InlineData("Where(_ => true).LastOrDefault()", "LastOrDefault(_ => true)")]
        [InlineData("Where(_ => true).LongCount()", "LongCount(_ => true)")]
        [InlineData("Where(_ => true).Single()", "Single(_ => true)")]
        public async Task Test_Where_ImmutableArray(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        ImmutableArray<string> items = ImmutableArray.Create<string>();

        var x = items.[||];
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_WhereAndCount()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M(IEnumerable<C> items, int count)
    {
        if (items.[|Where(_ => true).Count()|] != count) { }
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M(IEnumerable<C> items, int count)
    {
        if (items.Count(_ => true) != count) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_SelectAndMin()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        int min = Enumerable.Empty<int>().[|Select(f => f).Min()|];
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        int min = Enumerable.Empty<int>().Min(f => f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_SelectAndMax()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        int max = Enumerable.Empty<int>().[|Select(f => f).Max()|];
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        int max = Enumerable.Empty<int>().Max(f => f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_SelectAndMin_ImmutableArray()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        int min = ImmutableArray.Create<int>().[|Select(f => f).Min()|];
    }
}
", @"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        int min = ImmutableArray.Create<int>().Min(f => f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_SelectAndMax_ImmutableArray()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        int max = ImmutableArray.Create<int>().[|Select(f => f).Max()|];
    }
}
", @"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        int max = ImmutableArray.Create<int>().Max(f => f);
    }
}
");
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("Where(f => f is object).Cast<object>()", "OfType<object>()")]
        [InlineData("Where((f) => f is object).Cast<object>()", "OfType<object>()")]
        [InlineData(@"Where(f =>
        {
            return f is object;
        }).Cast<object>()", "OfType<object>()")]
        public async Task Test_CallOfTypeInsteadOfWhereAndCast(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        IEnumerable<object> q = items.[||];
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData(@"Where(f => f.StartsWith(""a"")).Any(f => f.StartsWith(""b""))", @"Any(f => f.StartsWith(""a"") && f.StartsWith(""b""))")]
        [InlineData(@"Where((f) => f.StartsWith(""a"")).Any(f => f.StartsWith(""b""))", @"Any((f) => f.StartsWith(""a"") && f.StartsWith(""b""))")]
        public async Task Test_CombineWhereAndAny(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.[||]) { }
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData(@"Where(f => f.StartsWith(""a"")).Any(f => f.StartsWith(""b""))", @"Any(f => f.StartsWith(""a"") && f.StartsWith(""b""))")]
        [InlineData(@"Where((f) => f.StartsWith(""a"")).Any(f => f.StartsWith(""b""))", @"Any((f) => f.StartsWith(""a"") && f.StartsWith(""b""))")]
        public async Task Test_CombineWhereAndAny_ImmutableArray(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        ImmutableArray<string> items = ImmutableArray<string>.Empty;

        if (items.[||]) { }
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("items.FirstOrDefault(_ => true) != null", "items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) == null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) is null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault() != null", "items.Any()")]
        [InlineData("items.FirstOrDefault() == null", "!items.Any()")]
        [InlineData("items.FirstOrDefault() is null", "!items.Any()")]
        public async Task Test_FirstOrDefault_IEnumerableOfReferenceType(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>();

        if ([||]) { }
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("items.FirstOrDefault(_ => true) != null", "items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) == null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) is null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault() != null", "items.Any()")]
        [InlineData("items.FirstOrDefault() == null", "!items.Any()")]
        [InlineData("items.FirstOrDefault() is null", "!items.Any()")]
        public async Task Test_FirstOrDefault_IEnumerableOfNullableType(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<int?>();

        if ([||]) { }
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("items.FirstOrDefault(_ => true) != null", "items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) == null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) is null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault() != null", "items.Any()")]
        [InlineData("items.FirstOrDefault() == null", "!items.Any()")]
        [InlineData("items.FirstOrDefault() is null", "!items.Any()")]
        public async Task Test_FirstOrDefault_ImmutableArrayOfReferenceType(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        ImmutableArray<string> items = ImmutableArray<string>.Empty;

        if ([||]) { }
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("items.FirstOrDefault(_ => true) != null", "items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) == null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault(_ => true) is null", "!items.Any(_ => true)")]
        [InlineData("items.FirstOrDefault() != null", "items.Any()")]
        [InlineData("items.FirstOrDefault() == null", "!items.Any()")]
        [InlineData("items.FirstOrDefault() is null", "!items.Any()")]
        public async Task Test_FirstOrDefault_ImmutableArrayOfNullableType(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Immutable;
using System.Linq;

class C
{
    void M()
    {
        ImmutableArray<int?> items = ImmutableArray<int?>.Empty;

        if ([||]) { }
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_OptimizeOfType_ReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<C>();

        var q = items.[|OfType<C>()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<C>();

        var q = items.Where(f => f != null);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_OptimizeOfType_ValueType()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

struct C
{
    void M()
    {
        var items = new List<C>();

        var q = items.[|OfType<C>()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

struct C
{
    void M()
    {
        var items = new List<C>();

        var q = items;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_OptimizeOfType_TypeParameterWithStructConstraint()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M<T>() where T : struct
    {
        var items = new List<T>();

        var q = items.[|OfType<T>()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M<T>() where T : struct
    {
        var items = new List<T>();

        var q = items;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallCastInsteadOfSelect_ExtensionMethodCall()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>().[|Select(f => (object)f)|];
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>().Cast<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallCastInsteadOfSelect_StaticMethodCall()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.[|Select(Enumerable.Empty<string>(), f => (object)f)|];
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Cast<object>(Enumerable.Empty<string>());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallCastInsteadOfSelect_ParenthesizedLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>().[|Select((f) => (object)f)|];
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>().Cast<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallCastInsteadOfSelect_LambdaWithBlock()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>().[|Select(f =>
        {
            return (object)f;
        })|];
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        var items = Enumerable.Empty<string>().Cast<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallPeekInsteadOfFirst_Queue()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Queue<object>();

        var x = items.[|First|]();
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Queue<object>();

        var x = items.Peek();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallPeekInsteadOfFirst_Stack()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Stack<object>();

        var x = items.[|First|]();
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new Stack<object>();

        var x = items.Peek();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallFindInsteadOfFirstOrDefault_List()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<object>();

        var x = items.[|FirstOrDefault|](_ => true);
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<object>();

        var x = items.Find(_ => true);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallFindInsteadOfFirstOrDefault_Array()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Linq;

class C
{
    void M()
    {
        var items = new object[0];

        var x = items.[|FirstOrDefault|](_ => true);
    }
}
", @"
using System;
using System.Linq;

class C
{
    void M()
    {
        var items = new object[0];

        var x = Array.Find(items, _ => true);
    }
}
");
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("((List<object>)x).[|Count()|]", "((List<object>)x).Count")]
        [InlineData("((IList<object>)x).[|Count()|]", "((IList<object>)x).Count")]
        [InlineData("((IReadOnlyList<object>)x).[|Count()|]", "((IReadOnlyList<object>)x).Count")]
        [InlineData("((Collection<object>)x).[|Count()|]", "((Collection<object>)x).Count")]
        [InlineData("((ICollection<object>)x).[|Count()|]", "((ICollection<object>)x).Count")]
        [InlineData("((IReadOnlyCollection<object>)x).[|Count()|]", "((IReadOnlyCollection<object>)x).Count")]
        [InlineData("((ImmutableArray<object>)x).[|Count()|]", "((ImmutableArray<object>)x).Length")]
        [InlineData("((object[])x).[|Count()|]", "((object[])x).Length")]
        [InlineData("((string)x).[|Count()|]", "((string)x).Length")]
        public async Task Test_OptimizeCountCall_Array(string source, string expected)
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
        var count = [||];
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        [InlineData("items.[|Count()|] != 0", "items.Any()")]
        [InlineData("items.[|Count()|] > 0", "items.Any()")]
        [InlineData("items.[|Count()|] >= 1", "items.Any()")]
        [InlineData("0 != items.[|Count()|]", "items.Any()")]
        [InlineData("0 < items.[|Count()|]", "items.Any()")]
        [InlineData("1 <= items.[|Count()|]", "items.Any()")]
        [InlineData("items.[|Count()|] == 0", "!items.Any()")]
        [InlineData("items.[|Count()|] < 1", "!items.Any()")]
        [InlineData("items.[|Count()|] <= 0", "!items.Any()")]
        [InlineData("0 == items.[|Count()|]", "!items.Any()")]
        [InlineData("1 > items.[|Count()|]", "!items.Any()")]
        [InlineData("0 >= items.[|Count()|]", "!items.Any()")]
        public async Task Test_CallAnyInsteadOfCount(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        int i = 0;
        IEnumerable<object> items = null;

        if ([||])
        {
        }
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallFirstOrDefaultInsteadOfConditionalExpression_ReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        string s = null;

        IEnumerable<string> x = null;

        s = [|x.Any() ? x.First() : null|];
        s = [|(x.Any()) ? x.First() : null|];
        s = [|x.Any() ? x.First() : default|];
        s = [|x.Any() ? x.First() : default(string)|];

        s = [|!x.Any() ? null : x.First()|];
        s = [|!(x.Any()) ? null : x.First()|];
        s = [|(!(x.Any())) ? null : x.First()|];
        s = [|(!x.Any()) ? null : x.First()|];
        s = [|!x.Any() ? default : x.First()|];
        s = [|!x.Any() ? default(string) : x.First()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        string s = null;

        IEnumerable<string> x = null;

        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
        s = x.FirstOrDefault();

        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
        s = x.FirstOrDefault();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallFirstOrDefaultInsteadOfConditionalExpression_ValueType()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        int i = 0;

        IEnumerable<int> x = null;

        i = [|x.Any() ? x.First() : 0|];
        i = [|x.Any() ? x.First() : default|];
        i = [|x.Any() ? x.First() : default(int)|];

        i = [|!x.Any() ? 0 : x.First()|];
        i = [|!x.Any() ? default : x.First()|];
        i = [|!x.Any() ? default(int) : x.First()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        int i = 0;

        IEnumerable<int> x = null;

        i = x.FirstOrDefault();
        i = x.FirstOrDefault();
        i = x.FirstOrDefault();

        i = x.FirstOrDefault();
        i = x.FirstOrDefault();
        i = x.FirstOrDefault();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallOrderByDescendingInsteadOfOrderByAndReverse()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.[|OrderBy(f => f).Reverse()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.OrderByDescending(f => f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallOrderByAndWhereInReverseOrder()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.[|OrderBy(f => f).Where(_ => true)|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.Where(_ => true).OrderBy(f => f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallOrderByAndWhereInReverseOrder2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.[|OrderBy(f => f, default(IComparer<object>)).Where(_ => true)|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.Where(_ => true).OrderBy(f => f, default(IComparer<object>));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallOrderByDescendingAndWhereInReverseOrder()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.[|OrderByDescending(f => f).Where(_ => true)|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.Where(_ => true).OrderByDescending(f => f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallOrderByDescendingAndWhereInReverseOrder2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.[|OrderByDescending(f => f, default(IComparer<object>)).Where(_ => true)|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> x = null;

        x = x.Where(_ => true).OrderByDescending(f => f, default(IComparer<object>));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallSumInsteadOfSelectManyAndCount()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<List<object>> x = null;

        int count = x.[|SelectMany(f => f).Count()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<List<object>> x = null;

        int count = x.Sum(f => f.Count);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task Test_CallConvertAllInsteadOfSelectAndToList_List()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<object> x = null;

        List<string> x2 = x.[|Select(f => f.ToString()).ToList()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<object> x = null;

        List<string> x2 = x.ConvertAll(f => f.ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_CallOfTypeInsteadOfWhereAndCast()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        IEnumerable<object> q = items.Where(f => f is string).Cast<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_CombineWhereAndAny()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.Where(f => f.StartsWith(""a"")).Any(g => g.StartsWith(""b""))) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_FirstOrDefault_ValueType()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Immutable;
using System.Linq;

#pragma warning disable CS0472

class C
{
    void M()
    {
        var items = Enumerable.Empty<int>();

        if (items.FirstOrDefault(_ => true) != null) { }
        if (items.FirstOrDefault(_ => true) == null) { }
    }

    void M2()
    {
        ImmutableArray<int> items = ImmutableArray<int>.Empty;

        if (items.FirstOrDefault(_ => true) != null) { }
        if (items.FirstOrDefault(_ => true) == null) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_CallCastInsteadOfSelect_Conversion()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;

class C
{
    void M()
    {
            var items = Enumerable.Empty<C2>().Select(f => (C)f);

            var x1 = Enumerable.Empty<int>().Select(i => (byte)i);
            var x2 = Enumerable.Empty<int>().Select(i => (long)i);
            var x3 = Enumerable.Select(Enumerable.Empty<int>(), i => (byte)i);
            var x4 = Enumerable.Select(Enumerable.Empty<int>(), i => (long)i);
    }
}

class C2
{
    public static explicit operator C(C2 value)
    {
        return new C();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_OptimizeCountCall_IEnumerableOfT()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var count = Enumerable.Empty<object>().Count();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_OptimizeCountCall_ExpressionCannotBeMemberAccessExpression()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;

class C
{
    void M(Action action)
    {
        var items = new List<object>();

        items.Count();

        M(() => items.Count());

        M(_ => items.Count());
    }

    void M(Action<object> action)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_OptimizeCountCall_InfiniteRecursion()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class C : IReadOnlyCollection<int>
{
    public int Count => this.Count();

    public IEnumerator<int> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_OptimizeCountCall_InfiniteRecursion2()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class C : IReadOnlyCollection<int>
{
    public int Count
    {
        get { return this.Count(); }
    }

        public IEnumerator<int> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_CallAnyInsteadOfCount()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        int i = 0;
        IEnumerable<object> items = null;

        if (items.Count() == 1) { }
        if (items.Count() == i) { }
        if (items.Count() != 1) { }
        if (items.Count() != i) { }
        if (items.Count() > i) { }
        if (items.Count() >= i) { }
        if (items.Count() <= i) { }
        if (items.Count() < i) { }
        if (1 == items.Count()) { }
        if (i == items.Count()) { }
        if (1 != items.Count()) { }
        if (i != items.Count()) { }
        if (i < items.Count()) { }
        if (i <= items.Count()) { }
        if (i >= items.Count()) { }
        if (i > items.Count()) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_CallFindInsteadOfFirstOrDefault_Array_ConditionalAccess()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var x = new C();

        object item = x?.Items.FirstOrDefault(f => f != null);
    }

    object[] Items { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeLinqMethodCall)]
        public async Task TestNoDiagnostic_CallSumInsteadOfSelectManyAndCount()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        _ = Enumerable.Empty<string>()
            .SelectMany(M2())
            .Count();
    }

    Func<string, IEnumerable<string>> M2() => null;
}
");
        }
    }
}
