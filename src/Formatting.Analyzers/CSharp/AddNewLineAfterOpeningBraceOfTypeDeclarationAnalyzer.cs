// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddNewLineAfterOpeningBraceOfTypeDeclarationAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineAfterOpeningBraceOfTypeDeclaration); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.InterfaceDeclaration);
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;

            SyntaxToken openBrace = typeDeclaration.OpenBraceToken;

            if (openBrace.IsMissing)
                return;

            if (!typeDeclaration.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(openBrace.Span.End, openBrace.GetNextToken().SpanStart)))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddNewLineAfterOpeningBraceOfTypeDeclaration,
                Location.Create(typeDeclaration.SyntaxTree, new TextSpan(openBrace.Span.End, 0)));
        }
    }
}
