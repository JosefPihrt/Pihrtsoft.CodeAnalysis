// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1080UseCountOrLengthPropertyInsteadOfAnyMethodTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, InvocationExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseCountOrLengthPropertyInsteadOfAnyMethod;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_List()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<object> items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<object> items = null;

        bool any = items.Count > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_ListNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<object> items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<object> items = null;

        bool any = items.Count == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_IList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IList<object> items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IList<object> items = null;

        bool any = items.Count > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_IListNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IList<object> items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IList<object> items = null;

        bool any = items.Count == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_IReadOnlyList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyList<object> items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyList<object> items = null;

        bool any = items.Count > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_IReadOnlyListNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyList<object> items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyList<object> items = null;

        bool any = items.Count == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_Collection()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.ObjectModel;
using System.Linq;

class C
{
    void M()
    {
        Collection<object> items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.ObjectModel;
using System.Linq;

class C
{
    void M()
    {
        Collection<object> items = null;

        bool any = items.Count > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_CollectionNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.ObjectModel;
using System.Linq;

class C
{
    void M()
    {
        Collection<object> items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.ObjectModel;
using System.Linq;

class C
{
    void M()
    {
        Collection<object> items = null;

        bool any = items.Count == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_ICollection()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        ICollection<object> items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        ICollection<object> items = null;

        bool any = items.Count > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_ICollectionNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        ICollection<object> items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        ICollection<object> items = null;

        bool any = items.Count == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_IReadOnlyCollection()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyCollection<object> items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyCollection<object> items = null;

        bool any = items.Count > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_IReadOnlyCollectionNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyCollection<object> items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IReadOnlyCollection<object> items = null;

        bool any = items.Count == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_Array()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        object[] items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        object[] items = null;

        bool any = items.Length > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_ArrayNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        object[] items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        object[] items = null;

        bool any = items.Length == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_String()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        string items = null;

        bool any = items.[|Any()|] //x
            ;
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        string items = null;

        bool any = items.Length > 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task Test_StringNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;

class C
{
    void M()
    {
        string items = null;

        bool any = !items.[|Any()|] //x
            ;
    }
}
", @"
using System.Linq;

class C
{
    void M()
    {
        string items = null;

        bool any = items.Length == 0 //x
            ;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task TestNoDiagnostic_ImmutableArray()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Immutable;

class C
{
    void M()
    {
        ImmutableArray<object> items = ImmutableArray<object>.Empty;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod)]
        public async Task TestNoDiagnostic_IEnumerable()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> items = null;
    }
}
");
        }
    }
}