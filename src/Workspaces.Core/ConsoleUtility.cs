// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using static Roslynator.Logger;

namespace Roslynator
{
    internal static class ConsoleUtility
    {
        public static string? ReadUserInput(
            string prompt,
            string defaultValue,
            string? indent = null)
        {
            string? value = ReadUserInput(defaultValue, indent + prompt);

            if (value == null)
                ConsoleOut.WriteLine();

            return value;
        }

        private static string? ReadUserInput(string defaultValue, string? prompt = null)
        {
            bool treatControlCAsInput = Console.TreatControlCAsInput;

            try
            {
                Console.TreatControlCAsInput = true;

                return ReadUserInputImpl(defaultValue, prompt ?? "");
            }
            finally
            {
                Console.TreatControlCAsInput = treatControlCAsInput;
            }
        }

        private static string? ReadUserInputImpl(string defaultValue, string prompt)
        {
            Console.Write(prompt);

            int initTop = Console.CursorTop;
            int initLeft = Console.CursorLeft;

            List<char> buffer = defaultValue.ToList();

            Console.Write(defaultValue);

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            while (keyInfo.Key != ConsoleKey.Enter)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        {
                            int index = GetIndex();

                            if (index == 0)
                                break;

                            if (keyInfo.Modifiers == ConsoleModifiers.Control)
                            {
                                int i = index - 1;
                                while (i > 0)
                                {
                                    if (char.IsLetterOrDigit(buffer[i])
                                        && !char.IsLetterOrDigit(buffer[i - 1]))
                                    {
                                        break;
                                    }

                                    i--;
                                }

                                int offset = index - i;

                                int left = Console.CursorLeft;
                                int top = Console.CursorTop;

                                while (true)
                                {
                                    if (offset < left)
                                    {
                                        left -= offset;
                                        break;
                                    }
                                    else
                                    {
                                        offset -= left;
                                        left = Console.WindowWidth;
                                        top--;
                                    }
                                }

                                Console.SetCursorPosition(left, top);
                                break;
                            }

                            if (Console.CursorLeft == 0)
                            {
                                Console.SetCursorPosition(Console.WindowWidth - 1, Console.CursorTop - 1);
                            }
                            else
                            {
                                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                            }

                            break;
                        }
                    case ConsoleKey.RightArrow:
                        {
                            int index = GetIndex();

                            if (index == buffer.Count)
                                break;

                            if (keyInfo.Modifiers == ConsoleModifiers.Control)
                            {
                                int i = index + 1;
                                while (i < buffer.Count - 1)
                                {
                                    if (char.IsLetterOrDigit(buffer[i])
                                        && !char.IsLetterOrDigit(buffer[i + 1]))
                                    {
                                        break;
                                    }

                                    i++;
                                }

                                int offset = i + 1 - index;

                                int left = Console.CursorLeft;
                                int top = Console.CursorTop;

                                while (true)
                                {
                                    int right = Console.WindowWidth - left;

                                    if (offset < right)
                                    {
                                        left += offset;
                                        break;
                                    }
                                    else
                                    {
                                        offset -= right;
                                        left = 0;
                                        top++;
                                    }
                                }

                                Console.SetCursorPosition(left, top);
                                break;
                            }

                            if (Console.CursorLeft == Console.WindowWidth - 1)
                            {
                                Console.SetCursorPosition(0, Console.CursorTop + 1);
                            }
                            else
                            {
                                Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                            }

                            break;
                        }
                    case ConsoleKey.Home:
                        {
                            int left = (Console.CursorTop == initTop)
                                ? prompt.Length
                                : 0;

                            Console.SetCursorPosition(left, Console.CursorTop);
                            break;
                        }
                    case ConsoleKey.End:
                        {
                            int remainingLineCount = Console.WindowWidth - Console.CursorLeft;
                            int remainingCharCount = buffer.Count - GetIndex();

                            int left = (remainingCharCount > remainingLineCount)
                                ? Console.WindowWidth - 1
                                : Console.CursorLeft + remainingCharCount;

                            Console.SetCursorPosition(left, Console.CursorTop);
                            break;
                        }
                    case ConsoleKey.Backspace:
                        {
                            if (buffer.Count == 0)
                                break;

                            int index = GetIndex();

                            if (index == 0)
                                break;

                            buffer.RemoveAt(index - 1);

                            int left = Console.CursorLeft - 1;
                            int top = Console.CursorTop;

                            if (Console.CursorLeft == 0)
                            {
                                left = Console.WindowWidth - 1;
                                top--;
                            }

                            Console.SetCursorPosition(left, top);

                            char[] text = buffer.Skip(index - 1).Append(' ').ToArray();
                            Console.Write(text);

                            Console.SetCursorPosition(left, top);
                            break;
                        }
                    case ConsoleKey.Delete:
                        {
                            if (buffer.Count == 0)
                                break;

                            int index = GetIndex();

                            if (buffer.Count == index)
                                break;

                            buffer.RemoveAt(index);

                            int left = Console.CursorLeft;
                            int top = Console.CursorTop;

                            char[] text = buffer.Skip(index).Append(' ').ToArray();
                            Console.Write(text);

                            Console.SetCursorPosition(left, top);

                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            Console.SetCursorPosition(initLeft, initTop);
                            Console.Write(new string(' ', buffer.Count));
                            Console.SetCursorPosition(initLeft, initTop);

                            if (buffer.Count > 0)
                            {
                                buffer.Clear();
                            }
                            else
                            {
                                Console.Write(defaultValue);
                                buffer = defaultValue.ToList();
                            }

                            break;
                        }
                    default:
                        {
                            char ch = keyInfo.KeyChar;

                            // ctrl+c
                            if (keyInfo.Modifiers == ConsoleModifiers.Control
                                && ch == 3)
                            {
                                Console.WriteLine();
                                throw new OperationCanceledException();
                            }

                            if (ch < 32)
                                break;

                            int index = GetIndex();

                            buffer.Insert(index, ch);

                            int left = Console.CursorLeft;
                            int top = Console.CursorTop;

                            char[] text = buffer.Skip(index).ToArray();
                            Console.Write(text);

                            if (left == Console.WindowWidth - 1)
                            {
                                Console.SetCursorPosition(0, top + 1);
                            }
                            else
                            {
                                Console.SetCursorPosition(left + 1, top);
                            }

                            break;
                        }
                }

                keyInfo = Console.ReadKey(true);
            }

            Console.WriteLine();

            return new string(buffer.ToArray());

            int GetIndex()
            {
                if (Console.CursorTop == initTop)
                    return Console.CursorLeft - initLeft;

                int index = Console.WindowWidth - initLeft;
                index += (Console.CursorTop - initTop - 1) * Console.WindowWidth;
                index += Console.CursorLeft;

                return index;
            }
        }
    }
}
