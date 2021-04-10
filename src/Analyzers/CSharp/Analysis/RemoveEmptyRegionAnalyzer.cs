// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveEmptyRegionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.RemoveEmptyRegion,
                        DiagnosticRules.RemoveEmptyRegionFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveEmptyRegion.IsEffective(c))
                        AnalyzeRegionDirective(c);
                },
                SyntaxKind.RegionDirectiveTrivia);
        }

        private static void AnalyzeRegionDirective(SyntaxNodeAnalysisContext context)
        {
            var regionDirective = (RegionDirectiveTriviaSyntax)context.Node;

            RegionInfo region = SyntaxInfo.RegionInfo(regionDirective);

            if (!region.Success)
                return;

            if (!region.IsEmpty)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveEmptyRegion,
                regionDirective.GetLocation(),
                additionalLocations: ImmutableArray.Create(region.EndDirective.GetLocation()));

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyRegionFadeOut, regionDirective.GetLocation());
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyRegionFadeOut, region.EndDirective.GetLocation());
        }
    }
}
