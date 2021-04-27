// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RemoveStatementsFromSwitchSectionsRefactoring
    {
        public string GetValue()
        {
            DayOfWeek dayOfWeek = DayOfWeek.Sunday;

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    break;
                case DayOfWeek.Tuesday:
                    break;
                case DayOfWeek.Wednesday:
                    break;
                case DayOfWeek.Thursday:
                    break;
                case DayOfWeek.Friday:
                    break;
            }





            return null;
        }
    }
}
