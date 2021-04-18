// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeMetrics;

namespace Roslynator
{
    internal static class Extensions
    {
        public static SymbolGroupFilter ToSymbolGroupFilter(this TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Class:
                    return SymbolGroupFilter.Class;
                case TypeKind.Module:
                    return SymbolGroupFilter.Module;
                case TypeKind.Delegate:
                    return SymbolGroupFilter.Delegate;
                case TypeKind.Enum:
                    return SymbolGroupFilter.Enum;
                case TypeKind.Interface:
                    return SymbolGroupFilter.Interface;
                case TypeKind.Struct:
                    return SymbolGroupFilter.Struct;
                default:
                    throw new ArgumentException($"Invalid enum value '{typeKind}'", nameof(typeKind));
            }
        }

        public static MemberDeclarationKind GetMemberDeclarationKind(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Event:
                    {
                        return (((IEventSymbol)symbol).ExplicitInterfaceImplementations.Any())
                            ? MemberDeclarationKind.ExplicitlyImplementedEvent
                            : MemberDeclarationKind.Event;
                    }
                case SymbolKind.Field:
                    {
                        var fieldSymbol = (IFieldSymbol)symbol;

                        return (fieldSymbol.IsConst)
                            ? MemberDeclarationKind.Const
                            : MemberDeclarationKind.Field;
                    }
                case SymbolKind.Method:
                    {
                        var methodSymbol = (IMethodSymbol)symbol;

                        switch (methodSymbol.MethodKind)
                        {
                            case MethodKind.Ordinary:
                                return MemberDeclarationKind.Method;
                            case MethodKind.ExplicitInterfaceImplementation:
                                return MemberDeclarationKind.ExplicitlyImplementedMethod;
                            case MethodKind.Constructor:
                                return MemberDeclarationKind.Constructor;
                            case MethodKind.Destructor:
                                return MemberDeclarationKind.Destructor;
                            case MethodKind.StaticConstructor:
                                return MemberDeclarationKind.StaticConstructor;
                            case MethodKind.Conversion:
                                return MemberDeclarationKind.ConversionOperator;
                            case MethodKind.UserDefinedOperator:
                                return MemberDeclarationKind.Operator;
                        }

                        break;
                    }
                case SymbolKind.Property:
                    {
                        var propertySymbol = (IPropertySymbol)symbol;

                        bool explicitlyImplemented = propertySymbol.ExplicitInterfaceImplementations.Any();

                        if (propertySymbol.IsIndexer)
                        {
                            return (explicitlyImplemented)
                                ? MemberDeclarationKind.ExplicitlyImplementedIndexer
                                : MemberDeclarationKind.Indexer;
                        }
                        else
                        {
                            return (explicitlyImplemented)
                                ? MemberDeclarationKind.ExplicitlyImplementedProperty
                                : MemberDeclarationKind.Property;
                        }
                    }
            }

            Debug.Fail(symbol.ToDisplayString(SymbolDisplayFormats.Test));

