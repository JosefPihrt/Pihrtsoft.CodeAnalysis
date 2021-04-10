// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnconstrainedTypeParameterCheckedForNullAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UnconstrainedTypeParameterCheckedForNull);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeEqualsExpression(f), SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeNotEqualsExpression(f), SyntaxKind.NotEqualsExpression);
        }

        private static void AnalyzeEqualsExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            Analyze(context, (BinaryExpressionSyntax)context.Node, NullCheckStyles.EqualsToNull);
        }

        private static void AnalyzeNotEqualsExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            Analyze(context, (BinaryExpressionSyntax)context.Node, NullCheckStyles.NotEqualsToNull);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, BinaryExpressionSyntax binaryExpression, NullCheckStyles allowedStyles)
        {
            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(binaryExpression, allowedStyles: allowedStyles);

            if (nullCheck.Success
                && IsUnconstrainedTypeParameter(context.SemanticModel.GetTypeSymbol(nullCheck.Expression, context.CancellationToken))
                && !binaryExpression.SpanContainsDirectives())
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnconstrainedTypeParameterCheckedForNull, binaryExpression);
            }
        }

        private static bool IsUnconstrainedTypeParameter(ITypeSymbol typeSymbol)
        {
            return typeSymbol?.Kind == SymbolKind.TypeParameter
                && VerifyConstraint((ITypeParameterSymbol)typeSymbol, allowReference: false, allowValueType: false, allowConstructor: true);
        }

        private static bool VerifyConstraint(
            ITypeParameterSymbol typeParameterSymbol,
            bool allowReference,
            bool allowValueType,
            bool allowConstructor)
        {
            if (typeParameterSymbol == null)
                throw new ArgumentNullException(nameof(typeParameterSymbol));

            if (!CheckConstraint(typeParameterSymbol, allowReference, allowValueType, allowConstructor))
                return false;

            return VerifyConstraint(typeParameterSymbol.ConstraintTypes, allowReference, allowValueType, allowConstructor);
        }

        private static bool VerifyConstraint(ImmutableArray<ITypeSymbol> constraintTypes, bool allowReference, bool allowValueType, bool allowConstructor)
        {
            if (!constraintTypes.Any())
                return true;

            foreach (ITypeSymbol type in constraintTypes)
            {
                switch (type.TypeKind)
                {
                    case TypeKind.Class:
                        {
                            if (!allowReference)
                                return false;

                            break;
                        }
                    case TypeKind.Struct:
                        {
                            if (allowValueType)
                                return false;

                            break;
                        }
                    case TypeKind.Interface:
                        {
                            break;
                        }
                    case TypeKind.TypeParameter:
                        {
                            var typeParameterSymbol = (ITypeParameterSymbol)type;

                            if (!CheckConstraint(typeParameterSymbol, allowReference, allowValueType, allowConstructor))
                                return false;

                            if (!VerifyConstraint(typeParameterSymbol.ConstraintTypes, allowReference, allowValueType, allowConstructor))
                                return false;

                            break;
                        }
                    case TypeKind.Error:
                        {
                            return false;
                        }
                    default:
                        {
                            Debug.Fail(type.TypeKind.ToString());
                            return false;
                        }
                }
            }

            return true;
        }

        private static bool CheckConstraint(
            ITypeParameterSymbol typeParameterSymbol,
            bool allowReference,
            bool allowValueType,
            bool allowConstructor)
        {
            return (allowReference || !typeParameterSymbol.HasReferenceTypeConstraint)
                && (allowValueType || !typeParameterSymbol.HasValueTypeConstraint)
                && (allowConstructor || !typeParameterSymbol.HasConstructorConstraint);
        }
    }
}
