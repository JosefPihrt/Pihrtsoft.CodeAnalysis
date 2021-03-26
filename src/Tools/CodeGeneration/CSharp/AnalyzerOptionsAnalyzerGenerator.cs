// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class AnalyzerOptionsAnalyzerGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<string> identifiers,
            string @namespace)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives("System.Collections.Immutable", "Microsoft.CodeAnalysis", "Microsoft.CodeAnalysis.Diagnostics"),
                NamespaceDeclaration(
                    @namespace,
                    ClassDeclaration(
                        SingletonList(
                            AttributeList(
                                Attribute(
                                    IdentifierName("DiagnosticAnalyzer"),
                                    AttributeArgument(
                                        SimpleMemberAccessExpression(
                                            IdentifierName("LanguageNames"),
                                            IdentifierName("CSharp")))))),
                        Modifiers.Internal(),
                        Identifier("AnalyzerOptionsAnalyzer"),
                        default,
                        BaseList(SimpleBaseType(IdentifierName("DiagnosticAnalyzer"))),
                        default,
                        List(CreateMembers(identifiers)))));

            return compilationUnit.NormalizeWhitespace();
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<string> identifiers)
        {
            yield return PropertyDeclaration(
                Modifiers.Public_Override(),
                ParseTypeName("ImmutableArray<DiagnosticDescriptor>"),
                Identifier("SupportedDiagnostics"),
                AccessorList(
                    GetAccessorDeclaration(
                        Block(
                            ReturnStatement(
                                (identifiers.Any())
                                    ? SimpleMemberInvocationExpression(
                                        IdentifierName("ImmutableArray"),
                                        IdentifierName("Create"),
                                        ArgumentList(identifiers.Select(identifier => Argument(SimpleMemberAccessExpression(IdentifierName("AnalyzerOptionDiagnosticDescriptors"), IdentifierName(identifier)).WithTrailingTrivia(NewLine()))).ToSeparatedSyntaxList()))
                                    : ParseExpression("ImmutableArray<DiagnosticDescriptor>.Empty"))))));

            yield return MethodDeclaration(
                Modifiers.Public_Override(),
                VoidType(),
                Identifier("Initialize"),
                ParameterList(Parameter(IdentifierName("AnalysisContext"), "context")),
                Block());
        }
    }
}
