// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddEmptyLineAfterRegionDirectiveAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddEmptyLineAfterRegionDirective);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeRegionDirectiveTrivia(f), SyntaxKind.RegionDirectiveTrivia);
        }

        private static void AnalyzeRegionDirectiveTrivia(SyntaxNodeAnalysisContext context)
        {
            var regionDirective = (RegionDirectiveTriviaSyntax)context.Node;

            if (IsFollowedWithEmptyLineOrEndRegionDirective())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.AddEmptyLineAfterRegionDirective,
                Location.Create(regionDirective.SyntaxTree, regionDirective.EndOfDirectiveToken.Span));

            bool IsFollowedWithEmptyLineOrEndRegionDirective()
            {
                SyntaxTrivia parentTrivia = regionDirective.ParentTrivia;

                SyntaxTriviaList.Enumerator en = parentTrivia.Token.LeadingTrivia.GetEnumerator();

                while (en.MoveNext())
                {
                    if (en.Current == parentTrivia)
                    {
                        if (!en.MoveNext())
                            return false;

                        if (en.Current.IsWhitespaceTrivia()
                            && !en.MoveNext())
                        {
                            return false;
                        }

                        if (en.Current.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                            return true;

                        return en.Current.IsEndOfLineTrivia();
                    }
                }

                return false;
            }
        }
    }
}
