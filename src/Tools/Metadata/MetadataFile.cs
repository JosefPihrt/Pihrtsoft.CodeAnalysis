// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Roslynator.Metadata
{
    public static class MetadataFile
    {
        private static readonly Regex _lfWithoutCr = new Regex(@"(?<!\r)\n");

        private static string NormalizeNewLine(this string value)
        {
            return (value != null) ? _lfWithoutCr.Replace(value, "\r\n") : null;
        }

        public static IEnumerable<AnalyzerMetadata> ReadAnalyzers(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements())
            {
                string id = element.Element("Id").Value;
                string title = element.Element("Title").Value;
                string identifier = element.Attribute("Identifier").Value;
                string messageFormat = element.Element("MessageFormat")?.Value ?? title;
                string category = element.Element("Category").Value;
                string defaultSeverity = element.Element("DefaultSeverity").Value;
                var isEnabledByDefault = bool.Parse(element.Element("IsEnabledByDefault").Value);
                bool isObsolete = element.AttributeValueAsBooleanOrDefault("IsObsolete");
                bool supportsFadeOut = element.ElementValueAsBooleanOrDefault("SupportsFadeOut");
                bool supportsFadeOutAnalyzer = element.ElementValueAsBooleanOrDefault("SupportsFadeOutAnalyzer");
                string minLanguageVersion = element.Element("MinLanguageVersion")?.Value;
                string summary = element.Element("Summary")?.Value.NormalizeNewLine();
                string remarks = element.Element("Remarks")?.Value.NormalizeNewLine();
                IEnumerable<SampleMetadata> samples = LoadSamples(element)?.Select(f => f.WithBefore(f.Before.Replace("[|Id|]", id)));
                IEnumerable<LinkMetadata> links = LoadLinks(element);
                IEnumerable<AnalyzerOptionMetadata> options = LoadOptions(element, id);

                yield return new AnalyzerMetadata(
                    id: id,
                    identifier: identifier,
                    title: title,
                    messageFormat: messageFormat,
                    category: category,
                    defaultSeverity: defaultSeverity,
                    isEnabledByDefault: isEnabledByDefault,
                    isObsolete: isObsolete,
                    supportsFadeOut: supportsFadeOut,
                    supportsFadeOutAnalyzer: supportsFadeOutAnalyzer,
                    minLanguageVersion: minLanguageVersion,
                    summary: summary,
                    remarks: remarks,
                    samples: samples,
                    links: links,
                    options: options,
                    kind: AnalyzerOptionKind.None,
                    parent: null);
            }
        }

        public static IEnumerable<RefactoringMetadata> ReadRefactorings(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements())
            {
                yield return new RefactoringMetadata(
                    element.Attribute("Id")?.Value,
                    element.Attribute("Identifier").Value,
                    element.Attribute("Title").Value,
                    element.AttributeValueAsBooleanOrDefault("IsEnabledByDefault", true),
                    element.AttributeValueAsBooleanOrDefault("IsObsolete", false),
                    element.Element("Span")?.Value,
                    element.Element("Summary")?.Value.NormalizeNewLine(),
                    element.Element("Remarks")?.Value.NormalizeNewLine(),
                    element.Element("Syntaxes")
                        .Elements("Syntax")
                        .Select(f => new SyntaxMetadata(f.Value)),
                    LoadImages(element),
                    LoadSamples(element),
                    LoadLinks(element));
            }
        }

        private static IEnumerable<ImageMetadata> LoadImages(XElement element)
        {
            return element
                .Element("Images")?
                .Elements("Image")
                .Select(f => new ImageMetadata(f.Value));
        }

        private static IEnumerable<SampleMetadata> LoadSamples(XElement element)
        {
            return element
                .Element("Samples")?
                .Elements("Sample")
                .Select(f =>
                {
                    XElement before = f.Element("Before");
                    XElement after = f.Element("After");

                    return new SampleMetadata(
                        before.Value.NormalizeNewLine(),
                        after?.Value.NormalizeNewLine());
                });
        }

        private static IEnumerable<LinkMetadata> LoadLinks(XElement element)
        {
            return element
                .Element("Links")?
                .Elements("Link")
                .Select(f => new LinkMetadata(f.Element("Url").Value, f.Element("Text")?.Value, f.Element("Title")?.Value));
        }

        private static IEnumerable<AnalyzerOptionMetadata> LoadOptions(XElement element, string parentId)
        {
            return element
                .Element("Options")?
                .Elements("Option")
                .Select(f => LoadOption(f, parentId));
        }

        private static AnalyzerOptionMetadata LoadOption(XElement element, string parentId)
        {
            string title = element.Element("Title").Value;

            string identifier = element.Attribute("Identifier").Value;
            string id = element.Element("Id")?.Value;
            string optionKey = element.Element("OptionKey").Value;
            string optionValue = element.Element("OptionValue")?.Value;
            var kind = (AnalyzerOptionKind)Enum.Parse(typeof(AnalyzerOptionKind), element.Element("Kind").Value);
            bool isEnabledByDefault = element.ElementValueAsBooleanOrDefault("IsEnabledByDefault");
            bool supportsFadeOut = element.ElementValueAsBooleanOrDefault("SupportsFadeOut");
            string minLanguageVersion = element.Element("MinLanguageVersion")?.Value;
            string summary = element.Element("Summary")?.Value.NormalizeNewLine();
            IEnumerable<SampleMetadata> samples = LoadSamples(element)?.Select(f => f.WithBefore(f.Before.Replace("[|Id|]", parentId)));
            bool isObsolete = element.AttributeValueAsBooleanOrDefault("IsObsolete");

            return new AnalyzerOptionMetadata(
                identifier: identifier,
                id: id,
                parentId: parentId,
                optionKey: optionKey,
                optionValue: optionValue,
                kind: kind,
                title: title,
                isEnabledByDefault: isEnabledByDefault,
                supportsFadeOut: supportsFadeOut,
                minLanguageVersion: minLanguageVersion,
                summary: summary,
                samples: samples,
                isObsolete: isObsolete);
        }

        public static IEnumerable<CodeFixMetadata> ReadCodeFixes(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements())
            {
                yield return new CodeFixMetadata(
                    element.Attribute("Id").Value,
                    element.Attribute("Identifier").Value,
                    element.Attribute("Title").Value,
                    element.AttributeValueAsBooleanOrDefault("IsEnabledByDefault", true),
                    element.AttributeValueAsBooleanOrDefault("IsObsolete", false),
                    element.Element("FixableDiagnosticIds")
                        .Elements("Id")
                        .Select(f => f.Value));
            }
        }

        public static IEnumerable<CompilerDiagnosticMetadata> ReadCompilerDiagnostics(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements("Diagnostic"))
            {
                yield return new CompilerDiagnosticMetadata(
                    element.Attribute("Id").Value,
                    element.Attribute("Identifier").Value,
                    element.Attribute("Title").Value,
                    element.Attribute("Message").Value,
                    element.Attribute("Severity").Value,
                    element.Attribute("HelpUrl").Value);
            }
        }

        public static void SaveCompilerDiagnostics(IEnumerable<CompilerDiagnosticMetadata> diagnostics, string path)
        {
            var doc = new XDocument(
                new XElement(
                    "Diagnostics",
                    diagnostics.Select(f =>
                    {
                        return new XElement(
                            "Diagnostic",
                            new XAttribute("Id", f.Id),
                            new XAttribute("Identifier", f.Identifier),
                            new XAttribute("Severity", f.Severity ?? ""),
                            new XAttribute("Title", f.Title),
                            new XAttribute("Message", f.MessageFormat ?? ""),
                            new XAttribute("HelpUrl", f.HelpUrl ?? "")
                            );
                    })));

            using (var fs = new FileStream(path, FileMode.Create))
            using (XmlWriter xw = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true }))
                doc.Save(xw);
        }

        public static IEnumerable<SourceFile> ReadSourceFiles(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements())
            {
                string id = element.Attribute("Id").Value;

                yield return new SourceFile(
                    id,
                    element
                        .Element("Paths")
                        .Elements("Path")
                        .Select(f => f.Value));
            }
        }

        public static void SaveSourceFiles(IEnumerable<SourceFile> sourceFiles, string path)
        {
            var doc = new XDocument(
                new XElement(
                    "SourceFiles",
                    sourceFiles.Select(sourceFile =>
                    {
                        return new XElement(
                            "SourceFile",
                            new XAttribute("Id", sourceFile.Id),
                            new XElement("Paths", sourceFile.Paths.Select(p => new XElement("Path", p.Replace("\\", "/"))))
                        );
                    })));

            using (var fs = new FileStream(path, FileMode.Create))
            using (XmlWriter xw = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true }))
                doc.Save(xw);
        }

        public static void CleanAnalyzers(string filePath)
        {
            Debug.WriteLine(filePath);

            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements())
            {
                string title = element.Element("Title").Value;

                XElement messageFormatElement = element.Element("MessageFormat");

                if (messageFormatElement != null)
                {
                    string messageFormat = messageFormatElement.Value;

                    if (string.IsNullOrWhiteSpace(messageFormat)
                        || string.Equals(title, messageFormat, System.StringComparison.Ordinal))

                    {
                        messageFormatElement.Remove();
                    }
                }

                XElement supportsFadeOutElement = element.Element("SupportsFadeOut");

                if (supportsFadeOutElement != null
                    && !bool.Parse(supportsFadeOutElement.Value))
                {
                    supportsFadeOutElement.Remove();
                }

                XElement supportsFadeOutAnalyzerElement = element.Element("SupportsFadeOutAnalyzer");

                if (supportsFadeOutAnalyzerElement != null
                    && !bool.Parse(supportsFadeOutAnalyzerElement.Value))
                {
                    supportsFadeOutAnalyzerElement.Remove();
                }

                XElement minLanguageVersionElement = element.Element("MinLanguageVersion");

                if (minLanguageVersionElement != null
                    && string.IsNullOrWhiteSpace(minLanguageVersionElement.Value))
                {
                    minLanguageVersionElement.Remove();
                }

                XElement summaryElement = element.Element("Summary");

                if (summaryElement != null
                    && string.IsNullOrWhiteSpace(summaryElement.Value))
                {
                    summaryElement.Remove();
                }

                XElement remarksElement = element.Element("Remarks");

                if (remarksElement != null
                    && string.IsNullOrWhiteSpace(remarksElement.Value))
                {
                    remarksElement.Remove();
                }

                XElement configurationElement = element.Element("Configuration");

                if (configurationElement != null
                    && string.IsNullOrWhiteSpace(configurationElement.Value))
                {
                    configurationElement.Remove();
                }

                XElement samplesElement = element.Element("Samples");

                if (samplesElement != null)
                {
                    foreach (XElement sampleElement in samplesElement.Elements("Sample"))
                    {
                        XElement beforeElement = sampleElement.Element("Before");
                        XElement afterElement = sampleElement.Element("After");

                        if (string.IsNullOrEmpty(beforeElement?.Value)
                            && string.IsNullOrEmpty(afterElement?.Value))
                        {
                            sampleElement.Remove();
                            continue;
                        }

                        if (beforeElement != null)
                        {
                            string before = beforeElement.Value;

                            if (!before.Contains('\n'))
                            {
                                string newBefore = Regex.Replace(before, @"\ *//\ *\[\|Id\|\]\ *\z", "");

                                if (before.Length != newBefore.Length)
                                    beforeElement.ReplaceNodes(new XCData(newBefore));
                            }
                        }
                    }

                    if (samplesElement.IsEmpty)
                        samplesElement.Remove();
                }

                XElement linksElement = element.Element("Links");

                if (linksElement != null)
                {
                    foreach (XElement linkElement in linksElement.Elements("Link"))
                    {
                        if (string.IsNullOrWhiteSpace(linkElement.Value))
                            linkElement.Remove();
                    }
                }
            }

            using (var sw = new StreamWriter(filePath))
            using (XmlWriter xw = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 }))
                doc.Save(xw);
        }
    }
}
