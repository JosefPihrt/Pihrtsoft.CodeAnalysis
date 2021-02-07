﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public abstract class TestState
    {
        protected TestState(string source, string expectedSource)
        {
            Source = source;
            ExpectedSource = expectedSource;
        }

        protected TestState(
            string source,
            string expectedSource,
            IEnumerable<AdditionalFile> additionalFiles,
            string codeActionTitle,
            string equivalenceKey)
        {
            Source = source;
            ExpectedSource = expectedSource;
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty;
            CodeActionTitle = codeActionTitle;
            EquivalenceKey = equivalenceKey;
        }

        protected abstract TestState CommonWithSource(string source);

        protected abstract TestState CommonWithExpectedSource(string expectedSource);

        protected abstract TestState CommonWithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles);

        protected abstract TestState CommonWithCodeActionTitle(string codeActionTitle);

        protected abstract TestState CommonWithEquivalenceKey(string equivalenceKey);

        public TestState WithSource(string source) => CommonWithSource(source);

        public TestState WithExpectedSource(string expectedSource) => CommonWithExpectedSource(expectedSource);

        public TestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles) => CommonWithAdditionalFiles(additionalFiles);

        public TestState WithCodeActionTitle(string codeActionTitle) => CommonWithCodeActionTitle(codeActionTitle);

        public TestState WithEquivalenceKey(string equivalenceKey) => CommonWithEquivalenceKey(equivalenceKey);

        public string Source { get; protected set; }

        public string ExpectedSource { get; protected set; }

        public ImmutableArray<AdditionalFile> AdditionalFiles { get; protected set; }

        public string CodeActionTitle { get; protected set; }

        public string EquivalenceKey { get; protected set; }
    }
}
