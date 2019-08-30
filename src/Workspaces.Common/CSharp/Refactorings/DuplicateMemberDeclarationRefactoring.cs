// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Syntax;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class DuplicateMemberDeclarationRefactoring
    {
        public static Task<Document> RefactorAsync(
            Document document,
            MemberDeclarationSyntax member,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return document.ReplaceNodeAsync(member.Parent, Refactor(member), cancellationToken);
        }

        public static Task<Document> RefactorAsync(
            Document document,
            LocalFunctionStatementSyntax localFunction,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            StatementListInfo statementsInfos = SyntaxInfo.StatementListInfo(localFunction);

            SyntaxList<StatementSyntax> statements = statementsInfos.Statements;
            int index = statements.IndexOf(localFunction);

            if (index == 0
                && statementsInfos.ParentAsBlock.OpenBraceToken.GetFullSpanEndLine(cancellationToken) == localFunction.GetFullSpanStartLine(cancellationToken))
            {
                localFunction = localFunction.WithLeadingTrivia(localFunction.GetLeadingTrivia().Insert(0, NewLine()));
            }

            SyntaxList<StatementSyntax> newStatements = statements.Insert(index + 1, localFunction.WithNavigationAnnotation());

            return document.ReplaceStatementsAsync(statementsInfos, newStatements, cancellationToken);
        }

        private static SyntaxNode Refactor(MemberDeclarationSyntax member)
        {
            switch (member.Parent.Kind())
            {
                case SyntaxKind.CompilationUnit:
                    {
                        var parent = (CompilationUnitSyntax)member.Parent;
                        SyntaxList<MemberDeclarationSyntax> members = parent.Members;
                        int index = members.IndexOf(member);

                        return parent.WithMembers(members.Insert(index + 1, member.WithNavigationAnnotation()));
                    }
                case SyntaxKind.NamespaceDeclaration:
                    {
                        var parent = (NamespaceDeclarationSyntax)member.Parent;
                        SyntaxList<MemberDeclarationSyntax> members = parent.Members;
                        int index = members.IndexOf(member);

                        if (index == 0
                            && parent.OpenBraceToken.GetFullSpanEndLine() == member.GetFullSpanStartLine())
                        {
                            member = member.WithLeadingTrivia(member.GetLeadingTrivia().Insert(0, NewLine()));
                        }

                        return parent.WithMembers(members.Insert(index + 1, member.WithNavigationAnnotation()));
                    }
                case SyntaxKind.ClassDeclaration:
                    {
                        var parent = (ClassDeclarationSyntax)member.Parent;
                        SyntaxList<MemberDeclarationSyntax> members = parent.Members;
                        int index = members.IndexOf(member);

                        if (index == 0
                            && parent.OpenBraceToken.GetFullSpanEndLine() == member.GetFullSpanStartLine())
                        {
                            member = member.WithLeadingTrivia(member.GetLeadingTrivia().Insert(0, NewLine()));
                        }

                        return parent.WithMembers(members.Insert(index + 1, member.WithNavigationAnnotation()));
                    }
                case SyntaxKind.StructDeclaration:
                    {
                        var parent = (StructDeclarationSyntax)member.Parent;
                        SyntaxList<MemberDeclarationSyntax> members = parent.Members;
                        int index = members.IndexOf(member);

                        if (index == 0
                            && parent.OpenBraceToken.GetFullSpanEndLine() == member.GetFullSpanStartLine())
                        {
                            member = member.WithLeadingTrivia(member.GetLeadingTrivia().Insert(0, NewLine()));
                        }

                        return parent.WithMembers(members.Insert(index + 1, member.WithNavigationAnnotation()));
                    }
                case SyntaxKind.InterfaceDeclaration:
                    {
                        var parent = (InterfaceDeclarationSyntax)member.Parent;
                        SyntaxList<MemberDeclarationSyntax> members = parent.Members;
                        int index = members.IndexOf(member);

                        if (index == 0
                            && parent.OpenBraceToken.GetFullSpanEndLine() == member.GetFullSpanStartLine())
                        {
                            member = member.WithLeadingTrivia(member.GetLeadingTrivia().Insert(0, NewLine()));
                        }

                        return parent.WithMembers(members.Insert(index + 1, member.WithNavigationAnnotation()));
                    }
            }

            return null;
        }
    }
}
