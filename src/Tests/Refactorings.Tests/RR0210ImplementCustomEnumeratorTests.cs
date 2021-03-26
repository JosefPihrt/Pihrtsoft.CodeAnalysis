// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0210ImplementCustomEnumeratorTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ImplementCustomEnumerator;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ImplementCustomEnumerator)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Collections;
using System.Collections.Generic;

class [||]C<T> : IEnumerable<T>
{
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
", @"
using System;
using System.Collections;
using System.Collections.Generic;

class C<T> : IEnumerable<T>
{
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public struct Enumerator
    {
        private readonly C<T> _c;
        private int _index;

        internal Enumerator(C<T> c)
        {
            _c = c;
            _index = -1;
        }

        public T Current
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            _index = -1;
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotSupportedException();
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException();
        }
    }

    //TODO: IEnumerable.GetEnumerator() and IEnumerable<T>.GetEnumerator() should return instance of EnumeratorImpl.
    private class EnumeratorImpl : IEnumerator<T>
    {
        private Enumerator _e;

        internal EnumeratorImpl(C<T> c)
        {
            _e = new Enumerator(c);
        }

        public T Current
        {
            get
            {
                return _e.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return _e.Current;
            }
        }

        public bool MoveNext()
        {
            return _e.MoveNext();
        }

        void IEnumerator.Reset()
        {
            _e.Reset();
        }

        void IDisposable.Dispose()
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ImplementCustomEnumerator)]
        public async Task TestNoRefactoring_BaseTypeContainsEnumerator()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;

class [||]C : List<object>
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
