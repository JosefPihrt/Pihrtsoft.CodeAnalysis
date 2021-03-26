// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseExplicitTypeInsteadOfVarInForEachAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseExplicitTypeInsteadOfVarInForEach); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeForEachStatement(f), SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeForEachVariableStatement(f), SyntaxKind.ForEachVariableStatement);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;

            if (CSharpTypeAnalysis.IsImplicitThatCanBeExplicit(forEachStatement, context.SemanticModel))
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.UseExplicitTypeInsteadOfVarInForEach,
                    forEachStatement.Type);
            }
        }

        private static void AnalyzeForEachVariableStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachVariableStatementSyntax)context.Node;

            switch (forEachStatement.Variable)
            {
                case DeclarationExpressionSyntax declarationExpression:
                    {
                        if (CSharpTypeAnalysis.IsImplicitThatCanBeExplicit(forEachStatement, context.SemanticModel))
                            ReportDiagnostic(context, declarationExpression.Type);

                        break;
                    }
                case TupleExpressionSyntax tupleExpression:
                    {
                        foreach (ArgumentSyntax argument in tupleExpression.Arguments)
                        {
                            if (!(argument.Expression is DeclarationExpressionSyntax declarationExpression))
                                continue;

                            if (CSharpTypeAnalysis.IsImplicitThatCanBeExplicit(declarationExpression, context.SemanticModel, context.CancellationToken))
                                ReportDiagnostic(context, declarationExpression.Type);
                        }

                        break;
                    }
                default:
                    {
                        Debug.Assert(forEachStatement.ContainsDiagnostics, forEachStatement.Variable.Kind().ToString());
                        break;
                    }
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, TypeSyntax type)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.UseExplicitTypeInsteadOfVarInForEach,
                type);
        }
    }
}
