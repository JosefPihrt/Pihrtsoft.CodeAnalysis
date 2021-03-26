// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class DiagnosticIdentifiersGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<AnalyzerMetadata> analyzers,
            bool obsolete,
            IComparer<string> comparer,
            string @namespace,
            string className)
        {
            return CompilationUnit(
                UsingDirectives("System"),
                NamespaceDeclaration(
                    @namespace,
                    ClassDeclaration(
                        Modifiers.Public_Static_Partial(),
                        className,
                        analyzers
                            .Where(f => f.IsObsolete == obsolete)
                            .OrderBy(f => f.Id, comparer)
                            .SelectMany(f => CreateMembers(f))
                            .ToSyntaxList<MemberDeclarationSyntax>())));
        }

        private static IEnumerable<FieldDeclarationSyntax> CreateMembers(AnalyzerMetadata analyzer)
        {
            string id = analyzer.Id;
            string identifier = analyzer.Identifier;

            if (id != null)
                yield return CreateMember(id, identifier, analyzer.IsObsolete);
        }

        private static FieldDeclarationSyntax CreateMember(string id, string identifier, bool isObsolete)
        {
            FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                Modifiers.Public_Const(),
                PredefinedStringType(),
                identifier,
                StringLiteralExpression(id));

            if (isObsolete)
                fieldDeclaration = fieldDeclaration.AddObsoleteAttributeIf(isObsolete, error: true);

            return fieldDeclaration;
        }
    }
}
