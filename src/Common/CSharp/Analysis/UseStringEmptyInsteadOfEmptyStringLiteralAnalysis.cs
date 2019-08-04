// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class UseStringEmptyInsteadOfEmptyStringLiteralAnalysis {
        public static bool IsFixable(LiteralExpressionSyntax expressionSyntax)
        {
            return expressionSyntax.GetText()?.Length == 2;
        }
    }
}
