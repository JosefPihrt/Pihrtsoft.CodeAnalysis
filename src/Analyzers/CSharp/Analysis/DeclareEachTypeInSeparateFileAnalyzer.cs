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
    public sealed class DeclareEachTypeInSeparateFileAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.DeclareEachTypeInSeparateFile);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCompilationUnit(f), SyntaxKind.CompilationUnit);
        }

        private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            SyntaxList<MemberDeclarationSyntax> compilationUnitMembers = compilationUnit.Members;

            if (!compilationUnitMembers.Any())
                return;

            if (ContainsSingleNamespaceWithSingleNonNamespaceMember(compilationUnitMembers))
                return;

            MemberDeclarationSyntax firstTypeDeclaration = null;
            var isFirstReported = false;

            Analyze(compilationUnitMembers);

            void Analyze(SyntaxList<MemberDeclarationSyntax> members)
            {
                foreach (MemberDeclarationSyntax member in members)
                {
                    SyntaxKind kind = member.Kind();

                    if (kind == SyntaxKind.NamespaceDeclaration)
                    {
                        var namespaceDeclaration = (NamespaceDeclarationSyntax)member;

                        Analyze(namespaceDeclaration.Members);
                    }
                    else if (SyntaxFacts.IsTypeDeclaration(kind))
                    {
                        if (firstTypeDeclaration == null)
                        {
                            firstTypeDeclaration = member;
                        }
                        else
                        {
                            if (!isFirstReported)
                            {
                                ReportDiagnostic(context, firstTypeDeclaration);
                                isFirstReported = true;
                            }

                            ReportDiagnostic(context, member);
                        }
                    }
                }
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax member)
        {
            SyntaxToken token = CSharpUtility.GetIdentifier(member);

            if (token == default)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.DeclareEachTypeInSeparateFile, token);
        }

        private static bool ContainsSingleNamespaceWithSingleNonNamespaceMember(SyntaxList<MemberDeclarationSyntax> members)
        {
            MemberDeclarationSyntax member = members.SingleOrDefault(shouldThrow: false);

            if (member?.Kind() != SyntaxKind.NamespaceDeclaration)
                return false;

            var namespaceDeclaration = (NamespaceDeclarationSyntax)member;

            member = namespaceDeclaration.Members.SingleOrDefault(shouldThrow: false);

            return member != null
                && member.Kind() != SyntaxKind.NamespaceDeclaration;
        }
    }
}
