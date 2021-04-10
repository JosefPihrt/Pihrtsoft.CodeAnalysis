// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryExplicitUseOfEnumeratorAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UnnecessaryExplicitUseOfEnumerator);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeUsingStatement(f), SyntaxKind.UsingStatement);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            VariableDeclaratorSyntax declarator = usingStatement.Declaration?.Variables.SingleOrDefault(shouldThrow: false);

            if (declarator == null)
                return;

            if (!(usingStatement.Statement?.SingleNonBlockStatementOrDefault() is WhileStatementSyntax whileStatement))
                return;

            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(whileStatement.Condition);

            if (!invocationInfo.Success)
                return;

            if (invocationInfo.Arguments.Any())
                return;

            if (!string.Equals(invocationInfo.NameText, WellKnownMemberNames.MoveNextMethodName, StringComparison.Ordinal))
                return;

            if (!string.Equals((invocationInfo.Expression as IdentifierNameSyntax)?.Identifier.ValueText, declarator.Identifier.ValueText, StringComparison.Ordinal))
                return;

            SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(declarator.Initializer.Value);

            if (!invocationInfo2.Success)
                return;

            if (invocationInfo2.Arguments.Any())
                return;

            if (!string.Equals(invocationInfo2.NameText, WellKnownMemberNames.GetEnumeratorMethodName, StringComparison.Ordinal))
                return;

            UnnecessaryUsageOfEnumeratorWalker walker = UnnecessaryUsageOfEnumeratorWalker.GetInstance();

            walker.SetValues(declarator, context.SemanticModel, context.CancellationToken);

            walker.Visit(whileStatement.Statement);

            bool? isFixable = walker.IsFixable;

            UnnecessaryUsageOfEnumeratorWalker.Free(walker);

            if (isFixable == true)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnnecessaryExplicitUseOfEnumerator, usingStatement.UsingKeyword);
            }
        }
    }
}
