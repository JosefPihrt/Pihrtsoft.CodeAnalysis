// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimpleMemberAccessExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.UsePropertySyntaxNodeSpanStart,
                        DiagnosticRules.CallAnyInsteadOfAccessingCount);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeSimpleMemberAccessExpression(f), SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeSimpleMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            var memberAccessExpression = (MemberAccessExpressionSyntax)context.Node;

            SimpleNameSyntax name = memberAccessExpression.Name;

            switch (name)
            {
                case IdentifierNameSyntax identifierName:
                    {
                        switch (identifierName.Identifier.ValueText)
                        {
                            case "Start":
                                {
                                    ExpressionSyntax expression = memberAccessExpression.Expression;

                                    if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                                        break;

                                    ISymbol symbol = context.SemanticModel.GetSymbol(memberAccessExpression, context.CancellationToken);

                                    if (symbol == null)
                                        break;

                                    if (!symbol.ContainingType.HasMetadataName(RoslynMetadataNames.Microsoft_CodeAnalysis_Text_TextSpan))
                                        break;

                                    var memberAccess2 = (MemberAccessExpressionSyntax)expression;

                                    SimpleNameSyntax name2 = memberAccess2.Name;

                                    if (!(name2 is IdentifierNameSyntax identifierName2))
                                        break;

                                    if (!string.Equals(identifierName2.Identifier.ValueText, "Span", StringComparison.Ordinal))
                                        break;

                                    ISymbol symbol2 = context.SemanticModel.GetSymbol(expression, context.CancellationToken);

                                    if (symbol2 == null)
                                        break;

                                    if (!symbol2.ContainingType.HasMetadataName(RoslynMetadataNames.Microsoft_CodeAnalysis_SyntaxNode))
                                        break;

                                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePropertySyntaxNodeSpanStart, memberAccessExpression);
                                    break;
                                }
                            case "Count":
                                {
                                    CallAnyInsteadOfUsingCount();
                                    break;
                                }
                        }

                        break;
                    }
            }

            void CallAnyInsteadOfUsingCount()
            {
                SyntaxNode expression = memberAccessExpression.WalkUpParentheses();

                SyntaxNode parent = expression.Parent;

                if (!parent.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.GreaterThanExpression))
                    return;

                BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)parent);

                if (!binaryExpressionInfo.Success)
                    return;

                ExpressionSyntax otherExpression = (expression == binaryExpressionInfo.Left)
                    ? binaryExpressionInfo.Right
                    : binaryExpressionInfo.Left;

                if (!otherExpression.IsKind(SyntaxKind.NumericLiteralExpression))
                    return;

                var numericLiteralExpression = (LiteralExpressionSyntax)otherExpression;

                if (numericLiteralExpression.Token.ValueText != "0")
                    return;

                ISymbol symbol = context.SemanticModel.GetSymbol(memberAccessExpression, context.CancellationToken);

                if (symbol?.Kind != SymbolKind.Property
                    || symbol.IsStatic
                    || symbol.DeclaredAccessibility != Accessibility.Public
                    || !RoslynSymbolUtility.IsList(symbol.ContainingType.OriginalDefinition))
                {
                    return;
                }

                TextSpan span = (memberAccessExpression == binaryExpressionInfo.Left)
                    ? TextSpan.FromBounds(name.SpanStart, numericLiteralExpression.Span.End)
                    : TextSpan.FromBounds(numericLiteralExpression.SpanStart, name.Span.End);

                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.CallAnyInsteadOfAccessingCount, Location.Create(memberAccessExpression.SyntaxTree, span));
            }
        }
    }
}
