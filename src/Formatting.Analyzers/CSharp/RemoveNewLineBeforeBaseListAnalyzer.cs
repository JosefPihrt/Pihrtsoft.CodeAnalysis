// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveNewLineBeforeBaseListAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveNewLineBeforeBaseList);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEnumDeclaration(f), SyntaxKind.EnumDeclaration);
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;

            BaseListSyntax baseList = typeDeclaration.BaseList;

            if (baseList == null)
                return;

            if (!baseList.Types.Any())
                return;

            SyntaxToken previousToken = typeDeclaration.TypeParameterList?.GreaterThanToken ?? typeDeclaration.Identifier;

            Analyze(context, baseList, previousToken);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            BaseListSyntax baseList = enumDeclaration.BaseList;

            if (baseList == null)
                return;

            if (!baseList.Types.Any())
                return;

            Analyze(context, baseList, enumDeclaration.Identifier);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, BaseListSyntax baseList, SyntaxToken previousToken)
        {
            SyntaxTriviaList trailingTrivia = previousToken.TrailingTrivia;

            if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenEndOfLineTrivia(trailingTrivia))
                return;

            if (!baseList.ColonToken.LeadingTrivia.IsEmptyOrWhitespace())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveNewLineBeforeBaseList,
                Location.Create(baseList.SyntaxTree, new TextSpan(trailingTrivia.Last().SpanStart, 0)));
        }
    }
}
