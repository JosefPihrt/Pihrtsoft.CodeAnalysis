// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.Spelling;

namespace Roslynator.CSharp.Spelling
{
    internal sealed class CSharpSpellingDiagnostic : SpellingDiagnostic
    {
        public CSharpSpellingDiagnostic(
            Diagnostic diagnostic,
            string value,
            int valueIndex,
            string containingValue,
            int containingValueIndex,
            SyntaxToken identifier = default) : base(diagnostic, value, valueIndex, containingValue, containingValueIndex, identifier)
        {
        }

        public override bool IsApplicableFix(string fix)
        {
            if (IsSymbol)
            {
                if (string.IsNullOrEmpty(fix))
                    return false;

                if (Offset == 0)
                {
                    if (!SyntaxFacts.IsIdentifierStartCharacter(fix[0]))
                        return false;
                }
                else if (!SyntaxFacts.IsIdentifierPartCharacter(fix[0]))
                {
                    return false;
                }

                int length = fix.Length;
                for (int i = 1; i < length; i++)
                {
                    if (!SyntaxFacts.IsIdentifierPartCharacter(fix[i]))
                        return false;
                }

                return true;
            }
            else
            {
                int length = fix.Length;
                for (int i = 0; i < length; i++)
                {
                    if (fix[i] == '\r'
                        || fix[i] == '\n')
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
