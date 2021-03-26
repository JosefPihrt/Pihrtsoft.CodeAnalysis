// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Documentation;
using Roslynator.Documentation;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AddDocumentationCommentRefactoring
    {
        private static readonly string[] _tagSeparator = new[] { "," };

        public static async Task<Document> RefactorAsync(
            Document document,
            MemberDeclarationSyntax memberDeclaration,
            bool copyCommentFromBaseIfAvailable,
            CancellationToken cancellationToken = default)
        {
            MemberDeclarationSyntax newNode = null;

            if (copyCommentFromBaseIfAvailable
                && DocumentationCommentGenerator.CanGenerateFromBase(memberDeclaration.Kind()))
            {
                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                DocumentationCommentData data = DocumentationCommentGenerator.GenerateFromBase(memberDeclaration, semanticModel, cancellationToken);

                if (data.Success)
                {
                    SyntaxTrivia comment = DocumentationCommentTriviaFactory.Parse(data.RawXml, semanticModel, memberDeclaration.SpanStart);

                    newNode = memberDeclaration.WithDocumentationComment(comment, indent: true);
                }
            }

            DocumentationCommentGeneratorSettings settings = DocumentationCommentGeneratorSettings.Default;

            if (document.TryGetAnalyzerOptionValue(
                memberDeclaration,
                CodeFixOptions.CS1591_MissingXmlCommentForPubliclyVisibleTypeOrMember_IgnoredTags,
                out string value))
            {
                ImmutableArray<string> ignoredTags = value
                    .Split(_tagSeparator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim())
                    .Where(f => f.Length > 0)
                    .ToImmutableArray();

                settings = new DocumentationCommentGeneratorSettings(ignoredTags: ignoredTags);
            }

            newNode ??= memberDeclaration.WithNewSingleLineDocumentationComment(settings);

            return await document.ReplaceNodeAsync(memberDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
