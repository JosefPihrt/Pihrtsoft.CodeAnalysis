// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal static class DiagnosticDescriptorUtility
    {
        private const string HelpLinkUriRoot = "http://pihrt.net/roslynator/analyzer?id=";

        public static string GetHelpLinkUri(string analyzerId)
        {
            return HelpLinkUriRoot + analyzerId;
        }
    }
}
