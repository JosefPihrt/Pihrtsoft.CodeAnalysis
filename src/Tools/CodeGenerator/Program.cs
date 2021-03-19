// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeGeneration.CSharp;
using Roslynator.Metadata;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator.CodeGeneration
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
#if DEBUG
                args = new[] { @"..\..\..\..\.." };
#else
                args = new string[] { Environment.CurrentDirectory };
#endif
            }

            string rootPath = args[0];

            StringComparer comparer = StringComparer.InvariantCulture;

            var metadata = new RoslynatorMetadata(rootPath);

            ImmutableArray<AnalyzerMetadata> analyzers = metadata.Analyzers;
            ImmutableArray<AnalyzerMetadata> codeAnalysisAnalyzers = metadata.CodeAnalysisAnalyzers;
            ImmutableArray<AnalyzerMetadata> formattingAnalyzers = metadata.FormattingAnalyzers;
            ImmutableArray<RefactoringMetadata> refactorings = metadata.Refactorings;
            ImmutableArray<CodeFixMetadata> codeFixes = metadata.CodeFixes;
            ImmutableArray<CompilerDiagnosticMetadata> compilerDiagnostics = metadata.CompilerDiagnostics;

            WriteCompilationUnit(
                @"Refactorings\CSharp\RefactoringIdentifiers.Generated.cs",
                RefactoringIdentifiersGenerator.Generate(refactorings, obsolete: false, comparer: comparer));

            WriteCompilationUnit(
                @"Refactorings\CSharp\RefactoringIdentifiers.Deprecated.Generated.cs",
                RefactoringIdentifiersGenerator.Generate(refactorings, obsolete: true, comparer: comparer));

            WriteCompilationUnit(
                @"VisualStudio.Common\RefactoringsOptionsPage.Generated.cs",
                RefactoringsOptionsPageGenerator.Generate(refactorings.Where(f => !f.IsObsolete), comparer));

            WriteDiagnostics(@"Analyzers\CSharp", analyzers, @namespace: "Roslynator.CSharp");

            WriteDiagnostics(@"CodeAnalysis.Analyzers\CSharp", codeAnalysisAnalyzers, @namespace: "Roslynator.CodeAnalysis.CSharp");

            WriteDiagnostics(@"Formatting.Analyzers\CSharp", formattingAnalyzers, @namespace: "Roslynator.Formatting.CSharp");

            WriteCompilationUnit(
                @"CodeFixes\CSharp\CompilerDiagnosticDescriptors.Generated.cs",
                CompilerDiagnosticDescriptorsGenerator.Generate(compilerDiagnostics, comparer: comparer, @namespace: "Roslynator.CSharp"),
                normalizeWhitespace: false);

            WriteCompilationUnit(
                @"CodeFixes\CSharp\CodeFixDescriptors.Generated.cs",
                CodeFixDescriptorsGenerator.Generate(codeFixes.Where(f => !f.IsObsolete), comparer: comparer, @namespace: "Roslynator.CSharp"),
                normalizeWhitespace: false);

            WriteCompilationUnit(
                @"CodeFixes\CSharp\CodeFixIdentifiers.Generated.cs",
                CodeFixIdentifiersGenerator.Generate(codeFixes, comparer));

            WriteCompilationUnit(
                @"VisualStudio.Common\CodeFixesOptionsPage.Generated.cs",
                CodeFixesOptionsPageGenerator.Generate(codeFixes, comparer));

            WriteCompilationUnit(
                @"CSharp\CSharp\CompilerDiagnosticIdentifiers.Generated.cs",
                CompilerDiagnosticIdentifiersGenerator.Generate(compilerDiagnostics, comparer));

            WriteCompilationUnit(
                @"Tools\CodeGeneration\CSharp\Symbols.Generated.cs",
                SymbolsGetKindsGenerator.Generate());

            WriteCompilationUnit(
                @"CSharp\CSharp\SyntaxWalkers\CSharpSyntaxNodeWalker.cs",
                CSharpSyntaxNodeWalkerGenerator.Generate());

            string ruleSetXml = File.ReadAllText(Path.Combine(rootPath, @"Tools\CodeGeneration\DefaultRuleSet.xml"));

            WriteCompilationUnit(
                @"VisualStudio.Common\RuleSetHelpers.Generated.cs",
                RuleSetGenerator.Generate(ruleSetXml));

            File.WriteAllText(
                Path.Combine(rootPath, @"VisualStudioCode\package\src\configurationFiles.generated.ts"),
                @"export const configurationFileContent = {
	ruleset: `"
                    + ruleSetXml
                    + @"`,
	config: `<?xml version=""1.0"" encoding=""utf-8""?>
<Roslynator>
  <Settings>
    <General>
      <!-- <PrefixFieldIdentifierWithUnderscore>true</PrefixFieldIdentifierWithUnderscore> -->
    </General>
    <Refactorings>
      <!-- <Refactoring Id=""RRxxxx"" IsEnabled=""false"" /> -->
    </Refactorings>
    <CodeFixes>
      <!-- <CodeFix Id=""CSxxxx.RCFxxxx"" IsEnabled=""false"" /> -->
      <!-- <CodeFix Id=""CSxxxx"" IsEnabled=""false"" /> -->
      <!-- <CodeFix Id=""RCFxxxx"" IsEnabled=""false"" /> -->
    </CodeFixes>
  </Settings>
</Roslynator>`
};",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            Console.WriteLine($"number of analyzers: {analyzers.Count(f => !f.IsObsolete)}");
            Console.WriteLine($"number of code analysis analyzers: {codeAnalysisAnalyzers.Count(f => !f.IsObsolete)}");
            Console.WriteLine($"number of formatting analyzers: {formattingAnalyzers.Count(f => !f.IsObsolete)}");
            Console.WriteLine($"number of refactorings: {refactorings.Length}");
            Console.WriteLine($"number of code fixes: {codeFixes.Length}");
            Console.WriteLine($"number of fixable compiler diagnostics: {codeFixes.SelectMany(f => f.FixableDiagnosticIds).Distinct().Count()}");

            void WriteDiagnostics(
                string dirPath,
                ImmutableArray<AnalyzerMetadata> analyzers,
                string @namespace,
                string descriptorsClassName = "DiagnosticDescriptors",
                string identifiersClassName = "DiagnosticIdentifiers")
            {
                WriteCompilationUnit(
                    Path.Combine(dirPath, $"{descriptorsClassName}.Generated.cs"),
                    DiagnosticDescriptorsGenerators.Default.Generate(analyzers, obsolete: false, comparer: comparer, @namespace: @namespace, className: descriptorsClassName, identifiersClassName: identifiersClassName),
                    normalizeWhitespace: false);

                WriteCompilationUnit(
                    Path.Combine(dirPath, $"{descriptorsClassName}.Deprecated.Generated.cs"),
                    DiagnosticDescriptorsGenerators.Default.Generate(analyzers, obsolete: true, comparer: comparer, @namespace: @namespace, className: descriptorsClassName, identifiersClassName: identifiersClassName),
                    normalizeWhitespace: false);

                WriteCompilationUnit(
                    Path.Combine(dirPath, $"{identifiersClassName}.Generated.cs"),
                    DiagnosticIdentifiersGenerator.Generate(analyzers, obsolete: false, comparer: comparer, @namespace: @namespace, className: identifiersClassName));

                WriteCompilationUnit(
                    Path.Combine(dirPath, $"{identifiersClassName}.Deprecated.Generated.cs"),
                    DiagnosticIdentifiersGenerator.Generate(analyzers, obsolete: true, comparer: comparer, @namespace: @namespace, className: identifiersClassName));

                IEnumerable<AnalyzerMetadata> optionAnalyzers = analyzers.SelectMany(f => f.OptionAnalyzers);

                if (optionAnalyzers.Any())
                {
                    WriteCompilationUnit(
                        Path.Combine(dirPath, "AnalyzerOptionDiagnosticDescriptors.Generated.cs"),
                        DiagnosticDescriptorsGenerators.Default.Generate(optionAnalyzers, obsolete: false, comparer: comparer, @namespace: @namespace, className: "AnalyzerOptionDiagnosticDescriptors", identifiersClassName: "AnalyzerOptionDiagnosticIdentifiers"),
                        normalizeWhitespace: false,
                        fileMustExist: false);

                    WriteCompilationUnit(
                        Path.Combine(dirPath, "AnalyzerOptionDiagnosticIdentifiers.Generated.cs"),
                        DiagnosticIdentifiersGenerator.Generate(optionAnalyzers, obsolete: false, comparer: comparer, @namespace: @namespace, className: "AnalyzerOptionDiagnosticIdentifiers"),
                        fileMustExist: false);

                    WriteCompilationUnit(
                        Path.Combine(dirPath, "AnalyzerOptions.Generated.cs"),
                        AnalyzerOptionDescriptorsGenerator.Generate(analyzers, obsolete: false, comparer: comparer, @namespace: @namespace, className: "AnalyzerOptions"),
                        fileMustExist: false);
                }

                IEnumerable<string> analyzerOptionIdentifiers = analyzers
                    .SelectMany(f => f.OptionAnalyzers)
                    .Where(f => f.Id != null)
                    .Select(f => f.Identifier);

                WriteCompilationUnit(
                    Path.Combine(dirPath, "AnalyzerOptionsAnalyzer.Generated.cs"),
                    AnalyzerOptionsAnalyzerGenerator.Generate(analyzerOptionIdentifiers, @namespace: @namespace),
                    fileMustExist: false);
            }

            void WriteCompilationUnit(
                string path,
                CompilationUnitSyntax compilationUnit,
                bool autoGenerated = true,
                bool normalizeWhitespace = true,
                bool fileMustExist = true,
                bool overwrite = true)
            {
                CodeGenerationHelpers.WriteCompilationUnit(
                    path: Path.Combine(rootPath, path),
                    compilationUnit: compilationUnit,
                    banner: "Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.",
                    autoGenerated: autoGenerated,
                    normalizeWhitespace: normalizeWhitespace,
                    fileMustExist: fileMustExist,
                    overwrite: overwrite);
            }
        }
    }
}
