// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MergePreprocessorDirectivesAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.MergePreprocessorDirectives);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzePragmaWarningDirectiveTrivia(f), SyntaxKind.PragmaWarningDirectiveTrivia);
        }

        private static void AnalyzePragmaWarningDirectiveTrivia(SyntaxNodeAnalysisContext context)
        {
            var directive = (PragmaWarningDirectiveTriviaSyntax)context.Node;

            SeparatedSyntaxList<ExpressionSyntax> errorCodes = directive.ErrorCodes;

            int codeCount = errorCodes.Count;

            if (codeCount == 0)
                return;

            if (codeCount == errorCodes.SeparatorCount)
            {
                if (!errorCodes.GetSeparator(codeCount - 1).TrailingTrivia.IsEmptyOrWhitespace())
                    return;
            }
            else if (!errorCodes.Last().GetTrailingTrivia().IsEmptyOrWhitespace())
            {
                return;
            }

            if (IsSuppressingThisAnalyzer(errorCodes))
                return;

            SyntaxTrivia trivia = directive.ParentTrivia;

            if (!trivia.TryGetContainingList(out SyntaxTriviaList list))
                return;

            int count = list.Count;

            if (count == 1)
                return;

            int index = list.IndexOf(trivia);

            if (index == count - 1)
                return;

            int i = index - 1;

            while (i >= 0
                && list[i].IsWhitespaceOrEndOfLineTrivia())
            {
                i--;
            }

            if (i >= 0)
            {
                SyntaxTrivia directiveTrivia = list[i];

                if (directiveTrivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia))
                {
                    var previousDirective = (PragmaWarningDirectiveTriviaSyntax)directiveTrivia.GetStructure();

                    if (!IsSuppressingThisAnalyzer(previousDirective.ErrorCodes))
                        return;
                }
            }

            i = index + 1;

            while (i < count
                && list[i].IsWhitespaceOrEndOfLineTrivia())
            {
                i++;
            }

            if (i == count)
                return;

            if (!list[i].IsKind(SyntaxKind.PragmaWarningDirectiveTrivia))
                return;

            if (!(list[i].GetStructure() is PragmaWarningDirectiveTriviaSyntax nextDirective))
                return;

            SyntaxToken disableOrRestoreKeyword = directive.DisableOrRestoreKeyword;

            SyntaxKind keywordKind = disableOrRestoreKeyword.Kind();

            if (keywordKind != nextDirective.DisableOrRestoreKeyword.Kind())
                return;

            if (keywordKind == SyntaxKind.DisableKeyword
                && IsSuppressingThisAnalyzer(nextDirective.ErrorCodes))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.MergePreprocessorDirectives, directive);
        }

        private static bool IsSuppressingThisAnalyzer(SeparatedSyntaxList<ExpressionSyntax> errorCodes)
        {
            return errorCodes.SingleOrDefault(shouldThrow: false) is IdentifierNameSyntax identifierName
                && string.Equals(identifierName.Identifier.ValueText, DiagnosticIdentifiers.MergePreprocessorDirectives, StringComparison.Ordinal);
        }
    }
}
