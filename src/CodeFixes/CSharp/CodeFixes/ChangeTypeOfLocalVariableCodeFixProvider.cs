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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeTypeOfLocalVariableCodeFixProvider))]
    [Shared]
    public sealed class ChangeTypeOfLocalVariableCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.CannotAssignMethodGroupToImplicitlyTypedVariable,
                    CompilerDiagnosticIdentifiers.NoOverloadMatchesDelegate,
                    CompilerDiagnosticIdentifiers.MethodHasWrongReturnType);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeTypeOfLocalVariable))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.VariableDeclarator, SyntaxKind.AddAssignmentExpression, SyntaxKind.SubtractAssignmentExpression)))
                return;

            if (!(node is VariableDeclaratorSyntax variableDeclarator))
                return;

            if (!variableDeclarator.IsParentKind(SyntaxKind.VariableDeclaration))
                return;

            ExpressionSyntax value = variableDeclarator.Initializer?.Value;

            if (value == null)
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(value, context.CancellationToken);

            if (symbolInfo.Symbol != null)
            {
                ComputeCodeFix(context, diagnostic, variableDeclarator, symbolInfo.Symbol, semanticModel);
            }
            else
            {
                foreach (ISymbol candidateSymbol in symbolInfo.CandidateSymbols)
                    ComputeCodeFix(context, diagnostic, variableDeclarator, candidateSymbol, semanticModel);
            }
        }

        private void ComputeCodeFix(
            CodeFixContext context,
            Diagnostic diagnostic,
            VariableDeclaratorSyntax variableDeclarator,
            ISymbol symbol,
            SemanticModel semanticModel)
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

                if (parameters.Length <= 16)
                {
                    ITypeSymbol returnType = methodSymbol.ReturnType;

                    if (SupportsExplicitDeclaration(returnType, parameters))
                    {
                        INamedTypeSymbol typeSymbol = ConstructActionOrFunc(returnType, parameters, semanticModel);

                        var variableDeclaration = (VariableDeclarationSyntax)variableDeclarator.Parent;

                        CodeAction codeAction = CodeActionFactory.ChangeType(
                            context.Document,
                            variableDeclaration.Type,
                            typeSymbol,
                            semanticModel,
                            equivalenceKey: GetEquivalenceKey(diagnostic, SymbolDisplay.ToDisplayString(typeSymbol)));

                        context.RegisterCodeFix(codeAction, diagnostic);
                    }
                }
            }
        }

        private static bool SupportsExplicitDeclaration(ITypeSymbol returnType, ImmutableArray<IParameterSymbol> parameters)
        {
            if (!returnType.IsVoid()
                && !returnType.SupportsExplicitDeclaration())
            {
                return false;
            }

            foreach (IParameterSymbol parameter in parameters)
            {
                if (!parameter.Type.SupportsExplicitDeclaration())
                    return false;
            }

            return true;
        }

        private static INamedTypeSymbol ConstructActionOrFunc(
            ITypeSymbol returnType,
            ImmutableArray<IParameterSymbol> parameters,
            SemanticModel semanticModel)
        {
            int length = parameters.Length;

            if (returnType.IsVoid())
            {
                if (length == 0)
                    return semanticModel.GetTypeByMetadataName("System.Action");

                INamedTypeSymbol actionSymbol = semanticModel.GetTypeByMetadataName($"System.Action`{length.ToString()}");

                var typeArguments = new ITypeSymbol[length];

                for (int i = 0; i < length; i++)
                    typeArguments[i] = parameters[i].Type;

                return actionSymbol.Construct(typeArguments);
            }
            else
            {
                INamedTypeSymbol funcSymbol = semanticModel.GetTypeByMetadataName($"System.Func`{(length + 1).ToString()}");

                var typeArguments = new ITypeSymbol[length + 1];

                for (int i = 0; i < length; i++)
                    typeArguments[i] = parameters[i].Type;

                typeArguments[length] = returnType;

                return funcSymbol.Construct(typeArguments);
            }
        }
    }
}
