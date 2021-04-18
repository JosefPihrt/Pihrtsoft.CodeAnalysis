// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeParameterConstraintClauseCodeFixProvider))]
    [Shared]
    public sealed class TypeParameterConstraintClauseCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.ConstraintsAreNotAllowedOnNonGenericDeclarations,
                    CompilerDiagnosticIdentifiers.ConstraintClauseHasAlreadyBeenSpecified);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeParameterConstraintClauseSyntax constraintClause))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.ConstraintsAreNotAllowedOnNonGenericDeclarations:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveConstraintClauses))
                                break;

                            GenericInfo genericInfo = SyntaxInfo.GenericInfo(constraintClause);

                            if (!genericInfo.Success)
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Remove constraints",
                                cancellationToken =>
                                {
                                    GenericInfo newGenericInfo = genericInfo.RemoveAllConstraintClauses();

                                    return context.Document.ReplaceNodeAsync(genericInfo.Node, newGenericInfo.Node, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ConstraintClauseHasAlreadyBeenSpecified:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.CombineConstraintClauses))
                                break;

                            GenericInfo genericInfo = SyntaxInfo.GenericInfo(constraintClause);

                            int index = genericInfo.ConstraintClauses.IndexOf(constraintClause);

                            string name = constraintClause.Name.Identifier.ValueText;

                            TypeParameterConstraintClauseSyntax constraintClause2 = genericInfo.FindConstraintClause(name);

                            if (constraintClause2 == null)
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                $"Combine constraints for '{name}'",
                                cancellationToken =>
                                {
                                    TypeParameterConstraintClauseSyntax newConstraintClause = constraintClause2.WithConstraints(constraintClause2.Constraints.AddRange(constraintClause.Constraints));

                                    SyntaxList<TypeParameterConstraintClauseSyntax> newConstraintClauses = genericInfo.ConstraintClauses
                                        .Replace(constraintClause2, newConstraintClause)
                                        .RemoveAt(index);

                                    GenericInfo newGenericInfo = genericInfo.WithConstraintClauses(newConstraintClauses);

                                    return context.Document.ReplaceNodeAsync(genericInfo.Node, newGenericInfo.Node, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
