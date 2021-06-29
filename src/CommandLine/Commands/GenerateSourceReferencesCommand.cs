// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis;
using Roslynator.Documentation;
using Roslynator.FindSymbols;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class GenerateSourceReferencesCommand : MSBuildWorkspaceCommand<BaseCommandResult>
    {
        public GenerateSourceReferencesCommand(
            GenerateSourceReferencesCommandLineOptions options,
            DocumentationDepth depth,
            Visibility visibility,
            in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            Depth = depth;
            Visibility = visibility;
        }

        public GenerateSourceReferencesCommandLineOptions Options { get; }

        public DocumentationDepth Depth { get; }

        public Visibility Visibility { get; }

        public override async Task<BaseCommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var filter = new SymbolFilterOptions(Visibility.ToVisibilityFilter());
            var success = false;

            WriteLine($"Save source references to '{Options.Output}'.", Verbosity.Minimal);

            using (XmlWriter writer = XmlWriter.Create(Options.Output, new XmlWriterSettings() { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("source");
                writer.WriteStartElement("repository");

                writer.WriteAttributeString("type", Options.RepositoryType);
                writer.WriteAttributeString("url", Options.RepositoryUrl);
                writer.WriteAttributeString("version", Options.Version);
                writer.WriteAttributeString("branch", Options.Branch);
                writer.WriteAttributeString("commit", Options.Commit);
                writer.WriteStartElement("members");

                foreach (Project project in FilterProjects(projectOrSolution))
                {
                    Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                    IAssemblySymbol assembly = compilation.Assembly;

                    foreach (INamedTypeSymbol type in assembly.GetTypes(symbol => filter.IsMatch(symbol)))
                    {
                        WriteSymbol(writer, type, cancellationToken);
                        success = true;

                        foreach (ISymbol member in type.GetMembers())
                        {
                            if (!member.IsKind(SymbolKind.NamedType))
                            {
                                if (filter.IsMatch(member)
                                    || member.IsExplicitImplementation())
                                {
                                    WriteSymbol(writer, member, cancellationToken);
                                }
                            }
                        }
                    }
                }

                writer.WriteEndDocument();
            }

            WriteLine($"Source references successfully saved to '{Options.Output}'.", Verbosity.Minimal);

            return (success) ? BaseCommandResult.Success : BaseCommandResult.NotSuccess;
        }

        private void WriteSymbol(XmlWriter writer, ISymbol symbol, CancellationToken cancellationToken)
        {
            writer.WriteStartElement("member");
            writer.WriteAttributeString("name", symbol.GetDocumentationCommentId());

            ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

            Debug.Assert(syntaxReferences.Any() || IsImplicitConstructor(symbol), symbol.ToDisplayString());

            ImmutableArray<SyntaxReference>.Enumerator en = syntaxReferences.GetEnumerator();

            if (en.MoveNext())
            {
                writer.WriteStartElement("locations");

                do
                {
                    writer.WriteStartElement("location");

                    SyntaxTree tree = en.Current.SyntaxTree;

                    string path = tree.FilePath;

                    if (path.StartsWith(Options.RootPath, StringComparison.OrdinalIgnoreCase))
                        path = path.Remove(0, Options.RootPath.Length).TrimStart(Path.DirectorySeparatorChar);

                    if (Path.DirectorySeparatorChar == '\\')
                        path = path.Replace(Path.DirectorySeparatorChar, '/');

                    Debug.Assert(path != null, symbol.ToDisplayString());

                    writer.WriteAttributeString("path", path);

                    int line = tree.GetLineSpan(en.Current.Span, cancellationToken).StartLine() + 1;

                    writer.WriteAttributeString("line", line.ToString(CultureInfo.InvariantCulture));

                    writer.WriteEndElement();

                } while (en.MoveNext());

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

#pragma warning disable RS1024
        private static bool IsImplicitConstructor(ISymbol symbol)
        {
            return symbol is IMethodSymbol methodSymbol
                && methodSymbol.MethodKind == MethodKind.Constructor
                && !methodSymbol.Parameters.Any()
                && methodSymbol.ContainingType.InstanceConstructors.SingleOrDefault(shouldThrow: false) == methodSymbol;
        }
#pragma warning restore RS1024
    }
}
