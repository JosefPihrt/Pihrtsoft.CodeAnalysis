// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RenamePrivateFieldAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.RenamePrivateFieldToCamelCaseWithUnderscore); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSymbolAction(f => AnalyzeFieldSymbol(f), SymbolKind.Field);
        }

        private static void AnalyzeFieldSymbol(SymbolAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            if (!fieldSymbol.IsConst
                && !fieldSymbol.IsImplicitlyDeclared
                && fieldSymbol.DeclaredAccessibility == Accessibility.Private
                && !string.IsNullOrEmpty(fieldSymbol.Name)
                && !IsValidIdentifier(fieldSymbol.Name))
            {
                if (!fieldSymbol.IsStatic
                    || !fieldSymbol.IsReadOnly
                    || !AnalyzerOptions.DoNotRenamePrivateStaticReadOnlyFieldToCamelCaseWithUnderscore.IsEnabled(context))
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticDescriptors.RenamePrivateFieldToCamelCaseWithUnderscore,
                        fieldSymbol.Locations[0]);
                }
            }
        }

        public static bool IsValidIdentifier(string value)
        {
            int i = 0;

            if (value[i] == 's'
                || value[i] == 't')
            {
                i++;
            }

            if (i < value.Length
                && value[i] == '_')
            {
                i++;

                if (i < value.Length)
                {
                    return value[i] != '_'
                        && !char.IsUpper(value[i]);
                }

                return true;
            }

            return false;
        }
    }
}
