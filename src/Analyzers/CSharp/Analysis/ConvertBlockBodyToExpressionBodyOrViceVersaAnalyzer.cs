// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConvertBlockBodyToExpressionBodyOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa,
                        DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersaFadeOut,
                        CommonDiagnosticRules.AnalyzerOptionIsObsolete);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion < LanguageVersion.CSharp6)
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeIndexerDeclaration(f), SyntaxKind.IndexerDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeOperatorDeclaration(f), SyntaxKind.OperatorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeConversionOperatorDeclaration(f), SyntaxKind.ConversionOperatorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeConstructorDeclaration(f), SyntaxKind.ConstructorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeDestructorDeclaration(f), SyntaxKind.DestructorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeLocalFunctionStatement(f), SyntaxKind.LocalFunctionStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.GetAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.SetAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.AddAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.RemoveAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.InitAccessorDeclaration);
            });
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            BlockSyntax body = methodDeclaration.Body;
            if (body != null)
            {
                if (body.ContainsDirectives)
                    return;

                BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body);

                if (!analysis.Success)
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && methodDeclaration.SyntaxTree.IsMultiLineSpan(methodDeclaration.HeaderSpan()))
                {
                    return;
                }

                AnalyzeBlock(context, body, analysis);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = methodDeclaration.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                        && methodDeclaration.SyntaxTree.IsMultiLineSpan(methodDeclaration.HeaderSpan()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            ArrowExpressionClauseSyntax expressionBody = propertyDeclaration.ExpressionBody;

            if (expressionBody?.ContainsDirectives == false)
            {
                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                    return;
                }

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && propertyDeclaration.SyntaxTree.IsMultiLineSpan(propertyDeclaration.HeaderSpan()))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                    return;
                }

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                    && expressionBody.Expression?.IsMultiLine() == true)
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                }
            }
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            ArrowExpressionClauseSyntax expressionBody = indexerDeclaration.ExpressionBody;

            if (expressionBody?.ContainsDirectives == false)
            {
                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                    return;
                }

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && indexerDeclaration.SyntaxTree.IsMultiLineSpan(indexerDeclaration.HeaderSpan()))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                    return;
                }

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                    && expressionBody.Expression?.IsMultiLine() == true)
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                }
            }
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            BlockSyntax body = operatorDeclaration.Body;
            if (body != null)
            {
                if (body.ContainsDirectives)
                    return;

                BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body, allowExpressionStatement: false);

                if (!analysis.Success)
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && operatorDeclaration.SyntaxTree.IsMultiLineSpan(operatorDeclaration.HeaderSpan()))
                {
                    return;
                }

                AnalyzeBlock(context, body, analysis);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = operatorDeclaration.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                        && operatorDeclaration.SyntaxTree.IsMultiLineSpan(operatorDeclaration.HeaderSpan()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var operatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            BlockSyntax body = operatorDeclaration.Body;
            if (body != null)
            {
                if (body.ContainsDirectives)
                    return;

                BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body, allowExpressionStatement: false);

                if (!analysis.Success)
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && operatorDeclaration.SyntaxTree.IsMultiLineSpan(operatorDeclaration.HeaderSpan()))
                {
                    return;
                }

                AnalyzeBlock(context, body, analysis);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = operatorDeclaration.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                        && operatorDeclaration.SyntaxTree.IsMultiLineSpan(operatorDeclaration.HeaderSpan()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            BlockSyntax body = constructorDeclaration.Body;
            if (body != null)
            {
                if (body.ContainsDirectives)
                    return;

                BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body);

                if (!analysis.Success)
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && constructorDeclaration.SyntaxTree.IsMultiLineSpan(constructorDeclaration.HeaderSpan()))
                {
                    return;
                }

                AnalyzeBlock(context, body, analysis);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = constructorDeclaration.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                        && constructorDeclaration.SyntaxTree.IsMultiLineSpan(constructorDeclaration.HeaderSpan()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzeDestructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var destructorDeclaration = (DestructorDeclarationSyntax)context.Node;

            BlockSyntax body = destructorDeclaration.Body;
            if (body != null)
            {
                if (body.ContainsDirectives)
                    return;

                BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body);

                if (!analysis.Success)
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && destructorDeclaration.SyntaxTree.IsMultiLineSpan(destructorDeclaration.HeaderSpan()))
                {
                    return;
                }

                AnalyzeBlock(context, body, analysis);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = destructorDeclaration.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                        && destructorDeclaration.SyntaxTree.IsMultiLineSpan(destructorDeclaration.HeaderSpan()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzeLocalFunctionStatement(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var localFunction = (LocalFunctionStatementSyntax)context.Node;

            BlockSyntax body = localFunction.Body;
            if (body != null)
            {
                if (body.ContainsDirectives)
                    return;

                BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body);

                if (!analysis.Success)
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                    && localFunction.SyntaxTree.IsMultiLineSpan(localFunction.HeaderSpan()))
                {
                    return;
                }

                AnalyzeBlock(context, body, analysis);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = localFunction.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context)
                        && localFunction.SyntaxTree.IsMultiLineSpan(localFunction.HeaderSpan()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzeAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa.IsEffective(context))
                return;

            var accessor = (AccessorDeclarationSyntax)context.Node;

            BlockSyntax body = accessor.Body;

            if (body != null)
            {
                AnalyzeAccessorDeclarationBlock(context, accessor, body);
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = accessor.ExpressionBody;

                if (expressionBody?.ContainsDirectives == false)
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBody, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
                        return;
                    }

                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                        && expressionBody.Expression?.IsMultiLine() == true)
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine, expressionBody, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
                    }
                }
            }
        }

        private static void AnalyzeAccessorDeclarationBlock(
            SyntaxNodeAnalysisContext context,
            AccessorDeclarationSyntax accessor,
            BlockSyntax body)
        {
            if (body.ContainsDirectives)
                return;

            if (accessor.AttributeLists.Any())
                return;

            bool isGetter = accessor.IsKind(SyntaxKind.GetAccessorDeclaration);

            BlockExpressionAnalysis analysis = BlockExpressionAnalysis.Create(body, allowExpressionStatement: !isGetter);

            ExpressionSyntax expression = analysis.Expression;

            if (expression == null)
                return;

            if (AnalyzerOptions.ConvertExpressionBodyToBlockBody.IsEnabled(context))
                return;

            if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                && expression.IsMultiLine())
            {
                return;
            }

            if (isGetter
                && accessor.Parent is AccessorListSyntax accessorList
                && accessorList.Accessors.Count == 1)
            {
                if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(accessorList.OpenBraceToken))
                    return;

                if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(accessor.Keyword))
                    return;

                if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(body.OpenBraceToken))
                    return;

                if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine.IsEnabled(context))
                {
                    switch (accessorList.Parent.Kind())
                    {
                        case SyntaxKind.PropertyDeclaration:
                            {
                                if (accessor.SyntaxTree.IsMultiLineSpan(((PropertyDeclarationSyntax)accessorList.Parent).HeaderSpan()))
                                    return;

                                break;
                            }
                        case SyntaxKind.IndexerDeclaration:
                            {
                                if (accessor.SyntaxTree.IsMultiLineSpan(((IndexerDeclarationSyntax)accessorList.Parent).HeaderSpan()))
                                    return;

                                break;
                            }
                        default:
                            {
                                Debug.Fail(accessorList.Parent.Kind().ToString());
                                break;
                            }
                    }

                    return;
                }

                ReportDiagnostic(context, accessorList, expression);
                ReportFadeOut(context, accessor.Keyword, body);
                return;
            }

            if (!accessor.Keyword.TrailingTrivia.IsEmptyOrWhitespace())
                return;

            if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(body.OpenBraceToken))
                return;

            if (!accessor.Keyword.LeadingTrivia.IsEmptyOrWhitespace())
                return;

            ReportDiagnostic(context, body, expression);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context, BlockSyntax block, BlockExpressionAnalysis analysis)
        {
            if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(context)
                && analysis.Expression.IsMultiLine())
            {
                return;
            }

            if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(block.OpenBraceToken))
                return;

            if (!analysis.ReturnOrThrowKeyword.LeadingTrivia.IsEmptyOrWhitespace())
                return;

            ReportDiagnostic(context, analysis.Block, analysis.Expression);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode blockOrAccessorList, ExpressionSyntax expression)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa, blockOrAccessorList);

            DiagnosticHelpers.ReportObsolete(context, blockOrAccessorList, AnalyzerOptions.ConvertExpressionBodyToBlockBody);
            DiagnosticHelpers.ReportObsolete(context, blockOrAccessorList, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenDeclarationIsMultiLine);
            DiagnosticHelpers.ReportObsolete(context, blockOrAccessorList, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);

            ReportFadeOut(context, (expression.Parent as ReturnStatementSyntax)?.ReturnKeyword ?? default, blockOrAccessorList);
        }

        private static void ReportFadeOut(SyntaxNodeAnalysisContext context, SyntaxToken token, SyntaxNode blockOrAccessorList)
        {
            DiagnosticDescriptor fadeOutAnalyzer = DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersaFadeOut;

            if (token != default)
                DiagnosticHelpers.ReportToken(context, fadeOutAnalyzer, token);

            switch (blockOrAccessorList)
            {
                case BlockSyntax block:
                    {
                        CSharpDiagnosticHelpers.ReportBraces(context, fadeOutAnalyzer, block);
                        break;
                    }
                case AccessorListSyntax accessorList:
                    {
                        CSharpDiagnosticHelpers.ReportBraces(context, fadeOutAnalyzer, accessorList);
                        break;
                    }
                default:
                    {
                        Debug.Fail(blockOrAccessorList.Kind().ToString());
                        break;
                    }
            }
        }
    }
}
