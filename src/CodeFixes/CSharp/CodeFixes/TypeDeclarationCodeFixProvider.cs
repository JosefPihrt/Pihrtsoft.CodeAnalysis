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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;
using static Roslynator.CSharp.CSharpTypeFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeDeclarationCodeFixProvider))]
    [Shared]
    public sealed class TypeDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.TypeDefinesEqualityOperatorButDoesNotOverrideObjectEquals,
                    CompilerDiagnosticIdentifiers.TypeDefinesEqualityOperatorButDoesNotOverrideObjectGetHashCode,
                    CompilerDiagnosticIdentifiers.TypeOverridesObjectEqualsButDoesNotOverrideObjectGetHashCode);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeDeclarationSyntax typeDeclaration))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.TypeDefinesEqualityOperatorButDoesNotOverrideObjectEquals:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.DefineObjectEquals))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken) as ITypeSymbol;

                            if (typeSymbol?.IsErrorType() != false)
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Override object.Equals",
                                cancellationToken =>
                                {
                                    TypeSyntax type = typeSymbol.ToMinimalTypeSyntax(semanticModel, typeDeclaration.Identifier.SpanStart);

                                    MethodDeclarationSyntax methodDeclaration = ObjectEqualsMethodDeclaration(type);

                                    TypeDeclarationSyntax newNode = MemberDeclarationInserter.Default.Insert(typeDeclaration, methodDeclaration);

                                    return context.Document.ReplaceNodeAsync(typeDeclaration, newNode, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.TypeDefinesEqualityOperatorButDoesNotOverrideObjectGetHashCode:
                    case CompilerDiagnosticIdentifiers.TypeOverridesObjectEqualsButDoesNotOverrideObjectGetHashCode:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.DefineObjectGetHashCode))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            MethodDeclarationSyntax methodDeclaration = ObjectGetHashCodeMethodDeclaration();

                            CodeAction codeAction = CodeAction.Create(
                                "Override object.GetHashCode",
                                cancellationToken =>
                                {
                                    TypeDeclarationSyntax newNode = MemberDeclarationInserter.Default.Insert(typeDeclaration, methodDeclaration);

                                    return context.Document.ReplaceNodeAsync(typeDeclaration, newNode, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static MethodDeclarationSyntax ObjectEqualsMethodDeclaration(
            TypeSyntax type,
            string parameterName = "obj",
            string localName = "other")
        {
            return MethodDeclaration(
                Modifiers.Public_Override(),
                BoolType(),
                Identifier("Equals"),
                ParameterList(Parameter(ObjectType(), parameterName)),
                Block(
                    IfStatement(
                        LogicalNotExpression(
                            IsPatternExpression(
                                IdentifierName(parameterName),
                                DeclarationPattern(
                                    type,
                                    SingleVariableDesignation(Identifier(localName))))
                                .Parenthesize()),
                        Block(ReturnStatement(FalseLiteralExpression()))),
                    ThrowNewStatement(NotImplementedException())));
        }

        private static MethodDeclarationSyntax ObjectGetHashCodeMethodDeclaration()
        {
            return MethodDeclaration(
                Modifiers.Public_Override(),
                IntType(),
                Identifier("GetHashCode"),
                ParameterList(),
                Block(ThrowNewStatement(NotImplementedException())));
        }
    }
}
