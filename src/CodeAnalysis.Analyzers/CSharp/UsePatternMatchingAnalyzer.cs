// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UsePatternMatchingAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableHashSet<string> _syntaxKindNames;
        private static ImmutableHashSet<string> _syntaxTypeNames;
        private static ImmutableDictionary<ushort, string> _syntaxKindValuesToNames;

        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UsePatternMatching);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (_syntaxKindNames == null)
                {
                    Compilation compilation = startContext.Compilation;

                    INamedTypeSymbol csharpSyntaxNodeSymbol = compilation.GetTypeByMetadataName("Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode");

                    if (csharpSyntaxNodeSymbol == null)
                        return;

                    Dictionary<string, string> kindsToNames = compilation
                        .GetTypeByMetadataName("Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax")
                        .ContainingNamespace
                        .GetTypeMembers()
                        .Where(f => f.TypeKind == TypeKind.Class
                            && !f.IsAbstract
                            && f.InheritsFrom(csharpSyntaxNodeSymbol)
                            && f.Name.EndsWith("Syntax", StringComparison.Ordinal))
                        .ToDictionary(f => f.Name.Remove(f.Name.Length - 6), f => f.Name);

                    ImmutableHashSet<string>.Builder syntaxKindNames = ImmutableHashSet.CreateBuilder<string>();
                    ImmutableHashSet<string>.Builder syntaxTypeNames = ImmutableHashSet.CreateBuilder<string>();
                    ImmutableDictionary<ushort, string>.Builder syntaxKindValuesToNames = ImmutableDictionary.CreateBuilder<ushort, string>();

                    foreach (SyntaxKind syntaxKind in Enum.GetValues(typeof(SyntaxKind))
                        .Cast<SyntaxKind>()
                        .Select(f => f))
                    {
                        string name = syntaxKind.ToString();

                        if (kindsToNames.TryGetValue(name, out string symbolName))
                        {
                            syntaxKindNames.Add(name);
                            syntaxTypeNames.Add(symbolName);
                        }

                        syntaxKindValuesToNames.Add((ushort)syntaxKind, name);
                    }

                    Interlocked.CompareExchange(ref _syntaxKindNames, syntaxKindNames.ToImmutable(), null);
                    Interlocked.CompareExchange(ref _syntaxTypeNames, syntaxTypeNames.ToImmutable(), null);
                    Interlocked.CompareExchange(ref _syntaxKindValuesToNames, syntaxKindValuesToNames.ToImmutable(), null);
                }

                startContext.RegisterSyntaxNodeAction(f => AnalyzeSwitchStatement(f), SyntaxKind.SwitchStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
            });
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;

            SyntaxList<SwitchSectionSyntax> sections = switchStatement.Sections;

            if (!sections.Any())
                return;

            ExpressionSyntax switchExpression = switchStatement.Expression;

            SingleLocalDeclarationStatementInfo localInfo = default;

            string name = GetName();

            if (name == null)
                return;

            ITypeSymbol kindSymbol = context.SemanticModel.GetTypeSymbol(switchExpression, context.CancellationToken);

            if (kindSymbol?.HasMetadataName(CSharpMetadataNames.Microsoft_CodeAnalysis_CSharp_SyntaxKind) != true)
                return;

            foreach (SwitchSectionSyntax section in sections)
            {
                SwitchLabelSyntax label = section.Labels.SingleOrDefault(shouldThrow: false);

                if (label == null)
                    return;

                SyntaxKind labelKind = label.Kind();

                if (labelKind == SyntaxKind.DefaultSwitchLabel)
                    continue;

                if (labelKind != SyntaxKind.CaseSwitchLabel)
                {
                    Debug.Assert(labelKind == SyntaxKind.CasePatternSwitchLabel, labelKind.ToString());
                    return;
                }

                var caseLabel = (CaseSwitchLabelSyntax)label;

                ExpressionSyntax value = caseLabel.Value;

                if (!value.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    return;

                var memberAccess = (MemberAccessExpressionSyntax)value;

                if (!(memberAccess.Name is IdentifierNameSyntax identifierName))
                    return;

                string kindName = identifierName.Identifier.ValueText;

                if (!_syntaxKindNames.Contains(kindName))
                    return;

                SyntaxList<StatementSyntax> statements = section.Statements;

                StatementSyntax statement = statements.FirstOrDefault();

                if (statement == null)
                    return;

                if (statement is BlockSyntax block)
                {
                    statement = block.Statements.FirstOrDefault();
                }

                if (!statement.IsKind(SyntaxKind.LocalDeclarationStatement))
                    return;

                SingleLocalDeclarationStatementInfo localStatement = SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)statement);

                if (!localStatement.Success)
                    return;

                if (!(localStatement.Value is CastExpressionSyntax castExpression))
                    return;

                if (!(castExpression.Expression is IdentifierNameSyntax localName))
                    return;

                if (name != localName.Identifier.ValueText)
                    return;

                if (!IsFixableSyntaxSymbol(castExpression.Type, kindName, context.SemanticModel, context.CancellationToken))
                    return;
            }

            if (localInfo.Success
                && IsLocalVariableReferenced(context, localInfo, switchStatement))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePatternMatching, switchStatement.SwitchKeyword);

            string GetName()
            {
                switch (switchExpression.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        {
                            StatementSyntax previousStatement = switchStatement.PreviousStatement();

                            if (!previousStatement.IsKind(SyntaxKind.LocalDeclarationStatement))
                                return null;

                            localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)previousStatement);

                            if (!localInfo.Success)
                                return null;

                            if (localInfo.IdentifierText != ((IdentifierNameSyntax)switchExpression).Identifier.ValueText)
                                return null;

                            if (!localInfo.Value.IsKind(SyntaxKind.InvocationExpression))
                                return null;

                            return GetName2((InvocationExpressionSyntax)localInfo.Value);
                        }
                    case SyntaxKind.InvocationExpression:
                        {
                            return GetName2((InvocationExpressionSyntax)switchExpression);
                        }
                    default:
                        {
                            return null;
                        }
                }
            }

            static string GetName2(InvocationExpressionSyntax invocationExpression)
            {
                SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

                if (!invocationInfo.Success)
                    return null;

                if (invocationInfo.Arguments.Any())
                    return null;

                if (invocationInfo.NameText != "Kind")
                    return null;

                if (!(invocationInfo.Expression is IdentifierNameSyntax identifierName))
                    return null;

                return identifierName.Identifier.ValueText;
            }
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            IsKindExpressionInfo isKindExpression = IsKindExpressionInfo.Create(ifStatement.Condition, semanticModel, cancellationToken: cancellationToken);

            if (!isKindExpression.Success)
                return;

            Optional<object> optionalConstantValue = semanticModel.GetConstantValue(isKindExpression.KindExpression, cancellationToken);

            if (!optionalConstantValue.HasValue)
                return;

            if (!(optionalConstantValue.Value is ushort value))
                return;

            if (!_syntaxKindValuesToNames.TryGetValue(value, out string name))
                return;

            if (!_syntaxKindNames.Contains(name))
                return;

            switch (isKindExpression.Style)
            {
                case IsKindExpressionStyle.IsKind:
                case IsKindExpressionStyle.IsKindConditional:
                case IsKindExpressionStyle.Kind:
                case IsKindExpressionStyle.KindConditional:
                    {
                        if (!(ifStatement.Statement is BlockSyntax block))
                            return;

                        Analyze(block.Statements.FirstOrDefault());
                        break;
                    }
                case IsKindExpressionStyle.NotIsKind:
                case IsKindExpressionStyle.NotIsKindConditional:
                case IsKindExpressionStyle.NotKind:
                case IsKindExpressionStyle.NotKindConditional:
                    {
                        if (ifStatement.Else != null)
                            return;

                        StatementSyntax statement = ifStatement.Statement.SingleNonBlockStatementOrDefault();

                        if (statement == null)
                            return;

                        if (!CSharpFacts.IsJumpStatement(statement.Kind()))
                            return;

                        Analyze(ifStatement.NextStatement());
                        break;
                    }
            }

            void Analyze(StatementSyntax statement)
            {
                SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo(statement);

                if (!localInfo.Success)
                    return;

                if (!(localInfo.Value is CastExpressionSyntax castExpression))
                    return;

                if (!IsFixableSyntaxSymbol(castExpression.Type, name, semanticModel, cancellationToken))
                    return;

                if (!CSharpFactory.AreEquivalent(isKindExpression.Expression, castExpression.Expression))
                    return;

                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePatternMatching, ifStatement.IfKeyword);
            }
        }

        private static bool IsFixableSyntaxSymbol(
            TypeSyntax type,
            string kindName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ITypeSymbol syntaxSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

            if (syntaxSymbol.IsErrorType())
                return false;

            string syntaxName = syntaxSymbol.Name;

            if (!_syntaxTypeNames.Contains(syntaxName))
                return false;

            if (kindName.Length != syntaxName.Length - 6)
                return false;

            if (string.Compare(kindName, 0, syntaxName, 0, kindName.Length, StringComparison.Ordinal) != 0)
                return false;

            if (!syntaxSymbol.InheritsFrom(CSharpMetadataNames.Microsoft_CodeAnalysis_CSharp_CSharpSyntaxNode))
                return false;

            return true;
        }

        private static bool IsLocalVariableReferenced(
            SyntaxNodeAnalysisContext context,
            SingleLocalDeclarationStatementInfo localInfo,
            SwitchStatementSyntax switchStatement)
        {
            ISymbol localSymbol = context.SemanticModel.GetDeclaredSymbol(localInfo.Declarator, context.CancellationToken);

            if (localSymbol.IsKind(SymbolKind.Local))
            {
                ContainsLocalOrParameterReferenceWalker walker = ContainsLocalOrParameterReferenceWalker.GetInstance(localSymbol, context.SemanticModel, context.CancellationToken);

                walker.VisitList(switchStatement.Sections);

                if (!walker.Result)
                {
                    StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(switchStatement);

                    if (statementsInfo.Success)
                    {
                        int index = statementsInfo.IndexOf(switchStatement);

                        if (index < statementsInfo.Count - 1)
                            walker.VisitList(statementsInfo.Statements, index + 1);
                    }
                }

                bool isReferenced = walker.Result;

                ContainsLocalOrParameterReferenceWalker.Free(walker);

                return isReferenced;
            }

            return false;
        }
    }
}
