// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnumDeclarationCodeFixProvider))]
    [Shared]
    public sealed class EnumDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.SortEnumMembers,
                    DiagnosticIdentifiers.EnumShouldDeclareExplicitValues,
                    DiagnosticIdentifiers.UseBitShiftOperator);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out EnumDeclarationSyntax enumDeclaration))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.SortEnumMembers:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                $"Sort '{enumDeclaration.Identifier}' members",
                                cancellationToken => SortEnumMembersAsync(document, enumDeclaration, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.EnumShouldDeclareExplicitValues:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            INamedTypeSymbol enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration, context.CancellationToken);

                            EnumSymbolInfo enumInfo = EnumSymbolInfo.Create(enumSymbol);

                            ImmutableArray<ulong> values = enumInfo
                                .Fields
                                .Where(f => f.HasValue && ((EnumMemberDeclarationSyntax)f.Symbol.GetSyntax(context.CancellationToken)).EqualsValue != null)
                                .Select(f => f.Value)
                                .ToImmutableArray();

                            Optional<ulong> optional = FlagsUtility<ulong>.Instance.GetUniquePowerOfTwo(values);

                            if (!optional.HasValue
                                || !ConvertHelpers.CanConvertFromUInt64(optional.Value, enumSymbol.EnumUnderlyingType.SpecialType))
                            {
                                return;
                            }

                            bool isFlags = enumSymbol.HasAttribute(MetadataNames.System_FlagsAttribute);

                            CodeAction codeAction = CodeAction.Create(
                                "Declare explicit values",
                                ct => DeclareExplicitValueAsync(document, enumDeclaration, enumSymbol, isFlags, useBitShift: false, values, semanticModel, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);

                            if (isFlags)
                            {
                                CodeAction codeAction2 = CodeAction.Create(
                                    "Declare explicit values (and use '<<' operator)",
                                    ct => DeclareExplicitValueAsync(document, enumDeclaration, enumSymbol, isFlags, useBitShift: true, values, semanticModel, ct),
                                    GetEquivalenceKey(diagnostic, "BitShift"));

                                context.RegisterCodeFix(codeAction2, diagnostic);
                            }

                            break;
                        }
                    case DiagnosticIdentifiers.UseBitShiftOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use '<<' operator",
                                ct => UseBitShiftOperatorAsync(document, enumDeclaration, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static async Task<Document> SortEnumMembersAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            SpecialType enumSpecialType = semanticModel.GetDeclaredSymbol(enumDeclaration).EnumUnderlyingType.SpecialType;

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> newMembers = members
                .OrderBy(f => GetConstantValue(f, semanticModel, cancellationToken), EnumValueComparer.GetInstance(enumSpecialType))
                .ToSeparatedSyntaxList();

            if (AreSeparatedWithEmptyLine(members))
            {
                for (int i = 0; i < newMembers.Count; i++)
                {
                    newMembers = newMembers.ReplaceAt(i, newMembers[i].TrimLeadingTrivia());
                }

                for (int i = 0; i < newMembers.Count - 1; i++)
                {
                    SyntaxToken separator = newMembers.GetSeparator(i);

                    newMembers = newMembers.ReplaceSeparator(
                        separator,
                        separator.TrimTrailingTrivia().AppendToTrailingTrivia(new SyntaxTrivia[] { NewLine(), NewLine() }));
                }
            }

            if (newMembers.SeparatorCount == members.SeparatorCount - 1)
            {
                SyntaxNodeOrTokenList newMembersWithSeparators = newMembers.GetWithSeparators();

                newMembersWithSeparators = newMembersWithSeparators.Add(CommaToken());

                newMembers = newMembersWithSeparators.ToSeparatedSyntaxList<EnumMemberDeclarationSyntax>();
            }

            MemberDeclarationSyntax newNode = enumDeclaration
                .WithMembers(newMembers)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static bool AreSeparatedWithEmptyLine(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            int count = members.Count;

            if (members.SeparatorCount < count - 1)
                return false;

            for (int i = 1; i < count; i++)
            {
                if (!members[i].GetLeadingTrivia().Any(SyntaxKind.EndOfLineTrivia))
                    return false;
            }

            for (int i = 0; i < count - 1; i++)
            {
                if (!members.GetSeparator(i).TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia))
                    return false;
            }

            return true;
        }

        private static object GetConstantValue(
            EnumMemberDeclarationSyntax enumMemberDeclaration,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return semanticModel.GetDeclaredSymbol(enumMemberDeclaration, cancellationToken)?.ConstantValue;
        }

        private static async Task<Document> DeclareExplicitValueAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            INamedTypeSymbol enumSymbol,
            bool isFlags,
            bool useBitShift,
            ImmutableArray<ulong> values,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            List<ulong> reservedValues = values.ToList();

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> newMembers = members;

            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].EqualsValue == null)
                {
                    IFieldSymbol fieldSymbol = semanticModel.GetDeclaredSymbol(members[i], cancellationToken);

                    ulong? value = null;

                    if (isFlags)
                    {
                        Optional<ulong> optional = FlagsUtility<ulong>.Instance.GetUniquePowerOfTwo(reservedValues);

                        if (optional.HasValue
                            && ConvertHelpers.CanConvertFromUInt64(optional.Value, enumSymbol.EnumUnderlyingType.SpecialType))
                        {
                            value = optional.Value;
                        }
                    }
                    else
                    {
                        value = SymbolUtility.GetEnumValueAsUInt64(fieldSymbol.ConstantValue, enumSymbol);
                    }

                    if (value != null)
                    {
                        reservedValues.Add(value.Value);

                        ExpressionSyntax expression;

                        if (useBitShift
                            && value.Value > 1)
                        {
                            var power = (int)Math.Log(Convert.ToDouble(value.Value), 2);

                            expression = LeftShiftExpression(NumericLiteralExpression(1), NumericLiteralExpression(power));
                        }
                        else
                        {
                            expression = NumericLiteralExpression(value.Value, enumSymbol.EnumUnderlyingType.SpecialType);
                        }

                        EqualsValueClauseSyntax equalsValue = EqualsValueClause(expression);

                        EnumMemberDeclarationSyntax newMember = members[i].WithEqualsValue(equalsValue);

                        newMembers = newMembers.ReplaceAt(i, newMember);
                    }
                }
            }

            EnumDeclarationSyntax newEnumDeclaration = enumDeclaration.WithMembers(newMembers);

            return await document.ReplaceNodeAsync(enumDeclaration, newEnumDeclaration, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> UseBitShiftOperatorAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            EnumDeclarationSyntax newEnumDeclaration = enumDeclaration.ReplaceNodes(
                GetExpressionsToRewrite(),
                (expression, _) =>
                {
                    Optional<object> constantValue = semanticModel.GetConstantValue(expression, cancellationToken);

                    var power = (int)Math.Log(Convert.ToDouble(constantValue.Value), 2);

                    BinaryExpressionSyntax leftShift = LeftShiftExpression(NumericLiteralExpression(1), NumericLiteralExpression(power));

                    return leftShift.WithTriviaFrom(expression);
                });

            return await document.ReplaceNodeAsync(enumDeclaration, newEnumDeclaration, cancellationToken).ConfigureAwait(false);

            IEnumerable<ExpressionSyntax> GetExpressionsToRewrite()
            {
                foreach (EnumMemberDeclarationSyntax member in enumDeclaration.Members)
                {
                    ExpressionSyntax expression = member.EqualsValue?.Value.WalkDownParentheses();

                    if (expression != null
                        && semanticModel.GetDeclaredSymbol(member, cancellationToken) is IFieldSymbol fieldSymbol
                        && fieldSymbol.HasConstantValue)
                    {
                        EnumFieldSymbolInfo fieldInfo = EnumFieldSymbolInfo.Create(fieldSymbol);

                        if (fieldInfo.Value > 1
                            && !fieldInfo.HasCompositeValue())
                        {
                            yield return expression;
                        }
                    }
                }
            }
        }
    }
}
