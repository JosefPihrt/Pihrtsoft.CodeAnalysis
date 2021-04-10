// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;
using static Roslynator.DiagnosticHelpers;

namespace Roslynator.CSharp.Analysis
{
    public static class FixDocumentationCommentTagAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, XmlElementInfo elementInfo)
        {
            if (elementInfo.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)elementInfo.Element;

            foreach (XmlNodeSyntax node in element.Content)
            {
                XmlElementInfo elementInfo2 = SyntaxInfo.XmlElementInfo(node);

                if (elementInfo2.Success)
                {
                    switch (elementInfo2.GetTag())
                    {
                        case XmlTag.C:
                            {
                                AnalyzeCElement(context, elementInfo2);
                                break;
                            }
                        case XmlTag.Code:
                            {
                                AnalyzeCodeElement(context, elementInfo2);
                                break;
                            }
                        case XmlTag.List:
                            {
                                AnalyzeList(context, elementInfo2);
                                break;
                            }
                        case XmlTag.Para:
                        case XmlTag.ParamRef:
                        case XmlTag.See:
                        case XmlTag.TypeParamRef:
                            {
                                Analyze(context, elementInfo2);
                                break;
                            }
                        case XmlTag.Content:
                        case XmlTag.Example:
                        case XmlTag.Exception:
                        case XmlTag.Exclude:
                        case XmlTag.Include:
                        case XmlTag.InheritDoc:
                        case XmlTag.Param:
                        case XmlTag.Permission:
                        case XmlTag.Remarks:
                        case XmlTag.Returns:
                        case XmlTag.SeeAlso:
                        case XmlTag.Summary:
                        case XmlTag.TypeParam:
                        case XmlTag.Value:
                            {
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }

        private static void AnalyzeList(SyntaxNodeAnalysisContext context, XmlElementInfo elementInfo)
        {
            if (elementInfo.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)elementInfo.Element;

            foreach (XmlNodeSyntax node in element.Content)
            {
                XmlElementInfo elementInfo2 = SyntaxInfo.XmlElementInfo(node);

                if (!elementInfo2.Success)
                    continue;

                if (elementInfo2.IsEmptyElement)
                    continue;

                if (!elementInfo2.HasLocalName("listheader", "item"))
                    continue;

                var element2 = (XmlElementSyntax)elementInfo2.Element;

                foreach (XmlNodeSyntax node2 in element2.Content)
                {
                    XmlElementInfo elementInfo3 = SyntaxInfo.XmlElementInfo(node2);

                    if (!elementInfo3.Success)
                        continue;

                    if (elementInfo3.IsEmptyElement)
                        continue;

                    if (elementInfo3.HasLocalName("term", "description"))
                        Analyze(context, elementInfo3);
                }
            }
        }

        private static void AnalyzeCElement(SyntaxNodeAnalysisContext context, XmlElementInfo elementInfo)
        {
            if (elementInfo.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)elementInfo.Element;

            SyntaxList<XmlNodeSyntax> content = element.Content;

            if (!content.Any())
                return;

            if (context.Node.SyntaxTree.IsMultiLineSpan(content.FullSpan))
                ReportDiagnostic(context, DiagnosticRules.FixDocumentationCommentTag, elementInfo.Element);
        }

        private static void AnalyzeCodeElement(SyntaxNodeAnalysisContext context, XmlElementInfo elementInfo)
        {
            if (elementInfo.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)elementInfo.Element;

            SyntaxList<XmlNodeSyntax> content = element.Content;

            if (!content.Any())
                return;

            if (context.Node.SyntaxTree.IsSingleLineSpan(content.FullSpan))
                ReportDiagnostic(context, DiagnosticRules.FixDocumentationCommentTag, elementInfo.Element);
        }
    }
}
