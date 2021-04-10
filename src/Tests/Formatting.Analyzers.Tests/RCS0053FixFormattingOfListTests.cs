// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0053FixFormattingOfListTests : AbstractCSharpDiagnosticVerifier<FixFormattingOfListAnalyzer, FixFormattingOfListCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.FixFormattingOfList;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Singleline_AlignedToParenthesis()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
           [|object x, object y|]) 
    {
    }
}
", @"
class C
{
    void M(
        object x, object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Singleline_NoIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
[|object x, object y|]) 
    {
    }
}
", @"
class C
{
    void M(
        object x, object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Singleline_EmptyLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(

[|object x, object y|]) 
    {
    }
}
", @"
class C
{
    void M(

        object x, object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Singleline_Comment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M( // x
    [|object x, object y|]) 
    {
    }
}
", @"
class C
{
    void M( // x
        object x, object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Singleline_EmptyLine_Comment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
// x
[|object x, object y|]) 
    {
    }
}
", @"
class C
{
    void M(
// x
        object x, object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Singleline_BaseList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections;
using System.Collections.Generic;

namespace N
{
    interface IC :
[|IEnumerable, IEnumerable<object>|]
    {
    }
}
", @"
using System;
using System.Collections;
using System.Collections.Generic;

namespace N
{
    interface IC :
        IEnumerable, IEnumerable<object>
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_AlignedToParenthesis()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
    }

    [Obsolete]
    void M2(
            [|object x,
            object y|]) 
    {
    }
}
", @"
using System;

class C
{
    void M()
    {
    }

