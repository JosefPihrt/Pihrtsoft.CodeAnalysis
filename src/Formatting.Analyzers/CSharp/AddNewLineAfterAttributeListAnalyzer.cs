// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddNewLineAfterAttributeListAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddNewLineAfterAttributeList);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeBaseTypeDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseMethodDeclaration(f), SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseMethodDeclaration(f), SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeDelegateDeclaration(f), SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseMethodDeclaration(f), SyntaxKind.DestructorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseTypeDeclaration(f), SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEnumMemberDeclaration(f), SyntaxKind.EnumMemberDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBasePropertyDeclaration(f), SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseFieldDeclaration(f), SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseFieldDeclaration(f), SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBasePropertyDeclaration(f), SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseTypeDeclaration(f), SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseMethodDeclaration(f), SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBasePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeBaseTypeDeclaration(f), SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.SetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.InitAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.AddAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.RemoveAccessorDeclaration);
        }

        private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = delegateDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxTokenList modifiers = delegateDeclaration.Modifiers;

            SyntaxToken token = (modifiers.Any())
                ? modifiers[0]
                : delegateDeclaration.DelegateKeyword;

            Analyze(context, attributeList, token);
        }

        private static void AnalyzeEnumMemberDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumMemberDeclaration = (EnumMemberDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = enumMemberDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxToken token = enumMemberDeclaration.Identifier;

            Analyze(context, attributeList, token);
        }

        private static void AnalyzeBaseFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var baseFieldDeclaration = (BaseFieldDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = baseFieldDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxTokenList modifiers = baseFieldDeclaration.Modifiers;

            SyntaxToken token;

            if (modifiers.Any())
            {
                token = modifiers[0];
            }
            else if (baseFieldDeclaration is EventFieldDeclarationSyntax eventFieldDeclaration)
            {
                token = eventFieldDeclaration.EventKeyword;
            }
            else
            {
                token = attributeList.CloseBracketToken.GetNextToken();
            }

            Analyze(context, attributeList, token);
        }

        private static void AnalyzeBaseMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var baseMethodDeclaration = (BaseMethodDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = baseMethodDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxTokenList modifiers = baseMethodDeclaration.Modifiers;

            SyntaxToken token;

            if (modifiers.Any())
            {
                token = modifiers[0];
            }
            else if (baseMethodDeclaration is ConstructorDeclarationSyntax constructorDeclaration)
            {
                token = constructorDeclaration.Identifier;
            }
            else if (baseMethodDeclaration is ConversionOperatorDeclarationSyntax conversionOperatorDeclaration)
            {
                token = conversionOperatorDeclaration.ImplicitOrExplicitKeyword;
            }
            else if (baseMethodDeclaration is DestructorDeclarationSyntax destructorDeclaration)
            {
                token = destructorDeclaration.TildeToken;
            }
            else
            {
                token = attributeList.CloseBracketToken.GetNextToken();
            }

            Analyze(context, attributeList, token);
        }

        private static void AnalyzeBasePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var basePropertyDeclaration = (BasePropertyDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = basePropertyDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxTokenList modifiers = basePropertyDeclaration.Modifiers;

            SyntaxToken token = (modifiers.Any())
                ? modifiers[0]
                : attributeList.CloseBracketToken.GetNextToken();

            Analyze(context, attributeList, token);
        }

        private static void AnalyzeBaseTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var baseTypeDeclaration = (BaseTypeDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = baseTypeDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxTokenList modifiers = baseTypeDeclaration.Modifiers;

            SyntaxToken token;

            if (modifiers.Any())
            {
                token = modifiers[0];
            }
            else if (baseTypeDeclaration is ClassDeclarationSyntax classDeclaration)
            {
                token = classDeclaration.Keyword;
            }
            else if (baseTypeDeclaration is InterfaceDeclarationSyntax interfaceDeclaration)
            {
                token = interfaceDeclaration.Keyword;
            }
            else if (baseTypeDeclaration is StructDeclarationSyntax structDeclaration)
            {
                token = structDeclaration.Keyword;
            }
            else if (baseTypeDeclaration is EnumDeclarationSyntax enumDeclaration)
            {
                token = enumDeclaration.EnumKeyword;
            }
            else
            {
                throw new InvalidOperationException("");
            }

            Analyze(context, attributeList, token);
        }

        private static void AnalyzeAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var accessorDeclaration = (AccessorDeclarationSyntax)context.Node;

            AttributeListSyntax attributeList = accessorDeclaration.AttributeLists.LastOrDefault();

            if (attributeList == null)
                return;

            SyntaxTokenList modifiers = accessorDeclaration.Modifiers;

            SyntaxToken token = (modifiers.Any())
                ? modifiers[0]
                : accessorDeclaration.Keyword;

            Analyze(context, attributeList, token);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, AttributeListSyntax attributeList, SyntaxToken token)
        {
            if (attributeList.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(attributeList.Span.End, token.SpanStart)))
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.AddNewLineAfterAttributeList,
                    Location.Create(token.SyntaxTree, token.Span.WithLength(0)));
            }
        }
    }
}
