// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeNameEquals(f), SyntaxKind.NameEquals);
            context.RegisterSyntaxNodeAction(f => AnalyzeEqualsValueClause(f), SyntaxKind.EqualsValueClause);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.AddAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.AndAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.CoalesceAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.DivideAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.ExclusiveOrAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.LeftShiftAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.ModuloAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.MultiplyAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.OrAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.RightShiftAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.SimpleAssignmentExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAssignmentExpression(f), SyntaxKind.SubtractAssignmentExpression);
        }

        private static void AnalyzeNameEquals(SyntaxNodeAnalysisContext context)
        {
            var node = (NameEqualsSyntax)context.Node;

            switch (node.Parent.Kind())
            {
                case SyntaxKind.AttributeArgument:
                    {
                        var attributeArgument = (AttributeArgumentSyntax)node.Parent;

                        Analyze(context, attributeArgument.NameEquals.EqualsToken, attributeArgument.Expression);
                        break;
                    }
                case SyntaxKind.AnonymousObjectMemberDeclarator:
                    {
                        var declarator = (AnonymousObjectMemberDeclaratorSyntax)node.Parent;

                        Analyze(context, declarator.NameEquals.EqualsToken, declarator.Expression);
                        break;
                    }
                default:
                    {
                        Debug.Fail(node.Parent.Kind().ToString());
                        break;
                    }
            }
        }

        private static void AnalyzeEqualsValueClause(SyntaxNodeAnalysisContext context = default)
        {
            var node = (EqualsValueClauseSyntax)context.Node;

            Analyze(context, node.EqualsToken, node.Value);
        }

        private static void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (AssignmentExpressionSyntax)context.Node;

            Analyze(context, node.OperatorToken, node.Right);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, SyntaxToken token, ExpressionSyntax expression)
        {
            FormattingSuggestion suggestion = FormattingAnalysis.AnalyzeNewLineBeforeOrAfter(context, token, expression, AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt);

            if (suggestion == FormattingSuggestion.AddNewLineBefore)
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa,
                    token.GetLocation());
            }
            else if (suggestion == FormattingSuggestion.AddNewLineAfter)
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.ReportOnly.AddNewLineAfterEqualsSignInsteadOfBeforeIt,
                    token.GetLocation(),
                    properties: DiagnosticProperties.AnalyzerOption_Invert);
            }
        }
    }
}
