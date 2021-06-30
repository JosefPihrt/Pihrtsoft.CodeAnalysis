// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Roslynator.WellKnownDiagnosticIdPrefixes;

namespace Roslynator
{
    internal static class DiagnosticIdPrefix
    {
        public static string GetPrefix(string id)
        {
            int length = id.Length;

            if (length == 0)
                return "";

            switch (id[0])
            {
                case 'A':
                    {
                        if (HasPrefix(AD))
                        {
                            return AD;
                        }
                        else if (HasPrefix(Async))
                        {
                            return Async;
                        }

                        break;
                    }
                case 'B':
                    {
                        if (HasPrefix(BC))
                        {
                            return BC;
                        }
                        else if (HasPrefix(BL))
                        {
                            return BL;
                        }

                        break;
                    }
                case 'C':
                    {
                        if (HasPrefix(CA))
                        {
                            return CA;
                        }
                        else if (HasPrefix(CC))
                        {
                            return CC;
                        }
                        else if (HasPrefix(CS))
                        {
                            return CS;
                        }

                        break;
                    }
                case 'E':
                    {
                        if (HasPrefix(ENC))
                        {
                            return ENC;
                        }

                        break;
                    }
                case 'I':
                    {
                        if (HasPrefix(IDE))
                        {
                            return IDE;
                        }
                        else if (HasPrefix(IL))
                        {
                            return IL;
                        }

                        break;
                    }
                case 'R':
                    {
                        if (HasPrefix(RCS))
                        {
                            return RCS;
                        }
                        else if (HasPrefix(ROS))
                        {
                            return ROS;
                        }
                        else if (HasPrefix(RECS))
                        {
                            return RECS;
                        }
                        else if (HasPrefix(REVB))
                        {
                            return REVB;
                        }
                        else if (HasPrefix(RS))
                        {
                            return RS;
                        }

                        break;
                    }
                case 'S':
                    {
                        if (HasPrefix(SA))
                        {
                            return SA;
                        }
                        else if (HasPrefix(SX))
                        {
                            return SX;
                        }

                        break;
                    }
                case 'U':
                    {
                        if (HasPrefix(U2U))
                        {
                            return U2U;
                        }

                        break;
                    }
                case 'V':
                    {
                        if (HasPrefix(VB))
                        {
                            return VB;
                        }
                        else if (HasPrefix(VSSDK))
                        {
                            return VSSDK;
                        }
                        else if (HasPrefix(VSTHRD))
                        {
                            return VSTHRD;
                        }

                        break;
                    }
                case 'x':
                    {
                        if (HasPrefix(xUnit))
                        {
                            return xUnit;
                        }

                        break;
                    }
            }

            int prefixLength = GetPrefixLength(id);

            string prefix = id.Substring(0, prefixLength);

            if (prefix.Length > 0)
            {
                Debug.Fail($"Unknown diagnostic id prefix: {prefix}");
            }
            else
            {
                Debug.Assert(prefix != "RemoveUnnecessaryImportsFixable", $"Unknown diagnostic id: {id}");
            }

            return prefix;

            bool HasPrefix(string value)
            {
                return length > value.Length
                    && char.IsDigit(id, value.Length)
                    && string.Compare(id, 1, value, 1, value.Length - 1, StringComparison.Ordinal) == 0;
            }
        }

        public static int GetPrefixLength(string id)
        {
            int length = id.Length;

            int i = length - 1;

            while (i >= 0
                && char.IsLetter(id[i]))
            {
                i--;
            }

            while (i >= 0
                && char.IsDigit(id[i]))
            {
                i--;
            }

            return i + 1;
        }

        public static IEnumerable<(string prefix, int count)> CountPrefixes(IEnumerable<string> values)
        {
            foreach (IGrouping<string, string> grouping in values
                .GroupBy(f => GetPrefix(f))
                .OrderBy(f => f.Key))
            {
                yield return (grouping.Key, grouping.Count());
            }
        }
    }
}
