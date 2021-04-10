// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis.UsePatternMatching
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UsePatternMatchingInsteadOfAsAndNullCheckAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UsePatternMatchingInsteadOfAsAndNullCheck);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion < LanguageVersion.CSharp7)
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeAsExpression(f), SyntaxKind.AsExpression);
            });
        }

        private static void AnalyzeAsExpression(SyntaxNodeAnalysisContext context)
        {
            var asExpression = (BinaryExpressionSyntax)context.Node;

            AsExpressionInfo asExpressionInfo = SyntaxInfo.AsExpressionInfo(asExpression);

            if (!asExpressionInfo.Success)
                return;

            SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo(asExpression);

            if (!localInfo.Success)
                return;

            if (localInfo.Statement.SpanOrTrailingTriviaContainsDirectives())
                return;

            if (!(localInfo.Statement.NextStatement() is IfStatementSyntax ifStatement))
                return;

            if (!ifStatement.IsSimpleIf())
                return;

            if (ifStatement.SpanOrLeadingTriviaContainsDirectives())
                return;

            StatementSyntax statement = ifStatement.SingleNonBlockStatementOrDefault();

            if (statement == null)
                return;

            if (!CSharpFacts.IsJumpStatement(statement.Kind()))
                return;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(ifStatement.Condition, NullCheckStyles.EqualsToNull | NullCheckStyles.IsNull);

            if (!nullCheck.Success)
                return;

            if (!string.Equals(localInfo.IdentifierText, (nullCheck.Expression as IdentifierNameSyntax)?.Identifier.ValueText, StringComparison.Ordinal))
                return;

            if (!localInfo.Type.IsVar)
            {
                SemanticModel semanticModel = context.SemanticModel;
                CancellationToken cancellationToken = context.CancellationToken;

                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(asExpressionInfo.Type, cancellationToken);

                if (typeSymbol.IsNullableType())
                    return;

                if (!SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeSymbol(localInfo.Type, cancellationToken), typeSymbol))
                    return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePatternMatchingInsteadOfAsAndNullCheck, localInfo.Statement);
        }
    }
}
