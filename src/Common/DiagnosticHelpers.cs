// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator
{
    internal static class DiagnosticHelpers
    {
        #region SymbolAnalysisContext
        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxToken token,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: token.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxTrivia trivia,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: trivia.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            AnalyzerOptionDescriptor obsoleteAnalyzerOption,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    messageArgs: messageArgs));

            ReportObsolete(context, location, obsoleteAnalyzerOption);
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    additionalLocations: additionalLocations,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    properties: properties,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    additionalLocations: additionalLocations,
                    properties: properties,
                    messageArgs: messageArgs));
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, Diagnostic diagnostic)
        {
            VerifyDiagnostic(diagnostic);

            context.ReportDiagnostic(diagnostic);
        }
        #endregion SymbolAnalysisContext

        #region SyntaxNodeAnalysisContext
        public static void ReportDiagnosticIfEffective(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            if (descriptor.IsEffective(context))
                ReportDiagnostic(context, descriptor, node, messageArgs);
        }

        public static void ReportDiagnosticIfEffective(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            if (descriptor.IsEffective(context))
                ReportDiagnostic(context, descriptor, location, messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            AnalyzerOptionDescriptor obsoleteAnalyzerOption,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);

            ReportObsolete(context, node.GetLocation(), obsoleteAnalyzerOption);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxToken token,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: token.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxTrivia trivia,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: trivia.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            AnalyzerOptionDescriptor obsoleteAnalyzerOption,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    messageArgs: messageArgs));

            ReportObsolete(context, location, obsoleteAnalyzerOption);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    additionalLocations: additionalLocations,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    properties: properties,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            AnalyzerOptionDescriptor obsoleteAnalyzerOption,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    properties: properties,
                    messageArgs: messageArgs));

            ReportObsolete(context, location, obsoleteAnalyzerOption);
        }

        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    additionalLocations: additionalLocations,
                    properties: properties,
                    messageArgs: messageArgs));
        }

        public static void ReportObsolete(
            SyntaxNodeAnalysisContext context,
            SyntaxNode node,
            AnalyzerOptionDescriptor analyzerOption)
        {
            ReportObsolete(context, node.GetLocation(), analyzerOption);
        }

        public static void ReportObsolete(
            SyntaxNodeAnalysisContext context,
            Location location,
            AnalyzerOptionDescriptor analyzerOption)
        {
            if (analyzerOption.Descriptor != null
                && context.Compilation.Options
                    .SpecificDiagnosticOptions
                    .TryGetValue(analyzerOption.Descriptor.Id, out ReportDiagnostic reportDiagnostic)
                && reportDiagnostic != Microsoft.CodeAnalysis.ReportDiagnostic.Suppress)
            {
                ReportDiagnostic(context, CreateObsoleteDiagnostic(analyzerOption, location));
            }
        }

        private static void ReportObsolete(
            SymbolAnalysisContext context,
            Location location,
            AnalyzerOptionDescriptor analyzerOption)
        {
            if (analyzerOption.Descriptor != null
                && context.Compilation.Options
                    .SpecificDiagnosticOptions
                    .TryGetValue(analyzerOption.Descriptor.Id, out ReportDiagnostic reportDiagnostic)
                && reportDiagnostic != Microsoft.CodeAnalysis.ReportDiagnostic.Suppress)
            {
                ReportDiagnostic(context, CreateObsoleteDiagnostic(analyzerOption, location));
            }
        }

        private static Diagnostic CreateObsoleteDiagnostic(AnalyzerOptionDescriptor analyzerOption, Location location)
        {
            return Diagnostic.Create(
                descriptor: CommonDiagnosticRules.AnalyzerIsObsolete,
                location: location,
                $"option '{analyzerOption.Descriptor.Id}'",
                $" Use EditorConfig option '{analyzerOption.OptionKey} = true' instead.");
        }

        public static void ReportToken(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, SyntaxToken token, params object[] messageArgs)
        {
            if (!token.IsMissing)
                ReportDiagnostic(context, descriptor, token, messageArgs);
        }

        public static void ReportNode(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, SyntaxNode node, params object[] messageArgs)
        {
            if (!node.IsMissing)
                ReportDiagnostic(context, descriptor, node, messageArgs);
        }

        public static void ReportDiagnostic(SyntaxNodeAnalysisContext context, Diagnostic diagnostic)
        {
            VerifyDiagnostic(diagnostic);

            context.ReportDiagnostic(diagnostic);
        }
        #endregion SyntaxNodeAnalysisContext

        #region SyntaxTreeAnalysisContext
        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxToken token,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: token.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxTrivia trivia,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: trivia.GetLocation(),
                messageArgs: messageArgs);
        }

        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    additionalLocations: additionalLocations,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    properties: properties,
                    messageArgs: messageArgs));
        }

        public static void ReportDiagnostic(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            ReportDiagnostic(
                context,
                Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    additionalLocations: additionalLocations,
                    properties: properties,
                    messageArgs: messageArgs));
        }

        private static void ReportDiagnostic(SyntaxTreeAnalysisContext context, Diagnostic diagnostic)
        {
            VerifyDiagnostic(diagnostic);

            context.ReportDiagnostic(diagnostic);
        }
        #endregion SyntaxTreeAnalysisContext

        [Conditional("DEBUG")]
        private static void VerifyDiagnostic(Diagnostic diagnostic)
        {
            Debug.Assert(Regex.IsMatch(diagnostic.Id, @"\ARCS[0-9]{4}(FadeOut)?\z"), $"Invalid diagnostic id '{diagnostic.Id}'.");
        }

        internal static bool IsAnyEffective(SyntaxNodeAnalysisContext context, ImmutableArray<DiagnosticDescriptor> descriptors)
        {
            return IsAnyEffective(context.Compilation, descriptors);
        }

        internal static bool IsAnyEffective(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            return IsAnyEffective(context.Compilation, descriptor1, descriptor2);
        }

        internal static bool IsAnyEffective(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2, DiagnosticDescriptor descriptor3)
        {
            return IsAnyEffective(context.Compilation, descriptor1, descriptor2, descriptor3);
        }

        internal static bool IsAnyEffective(Compilation compilation, ImmutableArray<DiagnosticDescriptor> descriptors)
        {
            foreach (DiagnosticDescriptor descriptor in descriptors)
            {
                if (descriptor.IsEffective(compilation))
                    return true;
            }

            return false;
        }

        internal static bool IsAnyEffective(Compilation compilation, DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            return descriptor1.IsEffective(compilation)
                || descriptor2.IsEffective(compilation);
        }

        internal static bool IsAnyEffective(Compilation compilation, DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2, DiagnosticDescriptor descriptor3)
        {
            return descriptor1.IsEffective(compilation)
                || descriptor2.IsEffective(compilation)
                || descriptor3.IsEffective(compilation);
        }
    }
}
