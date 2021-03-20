// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.Spelling
{
    public static class TextUtility
    {
        public static string ReplaceRange(string s, string value, int index, int length)
        {
            int endIndex = index + length;

            return s.Remove(index)
                + value
                + s.Substring(endIndex, s.Length - endIndex);
        }

        public static string SetTextCasing(string s, TextCasing textCasing)
        {
            TextCasing textCasing2 = GetTextCasing(s);

            if (textCasing == textCasing2)
                return s;

            switch (textCasing)
            {
                case TextCasing.Lower:
                    return s.ToLowerInvariant();
                case TextCasing.Upper:
                    return s.ToUpperInvariant();
                case TextCasing.FirstUpper:
                    return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1).ToLowerInvariant();
                default:
                    throw new InvalidOperationException($"Invalid enum value '{textCasing}'");
            }
        }

        public static TextCasing GetTextCasing(string s)
        {
            int length = s.Length;

            if (length == 0)
                return TextCasing.Undefined;

            char ch = s[0];

            if (char.IsLower(ch))
            {
                for (int i = 1; i < length; i++)
                {
                    if (char.IsLetter(s[i])
                        && !char.IsLower(s[i]))
                    {
                        return TextCasing.Undefined;
                    }
                }

                return TextCasing.Lower;
            }
            else if (char.IsUpper(ch))
            {
                if (length == 1)
                    return TextCasing.Upper;

                ch = s[1];

                if (char.IsLower(ch))
                {
                    for (int i = 2; i < length; i++)
                    {
                        if (char.IsLetter(s[i])
                            && !char.IsLower(s[i]))
                        {
                            return TextCasing.Undefined;
                        }
                    }

                    return TextCasing.FirstUpper;
                }
                else if (char.IsUpper(ch))
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (char.IsLetter(s[i])
                            && !char.IsUpper(s[i]))
                        {
                            return TextCasing.Undefined;
                        }
                    }

                    return TextCasing.Upper;
                }
            }

            return TextCasing.Undefined;
        }

        public static bool TextCasingEquals(string value1, string value2)
        {
            return GetTextCasing(value1) == GetTextCasing(value2);
        }
    }
}
