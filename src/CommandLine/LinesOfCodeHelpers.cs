// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeMetrics;
using Roslynator.Host.Mef;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal static class LinesOfCodeHelpers
    {
        public static ImmutableDictionary<ProjectId, CodeMetricsInfo> CountLinesInParallel(
            IEnumerable<Project> projects,
            LinesOfCodeKind kind,
            CodeMetricsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var codeMetrics = new ConcurrentBag<(ProjectId projectId, CodeMetricsInfo codeMetrics)>();

            Parallel.ForEach(
                projects,
                project =>
                {
                    ICodeMetricsService service = MefWorkspaceServices.Default.GetService<ICodeMetricsService>(project.Language);

                    CodeMetricsInfo projectMetrics = (service != null)
                        ? service.CountLinesAsync(project, kind, options, cancellationToken).Result
                        : CodeMetricsInfo.NotAvailable;

                    codeMetrics.Add((project.Id, codeMetrics: projectMetrics));
                });

            return codeMetrics.ToImmutableDictionary(f => f.projectId, f => f.codeMetrics);
        }

        public static void WriteLinesOfCode(Solution solution, ImmutableDictionary<ProjectId, CodeMetricsInfo> projectsMetrics)
        {
            int maxDigits = projectsMetrics.Max(f => f.Value.CodeLineCount).ToString("n0").Length;
            int maxNameLength = projectsMetrics.Max(f => solution.GetProject(f.Key).Name.Length);

            foreach (KeyValuePair<ProjectId, CodeMetricsInfo> kvp in projectsMetrics
                .OrderByDescending(f => f.Value.CodeLineCount)
                .ThenBy(f => solution.GetProject(f.Key).Name))
            {
                Project project = solution.GetProject(kvp.Key);
                CodeMetricsInfo codeMetrics = kvp.Value;

                string count = (codeMetrics.CodeLineCount >= 0)
                    ? codeMetrics.CodeLineCount.ToString("n0").PadLeft(maxDigits)
                    : "-";

                WriteLine($"{count} {project.Name.PadRight(maxNameLength)} {project.Language}", Verbosity.Normal);
            }
        }
    }
}
