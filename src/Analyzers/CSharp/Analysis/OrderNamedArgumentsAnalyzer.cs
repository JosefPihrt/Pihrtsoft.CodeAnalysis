// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OrderNamedArgumentsAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.OrderNamedArguments);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeBaseArgumentList(f), SyntaxKind.ArgumentList);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseArgumentList(f), SyntaxKind.BracketedArgumentList);
        }

        private static void AnalyzeBaseArgumentList(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            var argumentList = (BaseArgumentListSyntax)context.Node;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            int index = IndexOfFirstFixableParameter(argumentList, arguments, context.SemanticModel, context.CancellationToken);

            if (index == -1)
                return;

            TextSpan span = TextSpan.FromBounds(arguments[index].SpanStart, arguments.Last().Span.End);

            if (argumentList.ContainsDirectives(span))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.OrderNamedArguments,
                Location.Create(argumentList.SyntaxTree, span));
        }

        public static int IndexOfFirstFixableParameter(
            BaseArgumentListSyntax argumentList,
            SeparatedSyntaxList<ArgumentSyntax> arguments,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            int firstIndex = -1;

            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i].NameColon != null)
                {
                    firstIndex = i;
                    break;
                }
            }

            if (firstIndex != -1
                && firstIndex != arguments.Count - 1)
            {
                ISymbol symbol = semanticModel.GetSymbol(argumentList.Parent, cancellationToken);

                if (symbol != null)
                {
                    ImmutableArray<IParameterSymbol> parameters = symbol.ParametersOrDefault();

                    Debug.Assert(!parameters.IsDefault, symbol.Kind.ToString());

                    if (!parameters.IsDefault
                        && parameters.Length == arguments.Count)
                    {
                        for (int i = firstIndex; i < arguments.Count; i++)
                        {
                            ArgumentSyntax argument = arguments[i];

                            NameColonSyntax nameColon = argument.NameColon;

                            if (nameColon == null)
                                break;

                            if (!string.Equals(
                                nameColon.Name.Identifier.ValueText,
                                parameters[i].Name,
                                StringComparison.Ordinal))
                            {
                                int fixableIndex = i;

                                i++;

                                while (i < arguments.Count)
                                {
                                    if (arguments[i].NameColon == null)
                                        break;

                                    i++;
                                }

                                return fixableIndex;
                            }
                        }
                    }
                }
            }

            return -1;
        }
    }
}
