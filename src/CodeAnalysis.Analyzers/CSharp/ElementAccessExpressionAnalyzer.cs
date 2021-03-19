// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ElementAccessExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.CallLastInsteadOfUsingElementAccess); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeElementAccessExpression(f), SyntaxKind.ElementAccessExpression);
        }

        private static void AnalyzeElementAccessExpression(SyntaxNodeAnalysisContext context)
        {
            var elementAccessExpression = (ElementAccessExpressionSyntax)context.Node;

            ExpressionSyntax expression = elementAccessExpression
                .ArgumentList
                .Arguments
                .SingleOrDefault(shouldThrow: false)?
                .Expression
                .WalkDownParentheses();

            if (expression == null)
                return;

            if (!expression.IsKind(SyntaxKind.SubtractExpression))
                return;

            BinaryExpressionInfo subtractExpressionInfo = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)expression);

            if (!subtractExpressionInfo.Right.IsNumericLiteralExpression("1"))
                return;

            if (!subtractExpressionInfo.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return;

            var memberAccessExpression = (MemberAccessExpressionSyntax)subtractExpressionInfo.Left;

            if (!(memberAccessExpression.Name is IdentifierNameSyntax identifierName))
                return;

            if (identifierName.Identifier.ValueText != "Count")
                return;

            if (!CSharpFactory.AreEquivalent(elementAccessExpression.Expression, memberAccessExpression.Expression))
                return;

            ISymbol symbol = context.SemanticModel.GetSymbol(elementAccessExpression, context.CancellationToken);

            if (symbol?.Kind != SymbolKind.Property
                || symbol.IsStatic
                || symbol.DeclaredAccessibility != Accessibility.Public
                || !RoslynSymbolUtility.IsList(symbol.ContainingType.OriginalDefinition))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.CallLastInsteadOfUsingElementAccess, elementAccessExpression.ArgumentList);
        }
    }
}
