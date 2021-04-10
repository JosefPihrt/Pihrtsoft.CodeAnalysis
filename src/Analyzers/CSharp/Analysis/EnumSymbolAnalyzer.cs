// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EnumSymbolAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.DeclareEnumMemberWithZeroValue,
                        DiagnosticRules.CompositeEnumValueContainsUndefinedFlag,
                        DiagnosticRules.DeclareEnumValueAsCombinationOfNames,
                        DiagnosticRules.DuplicateEnumValue,
                        DiagnosticRules.UseBitShiftOperator);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSymbolAction(f => AnalyzeNamedType(f), SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (typeSymbol.IsImplicitlyDeclared)
                return;

            if (typeSymbol.TypeKind != TypeKind.Enum)
                return;

            bool hasFlagsAttribute = typeSymbol.HasAttribute(MetadataNames.System_FlagsAttribute);

            ImmutableArray<ISymbol> members = default;

            if (hasFlagsAttribute
                && DiagnosticRules.DeclareEnumMemberWithZeroValue.IsEffective(context))
            {
                members = typeSymbol.GetMembers();

                if (!ContainsFieldWithZeroValue(members))
                {
                    var enumDeclaration = (EnumDeclarationSyntax)typeSymbol.GetSyntax(context.CancellationToken);

                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.DeclareEnumMemberWithZeroValue, enumDeclaration.Identifier);
                }
            }

            EnumSymbolInfo enumInfo = default;

            if (hasFlagsAttribute
                && DiagnosticRules.CompositeEnumValueContainsUndefinedFlag.IsEffective(context))
            {
                enumInfo = EnumSymbolInfo.Create(typeSymbol);

                foreach (EnumFieldSymbolInfo field in enumInfo.Fields)
                {
                    if (field.HasValue
                        && ConvertHelpers.CanConvertFromUInt64(field.Value, typeSymbol.EnumUnderlyingType.SpecialType)
                        && !IsMaxValue(field.Value, typeSymbol.EnumUnderlyingType.SpecialType)
                        && field.HasCompositeValue())
                    {
                        foreach (ulong value in (field.GetFlags()))
                        {
                            if (!enumInfo.Contains(value))
                                ReportUndefinedFlag(context, field.Symbol, value.ToString());
                        }
                    }
                }
            }

            if (hasFlagsAttribute
                && DiagnosticRules.DeclareEnumValueAsCombinationOfNames.IsEffective(context))
            {
                if (members.IsDefault)
                    members = typeSymbol.GetMembers();

                foreach (ISymbol member in members)
                {
                    if (!(member is IFieldSymbol fieldSymbol))
                        continue;

                    if (!fieldSymbol.HasConstantValue)
                        break;

                    EnumFieldSymbolInfo fieldInfo = EnumFieldSymbolInfo.Create(fieldSymbol);

                    if (!fieldInfo.HasCompositeValue())
                        continue;

                    var declaration = (EnumMemberDeclarationSyntax)fieldInfo.Symbol.GetSyntax(context.CancellationToken);

                    ExpressionSyntax expression = declaration.EqualsValue?.Value.WalkDownParentheses();

                    if (expression != null
                        && (expression.IsKind(SyntaxKind.NumericLiteralExpression)
                            || expression
                                .DescendantNodes()
                                .Any(f => f.IsKind(SyntaxKind.NumericLiteralExpression))))
                    {
                        if (enumInfo.IsDefault)
                        {
                            enumInfo = EnumSymbolInfo.Create(typeSymbol);

                            if (enumInfo.Fields.Any(f => !f.HasValue))
                                break;
                        }

                        List<EnumFieldSymbolInfo> values = enumInfo.Decompose(fieldInfo);

                        if (values?.Count > 1)
                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.DeclareEnumValueAsCombinationOfNames, expression);
                    }
                }
            }

            if (hasFlagsAttribute
                && DiagnosticRules.UseBitShiftOperator.IsEffective(context))
            {
                if (members.IsDefault)
                    members = typeSymbol.GetMembers();

                foreach (ISymbol member in members)
                {
                    if (!(member is IFieldSymbol fieldSymbol))
                        continue;

                    if (!fieldSymbol.HasConstantValue)
                        continue;

                    EnumFieldSymbolInfo fieldInfo = EnumFieldSymbolInfo.Create(fieldSymbol);

                    if (fieldInfo.Value <= 1)
                        continue;

                    if (fieldInfo.HasCompositeValue())
                        continue;

                    var declaration = (EnumMemberDeclarationSyntax)fieldInfo.Symbol.GetSyntax(context.CancellationToken);

                    ExpressionSyntax expression = declaration.EqualsValue?.Value.WalkDownParentheses();

                    if (expression.IsKind(SyntaxKind.NumericLiteralExpression))
                    {
                        var enumDeclaration = (EnumDeclarationSyntax)typeSymbol.GetSyntax(context.CancellationToken);

                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseBitShiftOperator, enumDeclaration.Identifier);
                        break;
                    }
                }
            }

            if (DiagnosticRules.DuplicateEnumValue.IsEffective(context))
            {
                if (enumInfo.IsDefault)
                    enumInfo = EnumSymbolInfo.Create(typeSymbol);

                ImmutableArray<EnumFieldSymbolInfo> fields = enumInfo.Fields;

                if (fields.Length > 1)
                {
                    EnumFieldSymbolInfo symbolInfo1 = fields[0];
                    EnumFieldSymbolInfo symbolInfo2 = default;

                    for (int i = 1; i < fields.Length; i++, symbolInfo1 = symbolInfo2)
                    {
                        symbolInfo2 = fields[i];

                        if (!symbolInfo1.HasValue
                            || !symbolInfo2.HasValue
                            || symbolInfo1.Value != symbolInfo2.Value)
                        {
                            continue;
                        }

                        var enumMember1 = (EnumMemberDeclarationSyntax)symbolInfo1.Symbol.GetSyntax(context.CancellationToken);

                        if (enumMember1 == null)
                            continue;

                        var enumMember2 = (EnumMemberDeclarationSyntax)symbolInfo2.Symbol.GetSyntax(context.CancellationToken);

                        if (enumMember2 == null)
                            continue;

                        ExpressionSyntax value1 = enumMember1.EqualsValue?.Value?.WalkDownParentheses();
                        ExpressionSyntax value2 = enumMember2.EqualsValue?.Value?.WalkDownParentheses();

                        if (value1 == null)
                        {
                            if (value2 != null)
                            {
                                ReportDuplicateValue(context, enumMember1, value2);
                            }
                        }
                        else if (value2 == null)
                        {
                            ReportDuplicateValue(context, enumMember2, value1);
                        }
                        else
                        {
                            SyntaxKind kind1 = value1.Kind();
                            SyntaxKind kind2 = value2.Kind();

                            if (kind1 == SyntaxKind.NumericLiteralExpression)
                            {
                                if (kind2 == SyntaxKind.NumericLiteralExpression)
                                {
                                    var enumDeclaration = (EnumDeclarationSyntax)enumMember1.Parent;
                                    SeparatedSyntaxList<EnumMemberDeclarationSyntax> enumMembers = enumDeclaration.Members;

                                    if (enumMembers.IndexOf(enumMember1) < enumMembers.IndexOf(enumMember2))
                                    {
                                        ReportDuplicateValue(context, value2);
                                    }
                                    else
                                    {
                                        ReportDuplicateValue(context, value1);
                                    }
                                }
                                else if (!string.Equals((value2 as IdentifierNameSyntax)?.Identifier.ValueText, enumMember1.Identifier.ValueText, StringComparison.Ordinal))
                                {
                                    ReportDuplicateValue(context, value1);
                                }
                            }
                            else if (kind2 == SyntaxKind.NumericLiteralExpression
                                && !string.Equals((value1 as IdentifierNameSyntax)?.Identifier.ValueText, enumMember2.Identifier.ValueText, StringComparison.Ordinal))
                            {
                                ReportDuplicateValue(context, value2);
                            }
                        }
                    }
                }
            }
        }

        private static bool ContainsFieldWithZeroValue(ImmutableArray<ISymbol> members)
        {
            foreach (ISymbol member in members)
            {
                if (member.Kind == SymbolKind.Field)
                {
                    var fieldSymbol = (IFieldSymbol)member;

                    if (fieldSymbol.HasConstantValue)
                    {
                        EnumFieldSymbolInfo fieldInfo = EnumFieldSymbolInfo.Create(fieldSymbol);

                        if (fieldInfo.Value == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool IsMaxValue(ulong value, SpecialType numericType)
        {
            switch (numericType)
            {
                case SpecialType.System_SByte:
                    return value == (ulong)sbyte.MaxValue;
                case SpecialType.System_Byte:
                    return value == byte.MaxValue;
                case SpecialType.System_Int16:
                    return value == (ulong)short.MaxValue;
                case SpecialType.System_UInt16:
                    return value == ushort.MaxValue;
                case SpecialType.System_Int32:
                    return value == int.MaxValue;
                case SpecialType.System_UInt32:
                    return value == uint.MaxValue;
                case SpecialType.System_Int64:
                    return value == long.MaxValue;
                case SpecialType.System_UInt64:
                    return true;
                case SpecialType.System_Decimal:
                    return value == decimal.MaxValue;
                case SpecialType.System_Single:
                    return value == float.MaxValue;
                case SpecialType.System_Double:
                    return value == double.MaxValue;
                default:
                    throw new ArgumentException("", nameof(numericType));
            }
        }

        private static void ReportUndefinedFlag(SymbolAnalysisContext context, ISymbol fieldSymbol, string value)
        {
            var enumMember = (EnumMemberDeclarationSyntax)fieldSymbol.GetSyntax(context.CancellationToken);

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.CompositeEnumValueContainsUndefinedFlag,
                enumMember.GetLocation(),
                ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("Value", value) }),
                value);
        }

        private static void ReportDuplicateValue(
            SymbolAnalysisContext context,
            EnumMemberDeclarationSyntax enumMember,
            ExpressionSyntax value)
        {
            if (value is IdentifierNameSyntax identifierName
                && string.Equals(enumMember.Identifier.ValueText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.DuplicateEnumValue, enumMember);
        }

        private static void ReportDuplicateValue(
            SymbolAnalysisContext context,
            SyntaxNode node)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.DuplicateEnumValue, node);
        }
    }
}