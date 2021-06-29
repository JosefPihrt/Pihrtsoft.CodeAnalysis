// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CodeMetrics
{
    internal readonly struct CodeMetricsInfo : IEquatable<CodeMetricsInfo>
    {
        internal static CodeMetricsInfo NotAvailable { get; } = new CodeMetricsInfo(-1, 0, 0, 0, 0, 0);

        internal CodeMetricsInfo(
            int totalLineCount,
            int codeLineCount,
            int whitespaceLineCount,
            int commentLineCount,
            int preprocessorDirectiveLineCount,
            int blockBoundaryLineCount)
        {
            TotalLineCount = totalLineCount;
            CodeLineCount = codeLineCount;
            WhitespaceLineCount = whitespaceLineCount;
            CommentLineCount = commentLineCount;
            PreprocessorDirectiveLineCount = preprocessorDirectiveLineCount;
            BlockBoundaryLineCount = blockBoundaryLineCount;
        }

        public int TotalLineCount { get; }

        public int CodeLineCount { get; }

        public int WhitespaceLineCount { get; }

        public int CommentLineCount { get; }

        public int PreprocessorDirectiveLineCount { get; }

        public int BlockBoundaryLineCount { get; }

        internal CodeMetricsInfo Add(in CodeMetricsInfo codeMetrics)
        {
            return new CodeMetricsInfo(
                totalLineCount: TotalLineCount + codeMetrics.TotalLineCount,
                codeLineCount: CodeLineCount + codeMetrics.CodeLineCount,
                whitespaceLineCount: WhitespaceLineCount + codeMetrics.WhitespaceLineCount,
                commentLineCount: CommentLineCount + codeMetrics.CommentLineCount,
                preprocessorDirectiveLineCount: PreprocessorDirectiveLineCount + codeMetrics.PreprocessorDirectiveLineCount,
                blockBoundaryLineCount: BlockBoundaryLineCount + codeMetrics.BlockBoundaryLineCount);
        }

        public static CodeMetricsInfo Create(IEnumerable<CodeMetricsInfo> metrics)
        {
            return new CodeMetricsInfo(
                totalLineCount: metrics.Sum(f => f.TotalLineCount),
                codeLineCount: metrics.Sum(f => f.CodeLineCount),
                whitespaceLineCount: metrics.Sum(f => f.WhitespaceLineCount),
                commentLineCount: metrics.Sum(f => f.CommentLineCount),
                preprocessorDirectiveLineCount: metrics.Sum(f => f.PreprocessorDirectiveLineCount),
                blockBoundaryLineCount: metrics.Sum(f => f.BlockBoundaryLineCount));
        }

        public override bool Equals(object obj)
        {
            return obj is CodeMetricsInfo other && Equals(other);
        }

        public bool Equals(CodeMetricsInfo other)
        {
            return TotalLineCount == other.TotalLineCount
                && CodeLineCount == other.CodeLineCount
                && WhitespaceLineCount == other.WhitespaceLineCount
                && CommentLineCount == other.CommentLineCount
                && PreprocessorDirectiveLineCount == other.PreprocessorDirectiveLineCount
                && BlockBoundaryLineCount == other.BlockBoundaryLineCount;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(
                TotalLineCount,
                Hash.Combine(
                    CodeLineCount,
                    Hash.Combine(
                        WhitespaceLineCount,
                        Hash.Combine(
                            CommentLineCount,
                            Hash.Combine(PreprocessorDirectiveLineCount, Hash.Create(BlockBoundaryLineCount))))));
        }

        public static bool operator ==(in CodeMetricsInfo metrics1, in CodeMetricsInfo metrics2)
        {
            return metrics1.Equals(metrics2);
        }

        public static bool operator !=(in CodeMetricsInfo metrics1, in CodeMetricsInfo metrics2)
        {
            return !(metrics1 == metrics2);
        }
    }
}
