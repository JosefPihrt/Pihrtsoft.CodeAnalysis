// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

extern alias x

using System
using System.Collections.Generic

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddSemicolon
    {
        private class Entity
        {
            public string FooMethod() => FooMethod()

            public Entity() => FooMethod()

            ~Entity() => FooMethod()

            public static Entity operator !(Entity entity) => new Entity()

            public static explicit operator Entity(string value) => new Entity()

            public static explicit operator string(Entity value) => string.Empty

            public string FooProperty => string.Empty

            public string this[int index] => FooMethod()

            private string _fooProperty2

            public string FooProperty2
            {
                get => _fooProperty2
                set => _fooProperty2 = value
            }

            public string this[string index]
            {
                get => _fooProperty2
                set => _fooProperty2 = value
            }

            private EventHandler _event;

            public event EventHandler Event
            {
                add => _event += value
                remove => _event -= value
            }

            private string _fooField = ""

            public event EventHandler FooChanged

            public object Bar()
            {
                var items = new List<string>()

                foreach (string item in items)
                {
                    if (true)
                        break

                    if (true)
                    {
                        continue
                    }

                    if (true)
                    {
                        return null
                    }

                    if (true)
                        throw new Exception()

                    goto End
                }

                do
                {

                } while (true)

                Bar()


                switch (0)
                {
                    case 0:
                        break
                    case 1:
                        goto case 0
                    case 2:
                        goto default
                    default:
                        break
                }

                try
                {
                }
                catch (Exception)
                {
                    throw
                }

                End:
                Bar()
            }

            private void VoidMethod()
            {
                return
            }

            private IEnumerable<string> Iterator()
            {
                yield return ""
                yield break
            }
        }
    }
}
