// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassDeclarationCodeFixProvider))]
    [Shared]
    public sealed class ClassDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.MakeClassStatic,
                    DiagnosticIdentifiers.AddStaticModifierToAllPartialClassDeclarations,
                    DiagnosticIdentifiers.ImplementExceptionConstructors,
                    DiagnosticIdentifiers.UseAttributeUsageAttribute,
                    DiagnosticIdentifiers.MakeClassSealed);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ClassDeclarationSyntax classDeclaration))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.MakeClassStatic:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ISymbol symbol = semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);

                            ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

                            if (!syntaxReferences.Any())
                                break;

                            ModifiersCodeFixRegistrator.AddModifier(
                                context,
                                diagnostic,
                                ImmutableArray.CreateRange(syntaxReferences, f => f.GetSyntax()),
                                SyntaxKind.StaticKeyword,
                                title: "Make class static");

                            break;
                        }
                    case DiagnosticIdentifiers.AddStaticModifierToAllPartialClassDeclarations:
                        {
                            ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, classDeclaration, SyntaxKind.StaticKeyword);
                            break;
                        }
                    case DiagnosticIdentifiers.ImplementExceptionConstructors:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Generate exception constructors",
                                cancellationToken =>
                                {
                                    return ImplementExceptionConstructorsAsync(
                                        context.Document,
                                        classDeclaration,
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseAttributeUsageAttribute:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use AttributeUsageAttribute",
                                cancellationToken => UseAttributeUsageAttributeAsync(context.Document, classDeclaration, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.MakeClassSealed:
                        {
                            ModifiersCodeFixRegistrator.AddModifier(
                                context,
                                diagnostic,
                                classDeclaration,
                                SyntaxKind.SealedKeyword);

                            break;
                        }
                }
            }
        }

        private static Task<Document> UseAttributeUsageAttributeAsync(
            Document document,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken cancellationToken)
        {
            AttributeSyntax attribute = Attribute(
                (NameSyntax)ParseTypeName("System.AttributeUsageAttribute").WithSimplifierAnnotation(),
                AttributeArgumentList(
                    AttributeArgument(
                        SimpleMemberAccessExpression(
                            ParseTypeName("System.AttributeTargets").WithSimplifierAnnotation(),
                            IdentifierName(Identifier("All").WithRenameAnnotation()))),
                    AttributeArgument(
                        NameEquals(IdentifierName("AllowMultiple")),
                        FalseLiteralExpression())));

            ClassDeclarationSyntax newNode = classDeclaration.AddAttributeLists(AttributeList(attribute));

            return document.ReplaceNodeAsync(classDeclaration, newNode, cancellationToken);
        }

        private static async Task<Document> ImplementExceptionConstructorsAsync(
            Document document,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            List<IMethodSymbol> constructors = GenerateBaseConstructorsAnalysis.GetMissingBaseConstructors(classDeclaration, semanticModel, cancellationToken);

            for (int i = constructors.Count - 1; i >= 0; i--)
            {
                IMethodSymbol constructor = constructors[i];

                ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;

                if (parameters.Length == 2
                    && parameters[0].Type.HasMetadataName(MetadataNames.System_Runtime_Serialization_SerializationInfo)
                    && parameters[1].Type.HasMetadataName(MetadataNames.System_Runtime_Serialization_StreamingContext))
                {
                    constructors.RemoveAt(i);
                }
            }

            return await GenerateBaseConstructorsRefactoring.RefactorAsync(document, classDeclaration, constructors.ToArray(), semanticModel, cancellationToken).ConfigureAwait(false);
        }
    }
}
