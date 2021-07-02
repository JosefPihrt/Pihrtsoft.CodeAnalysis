// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Diagnostics;
using static Roslynator.Logger;

namespace Roslynator.CommandLine.Xml
{
    internal static class DiagnosticXmlSerializer
    {
        public static void Serialize(
            IEnumerable<ProjectAnalysisResult> results,
            string filePath,
            IFormatProvider formatProvider = null)
        {
            XElement summary = CreateSummary(results.SelectMany(f => f.CompilerDiagnostics.Concat(f.Diagnostics)), formatProvider);

            IEnumerable<XElement> projectElements = results
                .Where(f => f.Diagnostics.Any())
                .OrderBy(f => f.Project.FilePath, FileSystemHelpers.Comparer)
                .Select(result => SerializeProjectAnalysisResult(result, formatProvider));

            SerializeDocument(filePath, summary, projectElements);
        }

        private static XElement SerializeProjectAnalysisResult(
            ProjectAnalysisResult result,
            IFormatProvider formatProvider)
        {
            return new XElement(
                "Project",
                new XAttribute("Name", result.Project.Name),
                new XAttribute("FilePath", result.Project.FilePath),
                new XElement(
                    "Diagnostics",
                    result.Diagnostics
                        .OrderBy(f => f.LineSpan.Path)
                        .ThenBy(f => f.Descriptor.Id)
                        .ThenBy(f => f.LineSpan.StartLinePosition.Line)
                        .ThenBy(f => f.LineSpan.StartLinePosition.Character)
                        .Select(f => SerializeDiagnostic(f, formatProvider))
                )
            );
        }

        private static XElement SerializeDiagnostic(DiagnosticInfo diagnostic, IFormatProvider formatProvider)
        {
            XElement filePathElement = null;
            XElement locationElement = null;

            FileLinePositionSpan span = diagnostic.LineSpan;

            if (span.IsValid)
            {
                filePathElement = new XElement("FilePath", span.Path);

                LinePosition linePosition = span.Span.Start;

                locationElement = new XElement(
                    "Location",
                    new XAttribute("Line", linePosition.Line + 1),
                    new XAttribute("Character", linePosition.Character + 1));
            }

            return new XElement(
                "Diagnostic",
                new XAttribute("Id", diagnostic.Descriptor.Id),
                new XElement("Severity", diagnostic.Severity),
                new XElement("Message", diagnostic.Descriptor.Title.ToString(formatProvider)),
                filePathElement,
                locationElement);
        }

        private static XElement CreateSummary(IEnumerable<DiagnosticInfo> diagnostics, IFormatProvider formatProvider)
        {
            return new XElement(
                "Summary",
                diagnostics
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .OrderBy(f => f.Key, DiagnosticDescriptorComparer.Id)
                    .Select(f => new XElement(
                        "Diagnostic",
                        new XAttribute("Id", f.Key.Id),
                        new XAttribute("Title", f.Key.Title.ToString(formatProvider)),
                        new XAttribute("Count", f.Count()))));
        }

        private static void SerializeDocument(string filePath, XElement summary, params object[] projects)
        {
            var document = new XDocument(
                new XElement(
                    "Roslynator",
                    new XElement(
                        "CodeAnalysis",
                        summary,
                        new XElement("Projects", projects))));

            WriteLine($"Save code analysis to '{filePath}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (XmlWriter xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings() { Indent = true, CloseOutput = false }))
                document.Save(xmlWriter);
        }
    }
}
