// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Roslynator.Testing
{
    public sealed class DummyCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => throw new NotSupportedException();

        public override Task RegisterCodeFixesAsync(CodeFixContext context) => throw new NotSupportedException();

        public override FixAllProvider GetFixAllProvider() => throw new NotSupportedException();
    }
}
