// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.Testing.CSharp
{
    public static class ExpressionChainTests
    {
        [Fact]
        public static void TestExpressionChainEnumerator()
        {
            var be = (BinaryExpressionSyntax)ParseExpression("a + b + c");
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Enumerator en = new ExpressionChain(be).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be2.Left);
            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainEnumerator_WithSpan()
        {
            const string s = @"
class C
{
    void M(string a, string b, string c, string d)
    {
        string s = [|a + b + c|];
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Enumerator en = new ExpressionChain(be, code.Spans[0]).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be2.Left);
            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainEnumerator_WithSpan2()
        {
            const string s = @"
class C
{
    void M(string a, string b, string c, string d)
    {
        string s = a + b + [|c + d|];
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Enumerator en = new ExpressionChain(be, code.Spans[0]).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainEnumerator_WithSpan3()
        {
            const string s = @"
class C
{
    void M(string a, string b, string c, string d)
    {
        string s = a + [|b + c|] + d;
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();
            be = (BinaryExpressionSyntax)be.Left;
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Enumerator en = new ExpressionChain(be, code.Spans[0]).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainEnumerator_WithSpan4()
        {
            const string s = @"
class C
{
    void M(string a, string b)
    {
        string s = a + [|b|];
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();

            ExpressionChain.Enumerator en = new ExpressionChain(be, code.Spans[0]).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainEnumerator_WithSpan5()
        {
            const string s = @"
class C
{
    void M(string a, string b)
    {
        string s = [|a|] + b;
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();

            ExpressionChain.Enumerator en = new ExpressionChain(be, code.Spans[0]).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Left);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainReversedEnumerator()
        {
            var be = (BinaryExpressionSyntax)ParseExpression("a + b + c");
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Reversed.Enumerator en = new ExpressionChain.Reversed(new ExpressionChain(be)).GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(en.MoveNext() && en.Current == be2.Left);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainReversedEnumerator_WithSpan()
        {
            const string s = @"
class C
{
    void M(string a, string b, string c)
    {
        string s = [|a + b + c|];
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Reversed.Enumerator en = new ExpressionChain(be, code.Spans[0]).Reverse().GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(en.MoveNext() && en.Current == be2.Left);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainReversedEnumerator_WithSpan2()
        {
            const string s = @"
class C
{
    void M(string a, string b, string c, string d)
    {
        string s = a + b + [|c + d|];
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Reversed.Enumerator en = new ExpressionChain(be, code.Spans[0]).Reverse().GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainReversedEnumerator_WithSpan3()
        {
            const string s = @"
class C
{
    void M(string a, string b, string c, string d)
    {
        string s = a + [|b + c|] + d;
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();
            be = (BinaryExpressionSyntax)be.Left;
            var be2 = (BinaryExpressionSyntax)be.Left;

            ExpressionChain.Reversed.Enumerator en = new ExpressionChain(be, code.Spans[0]).Reverse().GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(en.MoveNext() && en.Current == be2.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainReversedEnumerator_WithSpan4()
        {
            const string s = @"
class C
{
    void M(string a, string b)
    {
        string s = a + [|b|];
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();

            ExpressionChain.Reversed.Enumerator en = new ExpressionChain(be, code.Spans[0]).Reverse().GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Right);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestExpressionChainReversedEnumerator_WithSpan5()
        {
            const string s = @"
class C
{
    void M(string a, string b)
    {
        string s = [|a|] + b;
    }
}";
            var code = TestCode.Parse(s);

            BinaryExpressionSyntax be = CSharpSyntaxTree.ParseText(code.Value).GetRoot().FirstDescendant<BinaryExpressionSyntax>();

            ExpressionChain.Reversed.Enumerator en = new ExpressionChain(be, code.Spans[0]).Reverse().GetEnumerator();

            Assert.True(en.MoveNext() && en.Current == be.Left);
            Assert.True(!en.MoveNext());
        }

        [Fact]
        public static void TestBinaryExpressionAsChainEnumerate()
        {
            var be = (BinaryExpressionSyntax)ParseExpression("a + b + c");
            var be2 = (BinaryExpressionSyntax)be.Left;

            Assert.Equal(
                be.AsChain(),
                new ExpressionSyntax[] { be2.Left, be2.Right, be.Right });
        }

        [Fact]
        public static void TestBinaryExpressionAsChainReversedEnumerate()
        {
            var be = (BinaryExpressionSyntax)ParseExpression("a + b + c");
            var be2 = (BinaryExpressionSyntax)be.Left;

            Assert.Equal(
                be.AsChain().Reverse(),
                new ExpressionSyntax[] { be.Right, be2.Right, be2.Left });
        }
    }
}
