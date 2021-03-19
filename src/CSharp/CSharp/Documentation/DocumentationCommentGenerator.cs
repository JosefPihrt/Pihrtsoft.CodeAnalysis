// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Documentation;
using Roslynator.Text;

namespace Roslynator.CSharp.Documentation
{
    internal static class DocumentationCommentGenerator
    {
        private static readonly XmlReaderSettings _xmlReaderSettings = new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment };

        public static SyntaxTriviaList Generate(MemberDeclarationSyntax memberDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (memberDeclaration == null)
                throw new ArgumentNullException(nameof(memberDeclaration));

            switch (memberDeclaration.Kind())
            {
                case SyntaxKind.NamespaceDeclaration:
                    return Generate((NamespaceDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.ClassDeclaration:
                    return Generate((ClassDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.StructDeclaration:
                    return Generate((StructDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.InterfaceDeclaration:
                    return Generate((InterfaceDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.EnumDeclaration:
                    return Generate((EnumDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.DelegateDeclaration:
                    return Generate((DelegateDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.EnumMemberDeclaration:
                    return Generate((EnumMemberDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.FieldDeclaration:
                    return Generate((FieldDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.EventFieldDeclaration:
                    return Generate((EventFieldDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.MethodDeclaration:
                    return Generate((MethodDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.OperatorDeclaration:
                    return Generate((OperatorDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.ConversionOperatorDeclaration:
                    return Generate((ConversionOperatorDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.ConstructorDeclaration:
                    return Generate((ConstructorDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.DestructorDeclaration:
                    return Generate((DestructorDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.PropertyDeclaration:
                    return Generate((PropertyDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.EventDeclaration:
                    return Generate((EventDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.IndexerDeclaration:
                    return Generate((IndexerDeclarationSyntax)memberDeclaration, settings);
                case SyntaxKind.RecordDeclaration:
                    return Generate((RecordDeclarationSyntax)memberDeclaration, settings);
                default:
                    throw new ArgumentException("", nameof(memberDeclaration));
            }
        }

        public static SyntaxTriviaList Generate(NamespaceDeclarationSyntax namespaceDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (namespaceDeclaration == null)
                throw new ArgumentNullException(nameof(namespaceDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(ClassDeclarationSyntax classDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (classDeclaration == null)
                throw new ArgumentNullException(nameof(classDeclaration));

            return Generate(
                classDeclaration.TypeParameterList,
                default(ParameterListSyntax),
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(RecordDeclarationSyntax recordDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (recordDeclaration == null)
                throw new ArgumentNullException(nameof(recordDeclaration));

            return Generate(
                recordDeclaration.TypeParameterList,
                recordDeclaration.ParameterList,
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(StructDeclarationSyntax structDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (structDeclaration == null)
                throw new ArgumentNullException(nameof(structDeclaration));

            return Generate(
                structDeclaration.TypeParameterList,
                default(ParameterListSyntax),
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(InterfaceDeclarationSyntax interfaceDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (interfaceDeclaration == null)
                throw new ArgumentNullException(nameof(interfaceDeclaration));

            return Generate(
                interfaceDeclaration.TypeParameterList,
                default(ParameterListSyntax),
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(EnumDeclarationSyntax enumDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (enumDeclaration == null)
                throw new ArgumentNullException(nameof(enumDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(DelegateDeclarationSyntax delegateDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (delegateDeclaration == null)
                throw new ArgumentNullException(nameof(delegateDeclaration));

            return Generate(
                delegateDeclaration.TypeParameterList,
                delegateDeclaration.ParameterList,
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(EnumMemberDeclarationSyntax enumMemberDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (enumMemberDeclaration == null)
                throw new ArgumentNullException(nameof(enumMemberDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(FieldDeclarationSyntax fieldDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (fieldDeclaration == null)
                throw new ArgumentNullException(nameof(fieldDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(EventFieldDeclarationSyntax eventFieldDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (eventFieldDeclaration == null)
                throw new ArgumentNullException(nameof(eventFieldDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(MethodDeclarationSyntax methodDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (methodDeclaration == null)
                throw new ArgumentNullException(nameof(methodDeclaration));

            return Generate(
                methodDeclaration.TypeParameterList,
                methodDeclaration.ParameterList,
                canGenerateReturns: !methodDeclaration.ReturnsVoid(),
                settings: settings);
        }

        public static SyntaxTriviaList Generate(OperatorDeclarationSyntax operatorDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (operatorDeclaration == null)
                throw new ArgumentNullException(nameof(operatorDeclaration));

            return Generate(
                default(TypeParameterListSyntax),
                operatorDeclaration.ParameterList,
                canGenerateReturns: true,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(ConversionOperatorDeclarationSyntax conversionOperatorDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (conversionOperatorDeclaration == null)
                throw new ArgumentNullException(nameof(conversionOperatorDeclaration));

            return Generate(
                default(TypeParameterListSyntax),
                conversionOperatorDeclaration.ParameterList,
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(ConstructorDeclarationSyntax constructorDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (constructorDeclaration == null)
                throw new ArgumentNullException(nameof(constructorDeclaration));

            return Generate(
                default(TypeParameterListSyntax),
                constructorDeclaration.ParameterList,
                canGenerateReturns: false,
                settings: settings);
        }

        public static SyntaxTriviaList Generate(DestructorDeclarationSyntax destructorDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (destructorDeclaration == null)
                throw new ArgumentNullException(nameof(destructorDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(PropertyDeclarationSyntax propertyDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (propertyDeclaration == null)
                throw new ArgumentNullException(nameof(propertyDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(EventDeclarationSyntax eventDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (eventDeclaration == null)
                throw new ArgumentNullException(nameof(eventDeclaration));

            return Generate(settings: settings);
        }

        public static SyntaxTriviaList Generate(IndexerDeclarationSyntax indexerDeclaration, DocumentationCommentGeneratorSettings settings = null)
        {
            if (indexerDeclaration == null)
                throw new ArgumentNullException(nameof(indexerDeclaration));

            return Generate(
                default(TypeParameterListSyntax),
                indexerDeclaration.ParameterList,
                canGenerateReturns: true,
                settings: settings);
        }

        private static SyntaxTriviaList Generate(
            TypeParameterListSyntax typeParameterList,
            BaseParameterListSyntax parameterList,
            bool canGenerateReturns = false,
            DocumentationCommentGeneratorSettings settings = null)
        {
            SeparatedSyntaxList<TypeParameterSyntax> typeParameters = typeParameterList?.Parameters ?? default;

            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList?.Parameters ?? default;

            return Generate(typeParameters, parameters, canGenerateReturns, settings);
        }

        private static SyntaxTriviaList Generate(
            SeparatedSyntaxList<TypeParameterSyntax> typeParameters = default,
            SeparatedSyntaxList<ParameterSyntax> parameters = default,
            bool canGenerateReturns = false,
            DocumentationCommentGeneratorSettings settings = null)
        {
            settings ??= DocumentationCommentGeneratorSettings.Default;

            ImmutableArray<string> summary = settings.Summary;

            StringBuilder sb = StringBuilderCache.GetInstance();

            sb.Append(settings.Indentation);
            sb.Append("/// <summary>");

            if (settings.SingleLineSummary
                && summary.Length <= 1)
            {
                if (summary.Length == 1)
                    sb.Append(summary[0]);

                sb.AppendLine("</summary>");
            }
            else
            {
                sb.AppendLine();

                if (summary.Any())
                {
                    foreach (string comment in summary)
                    {
                        sb.Append(settings.Indentation);
                        sb.Append("/// ");
                        sb.AppendLine(comment);
                    }
                }
                else
                {
                    sb.Append(settings.Indentation);
                    sb.AppendLine("/// ");
                }

                sb.Append(settings.Indentation);
                sb.AppendLine("/// </summary>");
            }

            if (!settings.IsTagIgnored(WellKnownXmlTags.TypeParam))
            {
                foreach (TypeParameterSyntax typeParameter in typeParameters)
                {
                    sb.Append(settings.Indentation);
                    sb.Append("/// <typeparam name=\"");
                    sb.Append(typeParameter.Identifier.ValueText);
                    sb.AppendLine("\"></typeparam>");
                }
            }

            if (!settings.IsTagIgnored(WellKnownXmlTags.Param))
            {
                foreach (ParameterSyntax parameter in parameters)
                {
                    sb.Append(settings.Indentation);
                    sb.Append("/// <param name=\"");
                    sb.Append(parameter.Identifier.ValueText);
                    sb.AppendLine("\"></param>");
                }
            }

            if (canGenerateReturns
                && !settings.IsTagIgnored(WellKnownXmlTags.Returns))
            {
                sb.Append(settings.Indentation);
                sb.AppendLine("/// <returns></returns>");
            }

            return SyntaxFactory.ParseLeadingTrivia(StringBuilderCache.GetStringAndFree(sb));
        }

        internal static bool CanGenerateFromBase(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.IndexerDeclaration:
                case SyntaxKind.EventFieldDeclaration:
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.ConstructorDeclaration:
                    return true;
                default:
                    return false;
            }
        }

        internal static DocumentationCommentData GenerateFromBase(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            switch (memberDeclaration.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return GenerateFromBase((MethodDeclarationSyntax)memberDeclaration, semanticModel, cancellationToken);
                case SyntaxKind.PropertyDeclaration:
                    return GenerateFromBase((PropertyDeclarationSyntax)memberDeclaration, semanticModel, cancellationToken);
                case SyntaxKind.IndexerDeclaration:
                    return GenerateFromBase((IndexerDeclarationSyntax)memberDeclaration, semanticModel, cancellationToken);
                case SyntaxKind.EventFieldDeclaration:
                    return GenerateFromBase((EventFieldDeclarationSyntax)memberDeclaration, semanticModel, cancellationToken);
                case SyntaxKind.EventDeclaration:
                    return GenerateFromBase((EventDeclarationSyntax)memberDeclaration, semanticModel, cancellationToken);
                case SyntaxKind.ConstructorDeclaration:
                    return GenerateFromBase((ConstructorDeclarationSyntax)memberDeclaration, semanticModel, cancellationToken);
                default:
                    throw new ArgumentException("", nameof(memberDeclaration));
            }
        }

        internal static DocumentationCommentData GenerateFromBase(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            if (!methodDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword)
                && methodDeclaration.ExplicitInterfaceSpecifier == null
                && !ContainingTypeHasBaseType(methodDeclaration))
            {
                return default;
            }

            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

            if (methodSymbol?.IsErrorType() == false)
            {
                string xml = GenerateFromOverriddenMethods(methodSymbol, cancellationToken);

                if (xml != null)
                    return new DocumentationCommentData(xml, DocumentationCommentOrigin.BaseMember);

                xml = GenerateFromInterfaceMember<IMethodSymbol>(methodSymbol, cancellationToken);

                if (xml != null)
                    return new DocumentationCommentData(xml, DocumentationCommentOrigin.InterfaceMember);
            }

            return default;
        }

        internal static DocumentationCommentData GenerateFromBase(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            if (!propertyDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword)
                && propertyDeclaration.ExplicitInterfaceSpecifier == null
                && !ContainingTypeHasBaseType(propertyDeclaration))
            {
                return default;
            }

            IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken);

            return GenerateFromBase(propertySymbol, cancellationToken);
        }

        internal static DocumentationCommentData GenerateFromBase(IndexerDeclarationSyntax indexerDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            if (!indexerDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword)
                && indexerDeclaration.ExplicitInterfaceSpecifier == null
                && !ContainingTypeHasBaseType(indexerDeclaration))
            {
                return default;
            }

            IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken);

            return GenerateFromBase(propertySymbol, cancellationToken);
        }

        private static DocumentationCommentData GenerateFromBase(IPropertySymbol propertySymbol, CancellationToken cancellationToken)
        {
            if (propertySymbol?.IsErrorType() == false)
            {
                string xml = GenerateFromOverriddenProperties(propertySymbol, cancellationToken);

                if (xml != null)
                    return new DocumentationCommentData(xml, DocumentationCommentOrigin.BaseMember);

                xml = GenerateFromInterfaceMember<IPropertySymbol>(propertySymbol, cancellationToken);

                if (xml != null)
                    return new DocumentationCommentData(xml, DocumentationCommentOrigin.InterfaceMember);
            }

            return default;
        }

        internal static DocumentationCommentData GenerateFromBase(EventDeclarationSyntax eventDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            if (!eventDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword)
                && eventDeclaration.ExplicitInterfaceSpecifier == null
                && !ContainingTypeHasBaseType(eventDeclaration))
            {
                return default;
            }

            IEventSymbol eventSymbol = semanticModel.GetDeclaredSymbol(eventDeclaration, cancellationToken);

            return GenerateFromBase(eventSymbol, cancellationToken);
        }

        internal static DocumentationCommentData GenerateFromBase(EventFieldDeclarationSyntax eventFieldDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            if (!eventFieldDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword)
                && !ContainingTypeHasBaseType(eventFieldDeclaration))
            {
                return default;
            }

            VariableDeclaratorSyntax variableDeclarator = eventFieldDeclaration.Declaration?.Variables.FirstOrDefault();

            if (variableDeclarator != null)
            {
                var eventSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken) as IEventSymbol;

                return GenerateFromBase(eventSymbol, cancellationToken);
            }

            return default;
        }

        private static DocumentationCommentData GenerateFromBase(IEventSymbol eventSymbol, CancellationToken cancellationToken)
        {
            if (eventSymbol?.IsErrorType() == false)
            {
                string xml = GenerateFromOverriddenEvents(eventSymbol, cancellationToken);

                if (xml != null)
                    return new DocumentationCommentData(xml, DocumentationCommentOrigin.BaseMember);

                xml = GenerateFromInterfaceMember<IEventSymbol>(eventSymbol, cancellationToken);

                if (xml != null)
                    return new DocumentationCommentData(xml, DocumentationCommentOrigin.InterfaceMember);
            }

            return default;
        }

        internal static DocumentationCommentData GenerateFromBase(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken = default)
        {
            ConstructorInitializerSyntax initializer = constructorDeclaration.Initializer;

            if (initializer?.Kind() == SyntaxKind.BaseConstructorInitializer)
            {
                ISymbol baseConstructor = semanticModel.GetSymbol(initializer, cancellationToken);

                if (baseConstructor?.IsErrorType() == false)
                {
                    string xml = GetDocumentationCommentXml(baseConstructor, cancellationToken);

                    if (xml != null)
                        return new DocumentationCommentData(xml, DocumentationCommentOrigin.BaseMember);
                }
            }

            return default;
        }

        private static string GenerateFromOverriddenMethods(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
        {
            for (IMethodSymbol overriddenMethod = methodSymbol.OverriddenMethod; overriddenMethod != null; overriddenMethod = overriddenMethod.OverriddenMethod)
            {
                string xml = GetDocumentationCommentXml(overriddenMethod, cancellationToken);

                if (xml != null)
                    return xml;
            }

            return null;
        }

        private static string GenerateFromOverriddenProperties(IPropertySymbol propertySymbol, CancellationToken cancellationToken)
        {
            for (IPropertySymbol overriddenProperty = propertySymbol.OverriddenProperty; overriddenProperty != null; overriddenProperty = overriddenProperty.OverriddenProperty)
            {
                string xml = GetDocumentationCommentXml(overriddenProperty, cancellationToken);

                if (xml != null)
                    return xml;
            }

            return null;
        }

        private static string GenerateFromOverriddenEvents(IEventSymbol eventSymbol, CancellationToken cancellationToken)
        {
            for (IEventSymbol overriddenEvent = eventSymbol.OverriddenEvent; overriddenEvent != null; overriddenEvent = overriddenEvent.OverriddenEvent)
            {
                string xml = GetDocumentationCommentXml(overriddenEvent, cancellationToken);

                if (xml != null)
                    return xml;
            }

            return null;
        }

        private static string GenerateFromInterfaceMember<TInterfaceSymbol>(
            ISymbol memberSymbol,
            CancellationToken cancellationToken) where TInterfaceSymbol : ISymbol
        {
            TInterfaceSymbol interfaceMember = memberSymbol.FindFirstImplementedInterfaceMember<TInterfaceSymbol>();

            if (!EqualityComparer<TInterfaceSymbol>.Default.Equals(interfaceMember, default))
            {
                return GetDocumentationCommentXml(interfaceMember, cancellationToken);
            }
            else
            {
                return null;
            }
        }

        private static string GetDocumentationCommentXml(ISymbol symbol, CancellationToken cancellationToken = default)
        {
            string xml = symbol.GetDocumentationCommentXml(cancellationToken: cancellationToken);

            if (xml == null)
                return null;

            using (var sr = new StringReader(xml))
            {
                using (XmlReader reader = XmlReader.Create(sr, _xmlReaderSettings))
                {
                    if (reader.Read()
                        && reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "member":
                            case "doc":
                                {
                                    try
                                    {
                                        return reader.ReadInnerXml();
                                    }
                                    catch (XmlException ex)
                                    {
                                        Debug.Fail(symbol.ToDisplayString() + "\r\n\r\n" + ex.ToString());
                                        return null;
                                    }
                                }
                            default:
                                {
                                    Debug.Fail(reader.Name);
                                    return null;
                                }
                        }
                    }
                }
            }

            return null;
        }

        private static bool ContainingTypeHasBaseType(MemberDeclarationSyntax memberDeclaration)
        {
            SyntaxNode parent = memberDeclaration.Parent;

            switch (parent.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    return ((ClassDeclarationSyntax)parent).BaseList?.Types.Any() == true;
                case SyntaxKind.RecordDeclaration:
                    return ((RecordDeclarationSyntax)parent).BaseList?.Types.Any() == true;
                case SyntaxKind.StructDeclaration:
                    return ((StructDeclarationSyntax)parent).BaseList?.Types.Any() == true;
            }

            return false;
        }
    }
}