            return MemberDeclarationKind.None;
        }

        public static SymbolGroup GetSymbolGroup(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.NamedType:
                    {
                        var namedType = (INamedTypeSymbol)symbol;

                        switch (namedType.TypeKind)
                        {
                            case TypeKind.Class:
                                return SymbolGroup.Class;
                            case TypeKind.Module:
                                return SymbolGroup.Module;
                            case TypeKind.Delegate:
                                return SymbolGroup.Delegate;
                            case TypeKind.Enum:
                                return SymbolGroup.Enum;
                            case TypeKind.Interface:
                                return SymbolGroup.Interface;
                            case TypeKind.Struct:
                                return SymbolGroup.Struct;
                        }

                        Debug.Fail(namedType.TypeKind.ToString());
                        return SymbolGroup.None;
                    }
                case SymbolKind.Event:
                    {
                        return SymbolGroup.Event;
                    }
                case SymbolKind.Field:
                    {
                        return (((IFieldSymbol)symbol).IsConst)
                            ? SymbolGroup.Const
                            : SymbolGroup.Field;
                    }
                case SymbolKind.Method:
                    {
                        return SymbolGroup.Method;
                    }
                case SymbolKind.Property:
                    {
                        return (((IPropertySymbol)symbol).IsIndexer)
                            ? SymbolGroup.Indexer
                            : SymbolGroup.Property;
                    }
            }

            Debug.Fail(symbol.Kind.ToString());
            return SymbolGroup.None;
        }

        public static string GetText(this SymbolGroup symbolGroup)
        {
            switch (symbolGroup)
            {
                case SymbolGroup.Namespace:
                    return "namespace";
                case SymbolGroup.Module:
                    return "module";
                case SymbolGroup.Class:
                    return "class";
                case SymbolGroup.Delegate:
                    return "delegate";
                case SymbolGroup.Enum:
                    return "enum";
                case SymbolGroup.Interface:
                    return "interface";
                case SymbolGroup.Struct:
                    return "struct";
                case SymbolGroup.Event:
                    return "event";
                case SymbolGroup.Field:
                    return "field";
                case SymbolGroup.Const:
                    return "const";
                case SymbolGroup.Method:
                    return "method";
                case SymbolGroup.Property:
                    return "property";
                case SymbolGroup.Indexer:
                    return "indexer";
            }

            Debug.Fail(symbolGroup.ToString());

            return "";
        }

        public static string GetPluralText(this SymbolGroup symbolGroup)
        {
            switch (symbolGroup)
            {
                case SymbolGroup.Namespace:
                    return "namespaces";
                case SymbolGroup.Module:
                    return "modules";
                case SymbolGroup.Class:
                    return "classes";
                case SymbolGroup.Delegate:
                    return "delegates";
                case SymbolGroup.Enum:
                    return "enums";
                case SymbolGroup.Interface:
                    return "interfaces";
                case SymbolGroup.Struct:
                    return "structs";
                case SymbolGroup.Event:
                    return "events";
                case SymbolGroup.Field:
                    return "fields";
                case SymbolGroup.Const:
                    return "consts";
                case SymbolGroup.Method:
                    return "methods";
                case SymbolGroup.Property:
                    return "properties";
                case SymbolGroup.Indexer:
                    return "indexers";
            }

            Debug.Fail(symbolGroup.ToString());

            return "";
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                return value;

            return default;
        }

        public static bool StartsWith(this string s, string value1, string value2, StringComparison comparisonType)
        {
            return s.StartsWith(value1, comparisonType) || s.StartsWith(value2, comparisonType);
        }

        public static bool HasFixAllProvider(this CodeFixProvider codeFixProvider, FixAllScope fixAllScope)
        {
            return codeFixProvider.GetFixAllProvider()?.GetSupportedFixAllScopes().Contains(fixAllScope) == true;
        }

        public static Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
            this Compilation compilation,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            CompilationWithAnalyzersOptions analysisOptions,
            CancellationToken cancellationToken = default)
        {
            var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, analyzers, analysisOptions);

            return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken);
        }

        public static void Add(this AnalyzerTelemetryInfo telemetryInfo, AnalyzerTelemetryInfo telemetryInfoToAdd)
        {
            telemetryInfo.CodeBlockActionsCount += telemetryInfoToAdd.CodeBlockActionsCount;
            telemetryInfo.CodeBlockEndActionsCount += telemetryInfoToAdd.CodeBlockEndActionsCount;
            telemetryInfo.CodeBlockStartActionsCount += telemetryInfoToAdd.CodeBlockStartActionsCount;
            telemetryInfo.CompilationActionsCount += telemetryInfoToAdd.CompilationActionsCount;
            telemetryInfo.CompilationEndActionsCount += telemetryInfoToAdd.CompilationEndActionsCount;
            telemetryInfo.CompilationStartActionsCount += telemetryInfoToAdd.CompilationStartActionsCount;
            telemetryInfo.ExecutionTime += telemetryInfoToAdd.ExecutionTime;
            telemetryInfo.OperationActionsCount += telemetryInfoToAdd.OperationActionsCount;
            telemetryInfo.OperationBlockActionsCount += telemetryInfoToAdd.OperationBlockActionsCount;
            telemetryInfo.OperationBlockEndActionsCount += telemetryInfoToAdd.OperationBlockEndActionsCount;
            telemetryInfo.OperationBlockStartActionsCount += telemetryInfoToAdd.OperationBlockStartActionsCount;
            telemetryInfo.SemanticModelActionsCount += telemetryInfoToAdd.SemanticModelActionsCount;
            telemetryInfo.SymbolActionsCount += telemetryInfoToAdd.SymbolActionsCount;
            telemetryInfo.SyntaxNodeActionsCount += telemetryInfoToAdd.SyntaxNodeActionsCount;
            telemetryInfo.SyntaxTreeActionsCount += telemetryInfoToAdd.SyntaxTreeActionsCount;
        }

        public static ConsoleColor GetColor(this DiagnosticSeverity diagnosticSeverity)
        {
            switch (diagnosticSeverity)
            {
                case DiagnosticSeverity.Hidden:
                    return ConsoleColor.DarkGray;
                case DiagnosticSeverity.Info:
                    return ConsoleColor.Cyan;
                case DiagnosticSeverity.Warning:
                    return ConsoleColor.Yellow;
                case DiagnosticSeverity.Error:
                    return ConsoleColor.Red;
                default:
                    throw new InvalidOperationException($"Unknown diagnostic severity '{diagnosticSeverity}'.");
            }
        }

        public static async Task<CodeMetricsInfo> CountLinesAsync(
            this ICodeMetricsService service,
            Project project,
            LinesOfCodeKind kind,
            CodeMetricsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            CodeMetricsInfo codeMetrics = default;

            foreach (Document document in project.Documents)
            {
                if (!document.SupportsSyntaxTree)
                    continue;

                CodeMetricsInfo documentMetrics = await service.CountLinesAsync(document, kind, options, cancellationToken).ConfigureAwait(false);

                codeMetrics = codeMetrics.Add(documentMetrics);
            }

            return codeMetrics;
        }

        public static async Task<CodeMetricsInfo> CountLinesAsync(
            this ICodeMetricsService service,
            Document document,
            LinesOfCodeKind kind,
            CodeMetricsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            SyntaxTree tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (tree == null)
                return default;

            if (!options.IncludeGeneratedCode
                && GeneratedCodeUtility.IsGeneratedCode(tree, f => service.SyntaxFacts.IsComment(f), cancellationToken))
            {
                return default;
            }

            SyntaxNode root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            switch (kind)
            {
                case LinesOfCodeKind.Physical:
                    return service.CountPhysicalLines(root, sourceText, options, cancellationToken);
                case LinesOfCodeKind.Logical:
                    return service.CountLogicalLines(root, sourceText, options, cancellationToken);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static OperationCanceledException GetOperationCanceledException(this AggregateException aggregateException)
        {
            OperationCanceledException operationCanceledException = null;

            foreach (Exception ex in aggregateException.InnerExceptions)
            {
                if (ex is OperationCanceledException operationCanceledException2)
                {
                    if (operationCanceledException == null)
                        operationCanceledException = operationCanceledException2;
                }
                else if (ex is AggregateException aggregateException2)
                {
                    foreach (Exception ex2 in aggregateException2.InnerExceptions)
                    {
                        if (ex2 is OperationCanceledException operationCanceledException3)
                        {
                            if (operationCanceledException == null)
                                operationCanceledException = operationCanceledException3;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return operationCanceledException;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public static ConsoleColor GetColor(this WorkspaceDiagnosticKind kind)
        {
            switch (kind)
            {
                case WorkspaceDiagnosticKind.Failure:
                    return ConsoleColor.Red;
                case WorkspaceDiagnosticKind.Warning:
                    return ConsoleColor.Yellow;
                default:
                    throw new InvalidOperationException($"Unknown value '{kind}'.");
            }
        }

        public static bool IsEffective(
            this Diagnostic diagnostic,
            CodeAnalysisOptions codeAnalysisOptions,
            CompilationOptions compilationOptions,
            CancellationToken cancellationToken = default)
        {
            if (!codeAnalysisOptions.IsSupportedDiagnosticId(diagnostic.Id))
                return false;

            SyntaxTree tree = diagnostic.Location.SourceTree;

            ReportDiagnostic reportDiagnostic = (tree != null)
                ? diagnostic.Descriptor.GetEffectiveSeverity(tree, compilationOptions, cancellationToken)
                : diagnostic.Descriptor.GetEffectiveSeverity(compilationOptions);

            return reportDiagnostic != ReportDiagnostic.Suppress
                && reportDiagnostic.ToDiagnosticSeverity() >= codeAnalysisOptions.SeverityLevel;
        }
    }
}
