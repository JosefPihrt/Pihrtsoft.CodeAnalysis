// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeParameterConstraintCodeFixProvider))]
    [Shared]
    public sealed class TypeParameterConstraintCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.NewConstraintMustBeLastConstraintSpecified,
                    CompilerDiagnosticIdentifiers.DuplicateConstraintForTypeParameter,
                    CompilerDiagnosticIdentifiers.ClassOrStructConstraintMustComeBeforeAnyOtherConstraints,
                    CompilerDiagnosticIdentifiers.CannotSpecifyBothConstraintClassAndClassOrStructConstraint,
                    CompilerDiagnosticIdentifiers.NewConstraintCannotBeUsedWithStructConstraint);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeParameterConstraintSyntax constraint))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.NewConstraintMustBeLastConstraintSpecified:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MoveConstraint))
                                break;

                            TypeParameterConstraintInfo constraintInfo = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!constraintInfo.Success)
                                break;

                            MoveConstraint(context, diagnostic, constraintInfo, constraintInfo.Constraints.Count - 1);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.DuplicateConstraintForTypeParameter:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveConstraint))
                                RemoveConstraint(context, diagnostic, constraint);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ClassOrStructConstraintMustComeBeforeAnyOtherConstraints:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MoveConstraint))
                                break;

                            TypeParameterConstraintInfo constraintInfo = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!constraintInfo.Success)
                                break;

                            if (constraintInfo.IsDuplicateConstraint)
                            {
                                RemoveConstraint(context, diagnostic, constraint);
                            }
                            else
                            {
                                MoveConstraint(context, diagnostic, constraintInfo, 0);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.CannotSpecifyBothConstraintClassAndClassOrStructConstraint:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveConstraint))
                                break;

                            TypeParameterConstraintInfo constraintInfo = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!constraintInfo.Success)
                                break;

                            RemoveConstraint(context, diagnostic, constraint);

                            TypeParameterConstraintSyntax classConstraint = constraintInfo.Constraints.Find(SyntaxKind.ClassConstraint);

                            if (classConstraint != null)
                                RemoveConstraint(context, diagnostic, classConstraint);

                            TypeParameterConstraintSyntax structConstraint = constraintInfo.Constraints.Find(SyntaxKind.StructConstraint);

                            if (structConstraint != null)
                                RemoveConstraint(context, diagnostic, structConstraint);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NewConstraintCannotBeUsedWithStructConstraint:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveConstraint))
                                break;

                            RemoveConstraint(context, diagnostic, constraint);

                            TypeParameterConstraintInfo constraintInfo = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!constraintInfo.Success)
                                break;

                            TypeParameterConstraintSyntax structConstraint = constraintInfo.Constraints.Find(SyntaxKind.StructConstraint);

                            RemoveConstraint(context, diagnostic, structConstraint);
                            break;
                        }
                }
            }
        }

        private void MoveConstraint(
            CodeFixContext context,
            Diagnostic diagnostic,
            TypeParameterConstraintInfo constraintInfo,
            int index)
        {
            CodeAction codeAction = CodeAction.Create(
                $"Move constraint '{constraintInfo.Constraint}'",
                cancellationToken =>
                {
                    SeparatedSyntaxList<TypeParameterConstraintSyntax> newConstraints = constraintInfo.Constraints
                        .Remove(constraintInfo.Constraint)
                        .Insert(index, constraintInfo.Constraint);

                    TypeParameterConstraintClauseSyntax newNode = constraintInfo.ConstraintClause
                        .WithConstraints(newConstraints)
                        .WithFormatterAnnotation();

                    return context.Document.ReplaceNodeAsync(constraintInfo.ConstraintClause, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private void RemoveConstraint(
            CodeFixContext context,
            Diagnostic diagnostic,
            TypeParameterConstraintSyntax constraint)
        {
            CodeAction codeAction = CodeAction.Create(
                $"Remove constraint '{constraint}'",
                cancellationToken => context.Document.RemoveNodeAsync(constraint, cancellationToken),
                GetEquivalenceKey(diagnostic, constraint.Kind().ToString()));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
