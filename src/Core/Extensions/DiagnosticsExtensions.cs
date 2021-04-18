// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator
{
    /// <summary>
    /// A set of extension methods for <see cref="SymbolAnalysisContext"/>, <see cref="SyntaxNodeAnalysisContext"/> and <see cref="SyntaxTreeAnalysisContext"/>.
    /// </summary>
    public static class DiagnosticsExtensions
    {
        #region SymbolAnalysisContext
        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="node"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="token"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
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

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="trivia"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
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

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="additionalLocations"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                additionalLocations: additionalLocations,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="properties"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                properties: properties,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="additionalLocations"></param>
        /// <param name="properties"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SymbolAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                additionalLocations: additionalLocations,
                properties: properties,
                messageArgs: messageArgs));
        }
        #endregion SymbolAnalysisContext

        #region SyntaxNodeAnalysisContext
        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="node"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="token"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
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

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="trivia"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
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

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="additionalLocations"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                additionalLocations: additionalLocations,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="properties"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                properties: properties,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="additionalLocations"></param>
        /// <param name="properties"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                additionalLocations: additionalLocations,
                properties: properties,
                messageArgs: messageArgs));
        }
        #endregion SyntaxNodeAnalysisContext

        #region SyntaxTreeAnalysisContext
        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="node"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            SyntaxNode node,
            params object[] messageArgs)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            ReportDiagnostic(
                context: context,
                descriptor: descriptor,
                location: node.GetLocation(),
                messageArgs: messageArgs);
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="token"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
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

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="trivia"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
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

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="additionalLocations"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                additionalLocations: additionalLocations,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="properties"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                properties: properties,
                messageArgs: messageArgs));
        }

        /// <summary>
        /// Report a <see cref="Diagnostic"/> about a <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="descriptor"></param>
        /// <param name="location"></param>
        /// <param name="additionalLocations"></param>
        /// <param name="properties"></param>
        /// <param name="messageArgs"></param>
        public static void ReportDiagnostic(
            this SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location location,
            IEnumerable<Location> additionalLocations,
            ImmutableDictionary<string, string> properties,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: descriptor,
                location: location,
                additionalLocations: additionalLocations,
                properties: properties,
                messageArgs: messageArgs));
        }
        #endregion SyntaxTreeAnalysisContext

        internal static ReportDiagnostic ToReportDiagnostic(this DiagnosticSeverity diagnosticSeverity)
        {
            switch (diagnosticSeverity)
            {
                case DiagnosticSeverity.Hidden:
                    return Microsoft.CodeAnalysis.ReportDiagnostic.Hidden;
                case DiagnosticSeverity.Info:
                    return Microsoft.CodeAnalysis.ReportDiagnostic.Info;
                case DiagnosticSeverity.Warning:
                    return Microsoft.CodeAnalysis.ReportDiagnostic.Warn;
                case DiagnosticSeverity.Error:
                    return Microsoft.CodeAnalysis.ReportDiagnostic.Error;
                default:
                    throw new ArgumentException($"Unknown value '{diagnosticSeverity}'.", nameof(diagnosticSeverity));
            }
        }

        internal static DiagnosticSeverity ToDiagnosticSeverity(this ReportDiagnostic reportDiagnostic)
        {
            switch (reportDiagnostic)
            {
                case Microsoft.CodeAnalysis.ReportDiagnostic.Error:
                    return DiagnosticSeverity.Error;
                case Microsoft.CodeAnalysis.ReportDiagnostic.Warn:
                    return DiagnosticSeverity.Warning;
                case Microsoft.CodeAnalysis.ReportDiagnostic.Info:
                    return DiagnosticSeverity.Info;
                case Microsoft.CodeAnalysis.ReportDiagnostic.Hidden:
                    return DiagnosticSeverity.Hidden;
                default:
                    throw new ArgumentException($"Unknown value '{reportDiagnostic}'.", nameof(reportDiagnostic));
            }
        }

        internal static bool IsAnalyzerExceptionDiagnostic(this Diagnostic diagnostic)
        {
            return IsAnalyzerExceptionDescriptor(diagnostic.Descriptor);
        }

        internal static bool IsAnalyzerExceptionDescriptor(this DiagnosticDescriptor descriptor)
        {
            if (descriptor.Id == "AD0001"
                || descriptor.Id == "AD0002")
            {
                foreach (string tag in descriptor.CustomTags)
                {
                    if (tag == WellKnownDiagnosticTags.AnalyzerException)
                        return true;
                }
            }

            return false;
        }

        internal static bool IsEffective(this DiagnosticDescriptor descriptor, SymbolAnalysisContext context)
        {
            return IsEffective(
                descriptor,
                context.Symbol.Locations[0].SourceTree,
                context.Compilation.Options,
                context.CancellationToken);
        }

        internal static bool IsEffective(this DiagnosticDescriptor descriptor, SyntaxNodeAnalysisContext context)
        {
            return IsEffective(
                descriptor,
                context.Node.SyntaxTree,
                context.Compilation.Options,
                context.CancellationToken);
        }

        internal static bool IsEffective(
            this DiagnosticDescriptor descriptor,
            SyntaxTree syntaxTree,
            CompilationOptions compilationOptions,
            CancellationToken cancellationToken = default)
        {
            var reportDiagnostic = Microsoft.CodeAnalysis.ReportDiagnostic.Default;

            if (compilationOptions
                .SyntaxTreeOptionsProvider?
                .TryGetDiagnosticValue(
                    syntaxTree,
                    descriptor.Id,
                    cancellationToken,
                    out reportDiagnostic) != true)
            {
                reportDiagnostic = compilationOptions
                    .SpecificDiagnosticOptions
                    .GetValueOrDefault(descriptor.Id);
            }

            return reportDiagnostic switch
            {
                Microsoft.CodeAnalysis.ReportDiagnostic.Default => descriptor.IsEnabledByDefault,
                Microsoft.CodeAnalysis.ReportDiagnostic.Suppress => false,
                _ => true,
            };
        }

        internal static ReportDiagnostic GetEffectiveSeverity(
            this DiagnosticDescriptor descriptor,
            SyntaxTree syntaxTree,
            CompilationOptions compilationOptions,
            CancellationToken cancellationToken = default)
        {
            SyntaxTreeOptionsProvider syntaxTreeOptionsProvider = compilationOptions.SyntaxTreeOptionsProvider;

            if (syntaxTreeOptionsProvider != null
                && syntaxTreeOptionsProvider.TryGetDiagnosticValue(
                    syntaxTree,
                    descriptor.Id,
                    cancellationToken,
                    out ReportDiagnostic syntaxTreeReportDiagnostic))
            {
                return syntaxTreeReportDiagnostic;
            }

            if (compilationOptions.SpecificDiagnosticOptions.TryGetValue(descriptor.Id, out ReportDiagnostic reportDiagnostic))
                return reportDiagnostic;

            return (descriptor.IsEnabledByDefault)
                ? descriptor.DefaultSeverity.ToReportDiagnostic()
                : Microsoft.CodeAnalysis.ReportDiagnostic.Suppress;
        }

        internal static bool IsEffective(this DiagnosticDescriptor descriptor, Compilation compilation)
        {
            return IsEffective(descriptor, compilation.Options);
        }

        internal static bool IsEffective(this DiagnosticDescriptor descriptor, CompilationOptions compilationOptions)
        {
            return (compilationOptions.SpecificDiagnosticOptions.GetValueOrDefault(descriptor.Id)) switch
            {
                Microsoft.CodeAnalysis.ReportDiagnostic.Default => descriptor.IsEnabledByDefault,
                Microsoft.CodeAnalysis.ReportDiagnostic.Suppress => false,
                _ => true,
            };
        }
    }
}