    [Obsolete]
    void M2(
        object x,
        object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_AlignedToParenthesis_WhitespaceAfterParenthesis()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M( [|object x,
           object y|]) 
    {
    }
}
", @"
class C
{
    void M(
        object x,
        object y) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_FirstParameterNotWrapped()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    class C
    {
        void M([|object x,
            object y|]) 
        {
        }
    }
}
", @"
namespace N
{
    class C
    {
        void M(
            object x,
            object y) 
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_TwoParametersOnSameLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
        [|object x,
        object y,object z|]) 
    {
    }
}
", @"
class C
{
    void M(
        object x,
        object y,
        object z) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_TwoParametersOnSameLine2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
        [|object x,
        object y, object z,
        object p4|]) 
    {
    }
}
", @"
class C
{
    void M(
        object x,
        object y,
        object z,
        object p4) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_Comment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M( // x
 // x
    [|object x, // xx
    object y|])
    {
    }
}
", @"
class C
{
    void M( // x
 // x
        object x, // xx
        object y)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_FirstParameterIsMultilineLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(Func<string, string> x, object y)
    {
        M([|f =>
{
    return null;
},
y|]);
    }
}
", @"
using System;

class C
{
    void M(Func<string, string> x, object y)
    {
        M(
            f =>
            {
                return null;
            },
            y);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_FirstParameterIsMultilineLambda2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(Func<string, string> x, object y)
    {
        M([|f =>
            {
                return null;
            },
y|]);
    }
}
", @"
using System;

class C
{
    void M(Func<string, string> x, object y)
    {
        M(
            f =>
            {
                return null;
            },
            y);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SecondParameterIsMultilineLambdaWithExpressionBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(object x, Func<string, string> y)
    {
        M([|x, f => f
            .ToString()
            .ToString()|]);
    }
}
", @"
using System;

class C
{
    void M(object x, Func<string, string> y)
    {
        M(
            x,
            f => f
                .ToString()
                .ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SecondParameterIsMultilineLambdaWithBlockBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(object x, Func<string, string> y)
    {
        M([|x, f =>
        {
            return f
                .ToString()
                .ToString();
        }|]);
    }
}
", @"
using System;

class C
{
    void M(object x, Func<string, string> y)
    {
        M(
            x,
            f =>
            {
                return f
                    .ToString()
                    .ToString();
            });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SecondParameterIsMultilineLambdaWithBlockBody2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(object x, Func<string, string> y)
    {
        M([|x, f => {
            return f
                .ToString()
                .ToString();
        }|]);
    }
}
", @"
using System;

class C
{
    void M(object x, Func<string, string> y)
    {
        M(
            x,
            f => {
                return f
                    .ToString()
                    .ToString();
            });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_MultilineLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(Func<string, string> x, string y)
    {
        M([|f =>
        {
            string s = null;

            //x
            return null;
        }, y|]);
    }
}
", @"
using System;

class C
{
    void M(Func<string, string> x, string y)
    {
        M(
            f =>
            {
                string s = null;

                //x
                return null;
            },
            y);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_MultilineLambda2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M(Func<string, string> x)
    {
        M([|f =>
            {
                return null;
            }|]);
    }
}
", @"
using System;

class C
{
    void M(Func<string, string> x)
    {
        M(f =>
        {
            return null;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_IndentationsDiffer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, string y)
    {
        M([|x, y.ToString()
            .ToString()|]);
    }
}
", @"
class C
{
    void M(object x, string y)
    {
        M(
            x,
            y.ToString()
                .ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_IndentationsDiffer_Tab()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
	void M(object x, string y)
	{
		M([|x, y.ToString()
			.ToString()|]);
	}
}
", @"
class C
{
	void M(object x, string y)
	{
		M(
			x,
			y.ToString()
				.ToString());
	}
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SecondParameterIsAlreadyIndented()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M(string x, IEnumerable<string> y)
    {
        M([|x,
            Enumerable.Empty<string>()
                .OrderBy(f => f)
                .Select(f => f)|]
        );
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M(string x, IEnumerable<string> y)
    {
        M(
            x,
            Enumerable.Empty<string>()
                .OrderBy(f => f)
                .Select(f => f)
        );
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_ConditionalExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string x, string y)
    {
        M([|x, (y != null)
            ? y
            : """"|]);
    }
}
", @"
class C
{
    void M(string x, string y)
    {
        M(
            x,
            (y != null)
                ? y
                : """");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SingleMultilineParameter_WrongIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string x)
    {
        M([|(x != null)
    ? x
    : """"|]);
    }
}
", @"
class C
{
    void M(string x)
    {
        M((x != null)
            ? x
            : """");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SingleMultilineParameter_WrongIndentation2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string x)
    {
        M([|M2(
                [|x,
                x|])|]);
    }

    string M2(string x, string y) => null;
}
", @"
class C
{
    void M(string x)
    {
        M(M2(
            x,
            x));
    }

    string M2(string x, string y) => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_SingleMultilineParameter_NoIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string x)
    {
        M([|(x != null)
? x
: """"|]);
    }
}
", @"
class C
{
    void M(string x)
    {
        M((x != null)
            ? x
            : """");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_BaseConstructorArguments()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public C(string x, string y) : base([|x, (y != null)
        ? y
        : """"|])
    {
    }
}

class B
{
    public B(string x, string y)
    {
    }
}
", @"
class C : B
{
    public C(string x, string y) : base(
        x,
        (y != null)
            ? y
            : """")
    {
    }
}

class B
{
    public B(string x, string y)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_BaseConstructorArguments2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public C(string x, string y)
        : base([|x, (y != null)
            ? y
            : """"|])
    {
    }
}

class B
{
    public B(string x, string y)
    {
    }
}
", @"
class C : B
{
    public C(string x, string y)
        : base(
            x,
            (y != null)
                ? y
                : """")
    {
    }
}

class B
{
    public B(string x, string y)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_StringLiteral()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string x, string y)
    {
        M([|x, @""
a
b
c
""|]);
    }
}
", @"
class C
{
    void M(string x, string y)
    {
        M(
            x,
            @""
a
b
c
"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_AttributeList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [[|Flags, Obsolete(
        """",
        true)|]]
    enum E
    {
    }
}
", @"
using System;

class C
{
    [Flags,
        Obsolete(
            """",
            true)]
    enum E
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_AttributeList_NotIndented()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [[|Flags, Obsolete(
[|"""",
true|])|]]
    enum E
    {
    }
}
", @"
using System;

class C
{
    [Flags,
        Obsolete(
            """",
            true)]
    enum E
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_BaseList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections;
using System.Collections.Generic;

namespace N
{
    interface IC : [|IEnumerable,
IEnumerable<object>|]
    {
    }
}
", @"
using System;
using System.Collections;
using System.Collections.Generic;

namespace N
{
    interface IC :
        IEnumerable,
        IEnumerable<object>
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_OneOfParametersIsMultiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M([|string x, string y
        = default(string), string z = default|])
    {
    }
}
", @"
class C
{
    void M(
        string x,
        string y
            = default(string),
        string z = default)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_TupleType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private ([|string x, string y,
        string z|]) M()
    {
        return ([|null, null,
            null|]);
    }
}
", @"
class C
{
    private (
        string x,
        string y,
        string z) M()
    {
        return (
            null,
            null,
            null);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_TupleExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(([|string x, string y,
        string z|]) p)
    {
    }
}
", @"
class C
{
    void M((
        string x,
        string y,
        string z) p)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_TupleExpression_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        ([|string x,
            string y, string z|]) = default((string, string, string));
    }
}
", @"
class C
{
    void M()
    {
        (string x,
            string y,
            string z) = default((string, string, string));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task Test_Multiline_ArrayInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new string[] { [|"""", default, new string(
            ' ',
            1)|] };
    }
}
", @"
class C
{
    void M()
    {
        var x = new string[] {
            """",
            default,
            new string(
                ' ',
                1) };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_Singleline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(object x, object y, object z) 
    {
    }

    void M2(
        object x, object y, object z) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_Multiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
    }

    void M2(
        object x,
        object y,
        object z) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_Multiline2()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
void M(
    object x,
    object y,
    object z) 
{
}
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_SingleParameterIsMultilineLambdaWithExpressionBody()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(Func<string, string> func)
    {
        M(f => f
            .ToString()
            .ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_SingleParameterIsMultilineLambdaWithExpressionBody2()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;

class C
{
    void M()
    {
        string s = string.Concat(Enumerable.Empty<string>().Select(f =>
        {
            return f;
        }));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_SingleParameterIsMultilineLambdaWithExpressionBody3()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;

class C
{
    void M()
    {
        string s = string.Concat(Enumerable.Empty<string>().Select(f => {
            return f;
        }));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_SingleMultilineParameter()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string x)
    {
        M((x != null)
            ? x
            : """");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfList)]
        public async Task TestNoDiagnostic_ArrayInitializerWithMultilineComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var x = new string[]
        {
            /* x */ """"
        };
    }
}
");
        }
    }
}
