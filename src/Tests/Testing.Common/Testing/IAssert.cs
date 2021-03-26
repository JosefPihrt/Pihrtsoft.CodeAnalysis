// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Testing
{
    internal interface IAssert
    {
        /// <summary>
        /// Compares specified values and throws error if they are not equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        void Equal(string expected, string actual);

        /// <summary>
        /// Throws an error if a condition is not equal to <c>true</c>.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="userMessage"></param>
        void True(bool condition, string userMessage);
    }
}
