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
    public sealed class OrderTypeParameterConstraintsAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.OrderTypeParameterConstraints);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeTypeParameterList(f), SyntaxKind.TypeParameterList);
        }

        private static void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        {
            var typeParameterList = (TypeParameterListSyntax)context.Node;

            GenericInfo genericInfo = SyntaxInfo.GenericInfo(typeParameterList);

            if (!genericInfo.Success)
                return;

            if (!genericInfo.TypeParameters.Any())
                return;

            if (!genericInfo.ConstraintClauses.Any())
                return;

            if (genericInfo.ConstraintClauses.SpanContainsDirectives())
                return;

            if (!IsFixable(genericInfo.TypeParameters, genericInfo.ConstraintClauses))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.OrderTypeParameterConstraints,
                genericInfo.ConstraintClauses[0]);
        }

        private static bool IsFixable(
            SeparatedSyntaxList<TypeParameterSyntax> typeParameters,
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        {
            int lastIndex = -1;

            for (int i = 0; i < typeParameters.Count; i++)
            {
                string name = typeParameters[i].Identifier.ValueText;

                int index = IndexOf(constraintClauses, name);

                if (index != -1)
                {
                    if (index < lastIndex)
                        return true;

                    lastIndex = index;
                }
            }

            return false;
        }

        internal static int IndexOf(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, string name)
        {
            for (int i = 0; i < constraintClauses.Count; i++)
            {
                if (constraintClauses[i].Name.Identifier.ValueText == name)
                    return i;
            }

            return -1;
        }
    }
}
