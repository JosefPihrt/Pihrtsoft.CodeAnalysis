// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static Roslynator.CSharp.Analysis.ConvertCommentToDocumentationCommentAnalysis;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConvertCommentToDocumentationCommentAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ConvertCommentToDocumentationComment);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeNamespaceDeclaration(f), SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeClassDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeStructDeclaration(f), SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeInterfaceDeclaration(f), SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEnumDeclaration(f), SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeDelegateDeclaration(f), SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeConstructorDeclaration(f), SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeDestructorDeclaration(f), SyntaxKind.DestructorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeOperatorDeclaration(f), SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeConversionOperatorDeclaration(f), SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeIndexerDeclaration(f), SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeFieldDeclaration(f), SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEventFieldDeclaration(f), SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEventDeclaration(f), SyntaxKind.EventDeclaration);
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(namespaceDeclaration.Name);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(classDeclaration.Identifier)
                ?? AnalyzeTrailing(classDeclaration.TypeParameterList)
                ?? AnalyzeTrailing(classDeclaration.BaseList)
                ?? AnalyzeTrailing(classDeclaration.ConstraintClauses);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var structDeclaration = (StructDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(structDeclaration.Identifier)
                ?? AnalyzeTrailing(structDeclaration.TypeParameterList)
                ?? AnalyzeTrailing(structDeclaration.BaseList)
                ?? AnalyzeTrailing(structDeclaration.ConstraintClauses);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(interfaceDeclaration.Identifier)
                ?? AnalyzeTrailing(interfaceDeclaration.TypeParameterList)
                ?? AnalyzeTrailing(interfaceDeclaration.BaseList)
                ?? AnalyzeTrailing(interfaceDeclaration.ConstraintClauses);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            AnalyzeEnumMembers(context, enumDeclaration.Members);

            if (AnalyzeLeading(context))
                return;

            TrailingAnalysis? analysis = AnalyzeTrailing(enumDeclaration.Identifier);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeEnumMembers(SyntaxNodeAnalysisContext context, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            int count = members.Count;
            int separatorCount = members.SeparatorCount;

            for (int i = 0; i < count; i++)
            {
                EnumMemberDeclarationSyntax enumMember = members[i];

                if (AnalyzeLeading(context, enumMember))
                    continue;

                TrailingAnalysis? analysis = AnalyzeTrailing(enumMember);

                if (analysis == null
                    && (separatorCount == count || i < count - 1))
                {
                    analysis = AnalyzeTrailing(members.GetSeparator(i));
                }

                ReportDiagnostic(context, analysis);
            }
        }

        private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(delegateDeclaration.ParameterList)
                ?? AnalyzeTrailing(delegateDeclaration.ConstraintClauses)
                ?? AnalyzeTrailing(delegateDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(methodDeclaration.ParameterList)
                ?? AnalyzeTrailing(methodDeclaration.ConstraintClauses)
                ?? AnalyzeTrailing(methodDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(methodDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(constructorDeclaration.ParameterList)
                ?? AnalyzeTrailing(constructorDeclaration.Initializer)
                ?? AnalyzeTrailing(constructorDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(constructorDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeDestructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var destructorDeclaration = (DestructorDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(destructorDeclaration.ParameterList)
                ?? AnalyzeTrailing(destructorDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(destructorDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(operatorDeclaration.ParameterList)
                ?? AnalyzeTrailing(operatorDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(operatorDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(conversionOperatorDeclaration.ParameterList)
                ?? AnalyzeTrailing(conversionOperatorDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(conversionOperatorDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(propertyDeclaration.Identifier)
                ?? AnalyzeTrailing(propertyDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(propertyDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(indexerDeclaration.ParameterList)
                ?? AnalyzeTrailing(indexerDeclaration.ExpressionBody?.ArrowToken)
                ?? AnalyzeTrailing(indexerDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(fieldDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeEventFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var eventFieldDeclaration = (EventFieldDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(eventFieldDeclaration);

            ReportDiagnostic(context, analysis);
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzeLeading(context))
                return;

            var eventDeclaration = (EventDeclarationSyntax)context.Node;

            TrailingAnalysis? analysis = AnalyzeTrailing(eventDeclaration.Identifier)
                ?? AnalyzeTrailing(eventDeclaration.AccessorList);

            ReportDiagnostic(context, analysis);
        }

        private static bool AnalyzeLeading(SyntaxNodeAnalysisContext context)
        {
            return AnalyzeLeading(context, context.Node);
        }

        private static bool AnalyzeLeading(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            LeadingAnalysis analysis = AnalyzeLeadingTrivia(node);

            if (analysis.HasDocumentationComment)
                return true;

            if (analysis.ContainsTaskListItem)
            {
                if (analysis.ContainsNonTaskListItem)
                    return true;
            }
            else if (!analysis.Span.IsEmpty)
            {
                ReportDiagnostic(context, analysis.Span);
                return true;
            }

            return false;
        }

        private static TrailingAnalysis? AnalyzeTrailing(SyntaxNodeOrToken? nodeOrToken)
        {
            return (nodeOrToken != null) ? AnalyzeTrailing(nodeOrToken.Value) : default;
        }

        private static TrailingAnalysis? AnalyzeTrailing(SyntaxNodeOrToken nodeOrToken)
        {
            TrailingAnalysis analysis = AnalyzeTrailingTrivia(nodeOrToken);

            if (analysis.ContainsEndOfLine)
                return analysis;

            if (!analysis.Span.IsEmpty)
                return analysis;

            return null;
        }

        private static TrailingAnalysis? AnalyzeTrailing<TNode>(SyntaxList<TNode> nodes) where TNode : SyntaxNode
        {
            foreach (TNode node in nodes)
            {
                TrailingAnalysis? analysis = AnalyzeTrailing(node);

                if (analysis != null)
                    return analysis;
            }

            return null;
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, TrailingAnalysis? analysis)
        {
            if (analysis?.Span.IsEmpty == false)
                ReportDiagnostic(context, analysis.Value.Span);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, TextSpan span)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ConvertCommentToDocumentationComment,
                Location.Create(context.Node.SyntaxTree, span));
        }
    }
}

