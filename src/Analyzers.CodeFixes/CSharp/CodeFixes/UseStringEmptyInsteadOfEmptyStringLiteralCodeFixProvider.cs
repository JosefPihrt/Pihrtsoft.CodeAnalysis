using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CSharp.CodeFixes {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseStringEmptyInsteadOfEmptyStringLiteralCodeFixProvider))]
    [Shared]
    public class UseStringEmptyInsteadOfEmptyStringLiteralCodeFixProvider : BaseCodeFixProvider {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIdentifiers.UseStringEmptyInsteadOfEmptyStringLiteral);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(
                SyntaxKind.StringLiteralExpression)))
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics[0];

            var codeAction = CodeAction.Create(
                "Use StringюEmpty instead of \"\"",
                cancellationToken => UseStringEmptyInsteadOfEmptyStringLiteralRefactoring.RefactorAsync(context.Document, (LiteralExpressionSyntax)node, cancellationToken));
            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
