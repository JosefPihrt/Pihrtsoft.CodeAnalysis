// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddNewLineBeforeEnumMemberAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineBeforeEnumMember); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeEnumDeclaration(f), SyntaxKind.EnumDeclaration);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

            if (members.Count <= 1)
                return;

            int previousIndex = members[0].GetSpanStartLine();

            for (int i = 1; i < members.Count; i++)
            {
                if (members[i].GetSpanStartLine() == previousIndex)
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticDescriptors.AddNewLineBeforeEnumMember,
                        Location.Create(enumDeclaration.SyntaxTree, members[i].Span.WithLength(0)));

                    return;
                }

                previousIndex = members[i].GetSpanEndLine();
            }
        }
    }
}
