// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.SyntaxRewriters;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MemberDeclarationCodeFixProvider))]
    [Shared]
    public sealed class MemberDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.MissingXmlCommentForPubliclyVisibleTypeOrMember,
                    CompilerDiagnosticIdentifiers.MethodReturnTypeMustMatchOverriddenMethodReturnType,
                    CompilerDiagnosticIdentifiers.MemberTypeMustMatchOverriddenMemberType,
                    CompilerDiagnosticIdentifiers.MissingPartialModifier,
                    CompilerDiagnosticIdentifiers.PartialMethodMustBeDeclaredInPartialClassOrPartialStruct,
                    CompilerDiagnosticIdentifiers.MemberIsAbstractButItIsContainedInNonAbstractClass,
                    CompilerDiagnosticIdentifiers.StaticConstructorMustBeParameterless,
                    CompilerDiagnosticIdentifiers.PartialMethodsMustHaveVoidReturnType,
                    CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationCanOnlyBeDeclaredInClassOrStruct,
                    CompilerDiagnosticIdentifiers.InterfacesCannotContainFields,
                    CompilerDiagnosticIdentifiers.InterfacesCannotContainOperators,
                    CompilerDiagnosticIdentifiers.InterfacesCannotDeclareTypes,
                    CompilerDiagnosticIdentifiers.OnlyClassTypesCanContainDestructors,
                    CompilerDiagnosticIdentifiers.StructsCannotContainExplicitParameterlessConstructors,
                    CompilerDiagnosticIdentifiers.NameOfDestructorMustMatchNameOfClass,
                    CompilerDiagnosticIdentifiers.CannotChangeTupleElementNameWhenOverridingInheritedMember,
                    CompilerDiagnosticIdentifiers.MethodsWithVariableArgumentsAreNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.ArgumentTypeIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.ReturnTypeIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.TypeOfVariableIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.IdentifierDifferingOnlyInCaseIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.OverloadedMethodDifferingOnlyInRefOrOutOrInArrayRankIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.OverloadedMethodDifferingOnlyByUnnamedArrayTypesIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.IdentifierIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.BaseTypeIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.ArraysAsAttributeArgumentsIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.ConstraintTypeIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.TypeIsNotCLSCompliantBecauseBaseInterfaceIsNotCLSCompliant,
                    CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationIsNotMemberOfInterface);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out MemberDeclarationSyntax memberDeclaration))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.MissingXmlCommentForPubliclyVisibleTypeOrMember:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddDocumentationComment))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Add documentation comment",
                                cancellationToken => AddDocumentationCommentRefactoring.RefactorAsync(context.Document, memberDeclaration, false, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);

                            CodeAction codeAction2 = CodeAction.Create(
                                "Add documentation comment (copy from base if available)",
                                cancellationToken => AddDocumentationCommentRefactoring.RefactorAsync(context.Document, memberDeclaration, true, cancellationToken),
                                GetEquivalenceKey(diagnostic, "CopyFromBaseIfAvailable"));

                            context.RegisterCodeFix(codeAction2, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MethodReturnTypeMustMatchOverriddenMethodReturnType:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeMethodReturnType))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            var methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(memberDeclaration, context.CancellationToken);

                            ITypeSymbol typeSymbol = methodSymbol.OverriddenMethod.ReturnType;

                            CodeFixRegistrator.ChangeTypeOrReturnType(context, diagnostic, memberDeclaration, typeSymbol, semanticModel);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.PartialMethodsMustHaveVoidReturnType:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeMethodReturnType))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            var methodDeclaration = (MethodDeclarationSyntax)memberDeclaration;

                            MethodDeclarationSyntax otherPart = semanticModel.GetOtherPart(methodDeclaration, context.CancellationToken);

                            if (otherPart == null)
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Change return type to 'void'",
                                cancellationToken =>
                                {
                                    return context.Document.Solution().ReplaceNodesAsync(
                                        new MethodDeclarationSyntax[] { methodDeclaration, otherPart },
                                        (node, _) => node.WithReturnType(CSharpFactory.VoidType().WithTriviaFrom(node.ReturnType)),
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberTypeMustMatchOverriddenMemberType:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MemberTypeMustMatchOverriddenMemberType))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ITypeSymbol typeSymbol = null;

                            switch (memberDeclaration.Kind())
                            {
                                case SyntaxKind.PropertyDeclaration:
                                case SyntaxKind.IndexerDeclaration:
                                    {
                                        var propertySymbol = (IPropertySymbol)semanticModel.GetDeclaredSymbol(memberDeclaration, context.CancellationToken);

                                        typeSymbol = propertySymbol.OverriddenProperty.Type;
                                        break;
                                    }
                                case SyntaxKind.EventDeclaration:
                                    {
                                        var eventSymbol = (IEventSymbol)semanticModel.GetDeclaredSymbol(memberDeclaration, context.CancellationToken);

                                        typeSymbol = eventSymbol.OverriddenEvent.Type;
                                        break;
                                    }
                                case SyntaxKind.EventFieldDeclaration:
                                    {
                                        VariableDeclaratorSyntax declarator = ((EventFieldDeclarationSyntax)memberDeclaration).Declaration.Variables[0];

                                        var eventSymbol = (IEventSymbol)semanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);

                                        typeSymbol = eventSymbol.OverriddenEvent.Type;
                                        break;
                                    }
                            }

                            CodeFixRegistrator.ChangeTypeOrReturnType(context, diagnostic, memberDeclaration, typeSymbol, semanticModel);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MissingPartialModifier:
                    case CompilerDiagnosticIdentifiers.PartialMethodMustBeDeclaredInPartialClassOrPartialStruct:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddPartialModifier))
                                break;

                            SyntaxNode node = null;

                            switch (memberDeclaration.Kind())
                            {
                                case SyntaxKind.MethodDeclaration:
                                    {
                                        if (memberDeclaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                                            node = memberDeclaration.Parent;

                                        break;
                                    }
                                case SyntaxKind.ClassDeclaration:
                                case SyntaxKind.StructDeclaration:
                                case SyntaxKind.InterfaceDeclaration:
                                case SyntaxKind.RecordDeclaration:
                                    {
                                        node = memberDeclaration;
                                        break;
                                    }
                            }

                            Debug.Assert(node != null, memberDeclaration.ToString());

                            if (node == null)
                                break;

                            ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, node, SyntaxKind.PartialKeyword, title: $"Make {CSharpFacts.GetTitle(node)} partial");
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberIsAbstractButItIsContainedInNonAbstractClass:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MakeContainingClassAbstract))
                                break;

                            if (!memberDeclaration.IsParentKind(SyntaxKind.ClassDeclaration))
                                break;

                            ModifiersCodeFixRegistrator.AddModifier(
                                context,
                                diagnostic,
                                memberDeclaration.Parent,
                                SyntaxKind.AbstractKeyword,
                                title: "Make containing class abstract");

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.StaticConstructorMustBeParameterless:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveParametersFromStaticConstructor))
                                break;

                            var constructorDeclaration = (ConstructorDeclarationSyntax)memberDeclaration;

                            CodeAction codeAction = CodeAction.Create(
                                "Remove parameters",
                                cancellationToken =>
                                {
                                    ParameterListSyntax parameterList = constructorDeclaration.ParameterList;

                                    ParameterListSyntax newParameterList = parameterList
                                        .WithParameters(default(SeparatedSyntaxList<ParameterSyntax>))
                                        .WithOpenParenToken(parameterList.OpenParenToken.WithoutTrailingTrivia())
                                        .WithCloseParenToken(parameterList.CloseParenToken.WithoutLeadingTrivia());

                                    ConstructorDeclarationSyntax newNode = constructorDeclaration.WithParameterList(newParameterList);

                                    return context.Document.ReplaceNodeAsync(constructorDeclaration, newNode, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationCanOnlyBeDeclaredInClassOrStruct:
                    case CompilerDiagnosticIdentifiers.InterfacesCannotContainFields:
                    case CompilerDiagnosticIdentifiers.InterfacesCannotContainOperators:
                    case CompilerDiagnosticIdentifiers.InterfacesCannotDeclareTypes:
                    case CompilerDiagnosticIdentifiers.OnlyClassTypesCanContainDestructors:
                    case CompilerDiagnosticIdentifiers.StructsCannotContainExplicitParameterlessConstructors:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveMemberDeclaration))
                                break;

                            CodeFixRegistrator.RemoveMemberDeclaration(context, diagnostic, memberDeclaration);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NameOfDestructorMustMatchNameOfClass:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RenameDestructorToMatchClassName))
                                break;

                            if (!(memberDeclaration is DestructorDeclarationSyntax destructorDeclaration))
                                break;

                            if (!(memberDeclaration.Parent is ClassDeclarationSyntax classDeclaration))
                                break;

                            if (classDeclaration.Identifier.ValueText.Length == 0)
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Rename destructor to match class name",
                                cancellationToken =>
                                {
                                    DestructorDeclarationSyntax newNode = destructorDeclaration.WithIdentifier(classDeclaration.Identifier.WithTriviaFrom(destructorDeclaration.Identifier));

                                    return context.Document.ReplaceNodeAsync(destructorDeclaration, newNode, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.CannotChangeTupleElementNameWhenOverridingInheritedMember:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RenameTupleElement))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
                            {
                                IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                                if (!(methodSymbol.ReturnType is INamedTypeSymbol tupleType))
                                    break;

                                if (!tupleType.IsTupleType)
                                    break;

                                if (!(methodSymbol.OverriddenMethod?.ReturnType is INamedTypeSymbol baseTupleType))
                                    break;

                                if (!baseTupleType.IsTupleType)
                                    break;

                                ImmutableArray<IFieldSymbol> elements = tupleType.TupleElements;
                                ImmutableArray<IFieldSymbol> baseElements = baseTupleType.TupleElements;

                                if (elements.Length != baseElements.Length)
                                    break;

                                int i = 0;
                                while (i < elements.Length)
                                {
                                    if (elements[i].Name != baseElements[i].Name)
                                        break;

                                    i++;
                                }

                                if (i == elements.Length)
                                    break;

                                TupleElementSyntax tupleElement = ((TupleTypeSyntax)methodDeclaration.ReturnType).Elements[i];

                                CodeAction codeAction = CodeAction.Create(
                                    $"Rename '{elements[i].Name}' to '{baseElements[i].Name}'",
                                    ct => RenameTupleElementAsync(context.Document, methodDeclaration, tupleElement, elements[i], baseElements[i].Name, semanticModel, ct),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else if (memberDeclaration is PropertyDeclarationSyntax propertyDeclaration)
                            {
                                IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);

                                if (!(propertySymbol.Type is INamedTypeSymbol tupleType))
                                    break;

                                if (!tupleType.IsTupleType)
                                    break;

                                if (!(propertySymbol.OverriddenProperty?.Type is INamedTypeSymbol baseTupleType))
                                    break;

                                if (!baseTupleType.IsTupleType)
                                    break;

                                ImmutableArray<IFieldSymbol> elements = tupleType.TupleElements;
                                ImmutableArray<IFieldSymbol> baseElements = baseTupleType.TupleElements;

                                if (elements.Length != baseElements.Length)
                                    break;

                                int i = 0;
                                while (i < elements.Length)
                                {
                                    if (elements[i].Name != baseElements[i].Name)
                                        break;

                                    i++;
                                }

                                if (i == elements.Length)
                                    break;

                                TupleElementSyntax tupleElement = ((TupleTypeSyntax)propertyDeclaration.Type).Elements[i];

                                CodeAction codeAction = CodeAction.Create(
                                    $"Rename '{elements[i].Name}' to '{baseElements[i].Name}'",
                                    ct => RenameTupleElementAsync(context.Document, propertyDeclaration, tupleElement, elements[i], baseElements[i].Name, semanticModel, ct),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MethodsWithVariableArgumentsAreNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.ArgumentTypeIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.ReturnTypeIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.TypeOfVariableIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.IdentifierDifferingOnlyInCaseIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.OverloadedMethodDifferingOnlyInRefOrOutOrInArrayRankIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.OverloadedMethodDifferingOnlyByUnnamedArrayTypesIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.IdentifierIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.BaseTypeIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.ArraysAsAttributeArgumentsIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.ConstraintTypeIsNotCLSCompliant:
                    case CompilerDiagnosticIdentifiers.TypeIsNotCLSCompliantBecauseBaseInterfaceIsNotCLSCompliant:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MarkDeclarationAsNonCLSCompliant))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                $"Mark {CSharpFacts.GetTitle(memberDeclaration)} as non-CLS-compliant",
                                ct => MarkDeclarationAsNonCLSCompliantAsync(context.Document, memberDeclaration, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationIsNotMemberOfInterface:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddParameterToExplicitlyImplementedInterfaceMember))
                                break;

                            var context2 = new CommonFixContext(
                                context.Document,
                                GetEquivalenceKey(diagnostic),
                                await context.GetSemanticModelAsync().ConfigureAwait(false),
                                context.CancellationToken);

                            CodeAction codeAction = AddParameterToInterfaceMemberRefactoring.ComputeRefactoringForExplicitImplementation(context2, memberDeclaration);

                            if (codeAction != null)
                                context.RegisterCodeFix(codeAction, diagnostic);

                            break;
                        }
                }
            }
        }

        private static Task<Document> RenameTupleElementAsync(
            Document document,
            MethodDeclarationSyntax methodDeclaration,
            TupleElementSyntax tupleElement,
            IFieldSymbol fieldSymbol,
            string newName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var returnType = (TupleTypeSyntax)methodDeclaration.ReturnType;

            SyntaxToken newIdentifier = Identifier(newName).WithTriviaFrom(tupleElement.Identifier);

            SeparatedSyntaxList<TupleElementSyntax> newElements = returnType.Elements.Replace(tupleElement, tupleElement.WithIdentifier(newIdentifier));

            MethodDeclarationSyntax newNode = methodDeclaration.WithReturnType(returnType.WithElements(newElements));

            var rewriter = new RenameRewriter(fieldSymbol, newName, semanticModel, cancellationToken);

            if (methodDeclaration.Body is BlockSyntax body)
            {
                newNode = newNode.WithBody((BlockSyntax)rewriter.VisitBlock(body));
            }
            else if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
            {
                newNode = newNode.WithExpressionBody((ArrowExpressionClauseSyntax)rewriter.VisitArrowExpressionClause(expressionBody));
            }

            return document.ReplaceNodeAsync(methodDeclaration, newNode, cancellationToken);
        }

        private static Task<Document> RenameTupleElementAsync(
            Document document,
            PropertyDeclarationSyntax propertyDeclaration,
            TupleElementSyntax tupleElement,
            IFieldSymbol fieldSymbol,
            string newName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var type = (TupleTypeSyntax)propertyDeclaration.Type;

            SyntaxToken newIdentifier = Identifier(newName).WithTriviaFrom(tupleElement.Identifier);

            SeparatedSyntaxList<TupleElementSyntax> newElements = type.Elements.Replace(tupleElement, tupleElement.WithIdentifier(newIdentifier));
            PropertyDeclarationSyntax newNode = propertyDeclaration.WithType(type.WithElements(newElements));

            var rewriter = new RenameRewriter(fieldSymbol, newName, semanticModel, cancellationToken);

            var newAccessorList = (AccessorListSyntax)rewriter.VisitAccessorList(propertyDeclaration.AccessorList);

            newNode = newNode.WithAccessorList(newAccessorList);

            return document.ReplaceNodeAsync(propertyDeclaration, newNode, cancellationToken);
        }

        private static Task<Document> MarkDeclarationAsNonCLSCompliantAsync(
            Document document,
            MemberDeclarationSyntax memberDeclaration,
            CancellationToken cancellationToken)
        {
            AttributeListSyntax attributeList = AttributeList(
                Attribute(
                    ParseName("global::System.CLSCompliantAttribute").WithSimplifierAnnotation(),
                    AttributeArgument(FalseLiteralExpression())))
                .WithFormatterAnnotation();

            MemberDeclarationSyntax newMemberDeclaration = SyntaxRefactorings.AddAttributeLists(memberDeclaration, keepDocumentationCommentOnTop: true, attributeList);

            return document.ReplaceNodeAsync(memberDeclaration, newMemberDeclaration, cancellationToken);
        }
    }
}
