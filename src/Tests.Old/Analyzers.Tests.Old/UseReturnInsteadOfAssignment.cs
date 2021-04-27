// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0219, RCS1004, RCS1016, RCS1048, RCS1081, RCS1111, RCS1118, RCS1124, RCS1126, RCS1163, RCS1169, RCS1176 

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UseReturnInsteadOfAssignment
    {
        private static bool _condition;

        private class UseReturnInsteadOfAssignment_IfStatement
        {
            public static int MethodName()
            {
                bool f = false;

                int x = 0;

                if (f)
                    x = 1;

                return x;

                int LocalFunction()
                {
                    LocalFunction();

                    int y = 0;

                    if (f)
                        y = 1;
                    else
                        y = 2;

                    return y;
                }
            }

            public static int MethodName2()
            {
                bool f = false;

                int x = 0, y = 0;

                void LocalFunction() => LocalFunction();

                if (f)
                {
                    x = 1;
                }
                else if (f)
                {
                    x = 2;
                }

                void LocalFunction2() => LocalFunction2();

                return x;
            }

            public static int MethodName3()
            {
                bool f = false;

                int x = 0;

                if (f)
                {
                    int y = x;
                    x = y;
                }
                else if (f)
                {
                    x = 2;
                }

                return x;
            }

            public static int MethodName4()
            {
                bool f = false;

                int x;

                if (f)
                {
                    x = 1;
                }
                else
                {
                    x = 2;
                }

                return x;
            }

            public int PropertyName
            {
                get
                {
                    bool f = false;
                    int x;

                    if (f)
                    {
                        x = 1;
                    }
                    else
                    {
                        x = 2;
                    }

                    return x;
                }
            }

            public int this[int x]
            {
                get
                {
                    bool f = false;

                    if (f)
                    {
                        x = 1;
                    }
                    else
                    {
                        x = 2;
                    }

                    return x;
                }
            }

            public static void SimpleLambda()
            {
                IEnumerable<int> q = Enumerable.Range(0, 1).Select(f =>
                {
                    bool condition = false;

                    if (condition)
                    {
                        f = 1;
                    }
                    else
                    {
                        f = 2;
                    }

                    return f;
                });
            }

            public static void ParenthesizedLambda()
            {
                IEnumerable<int> q = Enumerable.Range(0, 1).Select((f) =>
                {
                    bool condition = false;

                    if (condition)
                    {
                        f = 1;
                    }
                    else
                    {
                        f = 2;
                    }

                    return f;
                });
            }

            public static void AnonymousMethod()
            {
                IEnumerable<int> q = Enumerable.Range(0, 1).Select(delegate (int f)
                {
                    bool condition = false;

                    if (condition)
                    {
                        f = 1;
                    }
                    else
                    {
                        f = 2;
                    }

                    return f;
                });
            }
        }

        public class UseReturnInsteadOfAssignment_SwitchStatement
        {
            public static int MethodName()
            {
                bool f = false;

                int x = 0;

                switch (f)
                {
                    case true:
                        x = 1;
                        break;
                }

                return x;

                int LocalFunction()
                {
                    LocalFunction();

                    int y = 0;

                    switch (f)
                    {
                        case true:
                            {
                                y = 1;
                                break;
                            }
                        default:
                            {
                                y = 2;
                                break;
                            }
                    }

                    return y;
                }
            }

            public static int MethodName2()
            {
                bool f = false;

                int x = 0;

                switch (f)
                {
                    case true:
                        x = 1;
                        break;
                    case false:
                        x = 2;
                        break;
                }

                return x;
            }

            public static int MethodName3()
            {
                bool f = false;
                int x;

                switch (f)
                {
                    case true:
                        x = 1;
                        break;
                    default:
                        x = 2;
                        break;
                }

                return x;
            }

            public int PropertyName
            {
                get
                {
                    bool f = false;
                    int x;

                    switch (f)
                    {
                        case true:
                            x = 1;
                            break;
                        default:
                            x = 2;
                            break;
                    }

                    return x;
                }
            }

            public int this[int x]
            {
                get
                {
                    bool f = false;

                    switch (f)
                    {
                        case true:
                            x = 1;
                            break;
                        default:
                            x = 2;
                            break;
                    }

                    return x;
                }
            }

            public static void SimpleLambda()
            {
                IEnumerable<int> q = Enumerable.Range(0, 1).Select(f =>
                {
                    bool condition = false;

                    switch (condition)
                    {
                        case true:
                            f = 1;
                            break;
                        default:
                            f = 2;
                            break;
                    }

                    return f;
                });
            }

            public static void ParenthesizedLambda()
            {
                IEnumerable<int> q = Enumerable.Range(0, 1).Select((f) =>
                {
                    bool condition = false;

                    switch (condition)
                    {
                        case true:
                            f = 1;
                            break;
                        default:
                            f = 2;
                            break;
                    }

                    return f;
                });
            }

            public static void AnonymousMethod()
            {
                IEnumerable<int> q = Enumerable.Range(0, 1).Select(delegate (int f)
                {
                    bool condition = false;

                    switch (condition)
                    {
                        case true:
                            f = 1;
                            break;
                        default:
                            f = 2;
                            break;
                    }

                    return f;
                });
            }
        }
    }
}
