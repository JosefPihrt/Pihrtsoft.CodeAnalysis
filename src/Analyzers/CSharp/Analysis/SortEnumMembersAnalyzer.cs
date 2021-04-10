// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SortEnumMembersAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SortEnumMembers);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeEnumDeclaration(f), SyntaxKind.EnumDeclaration);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            if (IsListUnsorted(enumDeclaration.Members, context.SemanticModel, context.CancellationToken)
                && !enumDeclaration.ContainsDirectives(enumDeclaration.BracesSpan()))
            {
                SyntaxToken identifier = enumDeclaration.Identifier;
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SortEnumMembers, identifier, identifier);
            }
        }

        private static bool IsListUnsorted(
            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            int count = members.Count;

            if (count > 1)
            {
                IFieldSymbol firstField = semanticModel.GetDeclaredSymbol(members[0], cancellationToken);

                if (firstField?.HasConstantValue == true)
                {
                    SpecialType enumSpecialType = firstField.ContainingType.EnumUnderlyingType.SpecialType;

                    object previousValue = firstField.ConstantValue;

                    for (int i = 1; i < count - 1; i++)
                    {
                        IFieldSymbol field = semanticModel.GetDeclaredSymbol(members[i], cancellationToken);

                        if (field?.HasConstantValue != true)
                            return false;

                        object value = field.ConstantValue;

                        if (EnumValueComparer.GetInstance(enumSpecialType).Compare(previousValue, value) > 0)
                        {
                            i++;

                            while (i < count)
                            {
                                if (semanticModel.GetDeclaredSymbol(members[i], cancellationToken)?.HasConstantValue != true)
                                    return false;

                                i++;
                            }

                            return true;
                        }

                        previousValue = value;
                    }
                }
            }

            return false;
        }
    }
}
