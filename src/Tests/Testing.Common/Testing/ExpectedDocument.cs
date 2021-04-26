// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Roslynator.Testing
{
    internal readonly struct ExpectedDocument
    {
        public ExpectedDocument(DocumentId id, string text)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public DocumentId Id { get; }

        public string Text { get; }
    }
}
