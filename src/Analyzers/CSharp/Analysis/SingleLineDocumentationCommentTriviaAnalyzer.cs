// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;
using static Roslynator.DiagnosticHelpers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SingleLineDocumentationCommentTriviaAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnosticsWithoutFadeOut = ImmutableArray.Create(
            DiagnosticRules.AddSummaryToDocumentationComment,
            DiagnosticRules.AddSummaryElementToDocumentationComment,
            DiagnosticRules.AddParamElementToDocumentationComment,
            DiagnosticRules.AddTypeParamElementToDocumentationComment,
            DiagnosticRules.UnusedElementInDocumentationComment,
            DiagnosticRules.OrderElementsInDocumentationComment,
            DiagnosticRules.FixDocumentationCommentTag);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _supportedDiagnostics, _supportedDiagnosticsWithoutFadeOut.Add(DiagnosticRules.UnusedElementInDocumentationCommentFadeOut));

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (IsAnyEffective(c, _supportedDiagnosticsWithoutFadeOut))
                        AnalyzeSingleLineDocumentationCommentTrivia(c);
                },
                SyntaxKind.SingleLineDocumentationCommentTrivia);
        }

        private static void AnalyzeSingleLineDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
        {
            var documentationComment = (DocumentationCommentTriviaSyntax)context.Node;

            if (!documentationComment.IsPartOfMemberDeclaration())
                return;

            bool? useCorrectDocumentationTagEnabled = null;
            var containsInheritDoc = false;
            var containsIncludeOrExclude = false;
            var containsSummaryElement = false;
            var containsContentElement = false;
            var isFirst = true;

            CancellationToken cancellationToken = context.CancellationToken;

            SyntaxList<XmlNodeSyntax> content = documentationComment.Content;

            for (int i = 0; i < content.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                XmlElementInfo info = SyntaxInfo.XmlElementInfo(content[i]);

                if (info.Success)
                {
                    switch (info.GetTag())
                    {
                        case XmlTag.Include:
                        case XmlTag.Exclude:
                            {
                                if (isFirst)
                                    containsIncludeOrExclude = true;

                                break;
                            }
                        case XmlTag.InheritDoc:
                            {
                                containsInheritDoc = true;
                                break;
                            }
                        case XmlTag.Content:
                            {
                                containsContentElement = true;
                                break;
                            }
                        case XmlTag.Summary:
                            {
                                if (info.IsContentEmptyOrWhitespace)
                                    ReportDiagnosticIfEffective(context, DiagnosticRules.AddSummaryToDocumentationComment, info.Element);

                                containsSummaryElement = true;

                                if (useCorrectDocumentationTagEnabled ??= DiagnosticRules.FixDocumentationCommentTag.IsEffective(context))
                                    FixDocumentationCommentTagAnalysis.Analyze(context, info);

                                break;
                            }
                        case XmlTag.Example:
                        case XmlTag.Remarks:
                        case XmlTag.Returns:
                        case XmlTag.Value:
                            {
                                if (info.IsContentEmptyOrWhitespace)
                                    ReportUnusedElement(context, info.Element, i, content);

                                if (useCorrectDocumentationTagEnabled ??= DiagnosticRules.FixDocumentationCommentTag.IsEffective(context))
                                    FixDocumentationCommentTagAnalysis.Analyze(context, info);

                                break;
                            }
                        case XmlTag.Exception:
                        case XmlTag.List:
                        case XmlTag.Param:
                        case XmlTag.Permission:
                        case XmlTag.TypeParam:
                            {
                                if (useCorrectDocumentationTagEnabled ??= DiagnosticRules.FixDocumentationCommentTag.IsEffective(context))
                                    FixDocumentationCommentTagAnalysis.Analyze(context, info);

                                break;
                            }
                        case XmlTag.C:
                        case XmlTag.Code:
                        case XmlTag.Para:
                        case XmlTag.ParamRef:
                        case XmlTag.See:
                        case XmlTag.SeeAlso:
                        case XmlTag.TypeParamRef:
                            {
                                break;
                            }
                        default:
                            {
                                Debug.Fail(info.GetTag().ToString());
                                break;
                            }
                    }

                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        containsIncludeOrExclude = false;
                    }
                }
            }

            if (containsInheritDoc
                || containsIncludeOrExclude)
            {
                return;
            }

            if (!containsSummaryElement
                && !containsContentElement)
            {
                ReportDiagnosticIfEffective(context, DiagnosticRules.AddSummaryElementToDocumentationComment, documentationComment);
            }

            SyntaxNode parent = documentationComment.ParentTrivia.Token.Parent;

            bool unusedElement = DiagnosticRules.UnusedElementInDocumentationComment.IsEffective(context);
            bool orderParams = DiagnosticRules.OrderElementsInDocumentationComment.IsEffective(context);
            bool addParam = DiagnosticRules.AddParamElementToDocumentationComment.IsEffective(context);
            bool addTypeParam = DiagnosticRules.AddTypeParamElementToDocumentationComment.IsEffective(context);

            if (addParam
                || orderParams
                || unusedElement)
            {
                SeparatedSyntaxList<ParameterSyntax> parameters = CSharpUtility.GetParameters(
                    (parent is MemberDeclarationSyntax) ? parent : parent.Parent);

                if (addParam
                    && parameters.Any())
                {
                    foreach (ParameterSyntax parameter in parameters)
                    {
                        if (IsMissing(documentationComment, parameter))
                        {
                            ReportDiagnostic(context, DiagnosticRules.AddParamElementToDocumentationComment, documentationComment);
                            break;
                        }
                    }
                }

                if (orderParams || unusedElement)
                {
                    Analyze(context, documentationComment.Content, parameters, XmlTag.Param, (nodes, name) => nodes.IndexOf(name));
                }
            }

            if (addTypeParam
                || orderParams
                || unusedElement)
            {
                SeparatedSyntaxList<TypeParameterSyntax> typeParameters = CSharpUtility.GetTypeParameters(
                    (parent is MemberDeclarationSyntax) ? parent : parent.Parent);

                if (addTypeParam
                    && typeParameters.Any())
                {
                    foreach (TypeParameterSyntax typeParameter in typeParameters)
                    {
                        if (IsMissing(documentationComment, typeParameter))
                        {
                            ReportDiagnostic(context, DiagnosticRules.AddTypeParamElementToDocumentationComment, documentationComment);
                            break;
                        }
                    }
                }

                if (orderParams || unusedElement)
                {
                    Analyze(context, documentationComment.Content, typeParameters, XmlTag.TypeParam, (nodes, name) => nodes.IndexOf(name));
                }
            }
        }

        private static bool IsMissing(DocumentationCommentTriviaSyntax documentationComment, ParameterSyntax parameter)
        {
            foreach (XmlNodeSyntax xmlNode in documentationComment.Content)
            {
                XmlElementInfo elementInfo = SyntaxInfo.XmlElementInfo(xmlNode);

                if (elementInfo.Success
                    && !elementInfo.IsEmptyElement
                    && elementInfo.HasTag(XmlTag.Param))
                {
                    var element = (XmlElementSyntax)elementInfo.Element;

                    string value = element.GetAttributeValue("name");

                    if (value != null
                        && string.Equals(parameter.Identifier.ValueText, value, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsMissing(DocumentationCommentTriviaSyntax documentationComment, TypeParameterSyntax typeParameter)
        {
            foreach (XmlNodeSyntax xmlNode in documentationComment.Content)
            {
                XmlElementInfo elementInfo = SyntaxInfo.XmlElementInfo(xmlNode);

                if (elementInfo.Success
                    && !elementInfo.IsEmptyElement
                    && elementInfo.HasTag(XmlTag.TypeParam))
                {
                    var element = (XmlElementSyntax)elementInfo.Element;

                    string value = element.GetAttributeValue("name");

                    if (value != null
                        && string.Equals(typeParameter.Identifier.ValueText, value, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void Analyze<TNode>(
            SyntaxNodeAnalysisContext context,
            SyntaxList<XmlNodeSyntax> xmlNodes,
            SeparatedSyntaxList<TNode> nodes,
            XmlTag tag,
            Func<SeparatedSyntaxList<TNode>, string, int> indexOf) where TNode : SyntaxNode
        {
            XmlNodeSyntax firstElement = null;

            int firstIndex = -1;

            for (int i = 0; i < xmlNodes.Count; i++)
            {
                XmlElementInfo elementInfo = SyntaxInfo.XmlElementInfo(xmlNodes[i]);

                if (!elementInfo.Success)
                    continue;

                if (!elementInfo.HasTag(tag))
                {
                    firstIndex = -1;
                    continue;
                }

                XmlNodeSyntax element = elementInfo.Element;

                string name = (element.IsKind(SyntaxKind.XmlElement))
                    ? ((XmlElementSyntax)element).GetAttributeValue("name")
                    : ((XmlEmptyElementSyntax)element).GetAttributeValue("name");

                if (name == null)
                {
                    firstIndex = -1;
                    continue;
                }

                int index = indexOf(nodes, name);

                if (index == -1)
                {
                    ReportUnusedElement(context, element, i, xmlNodes);
                }
                else if (index < firstIndex)
                {
                    ReportDiagnosticIfEffective(context, DiagnosticRules.OrderElementsInDocumentationComment, firstElement);
                    return;
                }
                else
                {
                    firstElement = element;
                }

                firstIndex = index;
            }
        }

        private static void ReportUnusedElement(
            SyntaxNodeAnalysisContext context,
            XmlNodeSyntax xmlNode,
            int index,
            SyntaxList<XmlNodeSyntax> xmlNodes)
        {
            if (!DiagnosticRules.UnusedElementInDocumentationComment.IsEffective(context))
                return;

            ReportDiagnostic(context, DiagnosticRules.UnusedElementInDocumentationComment, xmlNode);

            if (index > 0
                && xmlNodes[index - 1] is XmlTextSyntax xmlText)
            {
                SyntaxTokenList tokens = xmlText.TextTokens;

                if (tokens.Count == 1)
                {
                    if (tokens[0].IsKind(SyntaxKind.XmlTextLiteralToken))
                    {
                        SyntaxTrivia trivia = tokens[0].LeadingTrivia.SingleOrDefault(shouldThrow: false);

                        if (trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia))
                            ReportDiagnostic(context, DiagnosticRules.UnusedElementInDocumentationCommentFadeOut, trivia);
                    }
                }
                else if (tokens.Count == 2)
                {
                    if (tokens[0].IsKind(SyntaxKind.XmlTextLiteralNewLineToken)
                        && tokens[1].IsKind(SyntaxKind.XmlTextLiteralToken))
                    {
                        SyntaxTrivia trivia = tokens[1].LeadingTrivia.SingleOrDefault(shouldThrow: false);

                        if (trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia))
                            ReportDiagnostic(context, DiagnosticRules.UnusedElementInDocumentationCommentFadeOut, trivia);
                    }
                }
            }
        }
    }
}
