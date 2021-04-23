## Roslynator Refactorings

#### Add braces \(RR0002\)

* **Syntax**: do statement, else clause, fixed statement, for statement, foreach statement, if statement, lock statement, using statement, while statement
* **Span**: embedded statement
![Add braces](../../images/refactorings/AddBraces.png)

#### Add braces to if\-else \(RR0003\)

* **Syntax**: if\-else chain
* **Span**: embedded statement
![Add braces to if-else](../../images/refactorings/AddBracesToIfElse.png)

#### Add braces to switch section \(RR0004\)

* **Syntax**: switch section
* **Span**: case or default keyword
![Add braces to switch section](../../images/refactorings/AddBracesToSwitchSection.png)

#### Add braces to switch sections \(RR0005\)

* **Syntax**: switch statement
* **Span**: case or default keyword
![Add braces to switch sections](../../images/refactorings/AddBracesToSwitchSections.png)

#### Add cast expression \(RR0006\)

* **Syntax**: argument, assignment expression, return statement, variable declaration
![Add cast expression](../../images/refactorings/AddCastExpressionToArgument.png)

![Add cast expression](../../images/refactorings/AddCastExpressionToAssignmentExpression.png)

![Add cast expression](../../images/refactorings/AddCastExpressionToReturnStatement.png)

![Add cast expression](../../images/refactorings/AddCastExpressionToVariableDeclaration.png)

#### Add default value to parameter \(RR0007\)

* **Syntax**: parameter without default value
* **Span**: identifier
![Add default value to parameter](../../images/refactorings/AddDefaultValueToParameter.png)

#### Add default value to return statement \(RR0008\)

* **Syntax**: return statement without expression
![Add default value to return statement](../../images/refactorings/AddDefaultValueToReturnStatement.png)

#### Add empty line between declarations \(RR0205\)

* **Syntax**: selected declarations

#### Before

```csharp
private object x;
private object y;
private object z;
```

#### After

```csharp
private object x;

private object y;

private object z;
```

#### Add exception to documentation comment \(RR0009\)

* **Syntax**: throw statement
![Add exception to documentation comment](../../images/refactorings/AddExceptionToDocumentationComment.png)

#### Add identifier to parameter \(RR0012\)

* **Syntax**: parameter
* **Span**: missing identifier
![Add identifier to parameter](../../images/refactorings/AddIdentifierToParameter.png)

#### Add identifier to variable declaration \(RR0010\)

* **Syntax**: variable declaration
![Add identifier to variable declaration](../../images/refactorings/AddIdentifierToVariableDeclaration.png)

#### Add member to interface \(RR0195\)

* **Syntax**: method, property, indexer, event
* **Span**: identifier

#### Before

```csharp
public class Foo : IFoo
{
    public void Bar()
    {
    }
}

public interface IFoo
{
}
```

#### After

```csharp
public class Foo : IFoo
{
    public void Bar()
    {
    }
}

public interface IFoo
{
    void Bar();
}
```

#### Add missing cases to switch statement \(RR0059\)

* **Syntax**: switch statement

#### Before

```csharp
switch (dayOfWeek)
{
    case DayOfWeek.Sunday:
        break;
    case DayOfWeek.Monday:
        break;
    case DayOfWeek.Tuesday:
        break;
    case DayOfWeek.Wednesday:
        break;
    case DayOfWeek.Thursday:
        break;
}
```

#### After

```csharp
switch (dayOfWeek)
{
    case DayOfWeek.Sunday:
        break;
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
    case DayOfWeek.Saturday:
        break;
}
```

#### Add parameter name to argument \(RR0011\)

* **Syntax**: argument list
![Add parameter name to argument](../../images/refactorings/AddParameterNameToArgument.png)

#### Add parameter to interface member \(RR0213\)

* **Syntax**: method declaration, indexer declaration
* **Span**: method name or 'this' keyword

#### Before

```csharp
interface IFoo
{
    void Bar(object p);
}

class Foo : IFoo
{
    public void Bar(object p, object p2)
    {
    }
}
```

#### After

```csharp
interface IFoo
{
    void Bar(object p, object p2);
}

class Foo : IFoo
{
    public void Bar(object p, object p2)
    {
    }
}
```

#### Add tag to documentation comment \(RR0208\)

* **Syntax**: selected word\(s\) in documentation comment

#### Before

```csharp
/// <summary>
/// null
/// </summary>
public class Foo
{
}
```

#### After

```csharp
/// <summary>
/// <c>null</c>
/// </summary>
public class Foo
{
}
```

#### Add type parameter \(RR0178\)

* **Syntax**: class declaration, struct declaration, interface declaration, delegate declaration, method declaration, local function
![Add type parameter](../../images/refactorings/AddTypeParameter.png)

#### Add using directive \(RR0013\)

* **Syntax**: qualified name
* **Span**: selected namespace
![Add using directive](../../images/refactorings/AddUsingDirective.png)

#### Add using static directive \(RR0014\)

* **Syntax**: member access expression \(public or internal static class\)
* **Span**: selected class name
![Add using static directive](../../images/refactorings/AddUsingStaticDirective.png)

#### Call 'ConfigureAwait\(false\)' \(RR0015\)

* **Syntax**: awaitable method invocation
* **Span**: method name
![Call 'ConfigureAwait(false)'](../../images/refactorings/CallConfigureAwait.png)

#### Call extension method as instance method \(RR0016\)

* **Syntax**: method invocation
![Call extension method as instance method](../../images/refactorings/CallExtensionMethodAsInstanceMethod.png)

#### Call string\.IndexOf instead of string\.Contains \(RR0144\)

* **Syntax**: method invocation
* **Span**: method name

#### Before

```csharp
if (s.Contains("a"))
{
{
```

#### After

```csharp
if (s.IndexOf("a", StringComparison.OrdinalIgnoreCase) != -1)
{
{
```

#### Call 'To\.\.\.' method \(ToString, ToArray, ToList\) \(RR0017\)

* **Syntax**: argument, assignment expression, return statement, variable declaration
![Call 'To...' method (ToString, ToArray, ToList)](../../images/refactorings/CallToMethod.png)

#### Change accessibility \(RR0186\)

* **Syntax**: access modifier
![Change accessibility](../../images/refactorings/ChangeAccessibility.png)

#### Change explicit type to 'var' \(RR0018\)

* **Syntax**: variable declaration, foreach statement
* **Span**: type
![Change explicit type to 'var'](../../images/refactorings/ChangeExplicitTypeToVar.png)

#### Change method return type to 'void' \(RR0021\)

* **Syntax**: method, local function
![Change method return type to 'void'](../../images/refactorings/ChangeMethodReturnTypeToVoid.png)

#### Change method/property/indexer type according to return expression \(RR0019\)

* **Syntax**: return statement in method/property/indexer
![Change method/property/indexer type according to return expression](../../images/refactorings/ChangeMemberTypeAccordingToReturnExpression.png)

#### Change method/property/indexer type according to yield return expression \(RR0020\)

* **Syntax**: yield return statement in method/property/indexer
![Change method/property/indexer type according to yield return expression](../../images/refactorings/ChangeMemberTypeAccordingToYieldReturnExpression.png)

#### Change type according to expression \(RR0022\)

* **Syntax**: variable declaration, foreach statement
* **Span**: type

#### Before

```csharp
IEnumerable<object> items = new List<object>();
```

#### After

```csharp
List<object> items = new List<object>();
```

#### Change 'var' to explicit type \(RR0023\)

* **Syntax**: variable declaration, foreach statement
* **Span**: type
![Change 'var' to explicit type](../../images/refactorings/ChangeVarToExplicitType.png)

#### Check expression for null \(RR0024\)

* **Syntax**: local declaration \(identifier\), assignment expression \(left\)
![Check expression for null](../../images/refactorings/CheckExpressionForNull.png)

#### Check parameter for null \(RR0025\)

* **Syntax**: parameter
* **Span**: parameter identifier
![Check parameter for null](../../images/refactorings/CheckParameterForNull.png)

#### Collapse to initializer \(RR0026\)

* **Syntax**: object creation followed with assignment\(s\)
![Collapse to initializer](../../images/refactorings/CollapseToInitializer.png)

#### Comment out member \(RR0027\)

* **Syntax**: method, constructor, property, indexer, operator, event, namespace, class, struct, interface
* **Span**: opening or closing brace
![Comment out member](../../images/refactorings/CommentOutMember.png)

#### Comment out statement \(RR0028\)

* **Syntax**: do statement, fixed statement, for statement, foreach statement, checked statement, if statement, lock statement, switch statement, try statement, unchecked statement, unsafe statement, using statement, while statement
* **Span**: opening or closing brace
![Comment out statement](../../images/refactorings/CommentOutStatement.png)

#### Convert "" to string\.Empty \(RR0171\)

* **Syntax**: empty string literal
![Convert "" to string.Empty](../../images/refactorings/ConvertEmptyStringToStringEmpty.png)

#### Convert '\(yield\) return' to 'if' \(RR0143\)

* **Syntax**: return statement, yield return statement
* **Span**: selected statement, yield keyword or return keyword
![Convert '(yield) return' to 'if'](../../images/refactorings/ReplaceReturnStatementWithIfElse.png)

#### Convert ?: to if\-else \(RR0120\)

* **Syntax**: ?: operator that is part of local declaration, assignment or \(yield\) return statement

#### Before

```csharp
string s = (x) ? "a" : "b";
```

#### After

```csharp
string s;
if (x)
{
    s = "a";
}
else
{
    s = "b";
}
```

- - -

#### Before

```csharp
string s = (x) ? "a" : (y) ? "b" : "c";
```

#### After

```csharp
string s;
if (x)
{
    s = "a";
}
else if (y)
{
    s = "b";
}
else
{
    s = "c";
}
```

#### Convert block body to expression\-body \(RR0169\)

* **Syntax**: method, property, indexer, operator
* **Span**: body or accessor list
![Convert block body to expression-body](../../images/refactorings/ConvertBlockBodyToExpressionBody.png)

#### Convert comment to documentation comment \(RR0192\)

* **Syntax**: single\-line comment

#### Before

```csharp
// comment
public class Foo
{
}
```

#### After

```csharp
/// <summary>
/// comment
/// </summary>
public class Foo
{
}
```

#### Convert 'do' to 'while' \(RR0123\)

* **Syntax**: do statement
* **Span**: do keyword

#### Before

```csharp
do
{
} while (condition);
```

#### After

```csharp
while (condition)
{
}
```

#### Convert expression\-body to block body \(RR0037\)

* **Syntax**: expression body
![Convert expression-body to block body](../../images/refactorings/ConvertExpressionBodyToBlockBody.png)

#### Convert 'for' to 'foreach' \(RR0130\)

* **Syntax**: for statement
![Convert 'for' to 'foreach'](../../images/refactorings/ConvertForToForEach.png)

#### Convert 'for' to 'while' \(RR0131\)

* **Syntax**: for statement
* **Span**: for keyword or selected for statement
![Convert 'for' to 'while'](../../images/refactorings/ConvertForToWhile.png)

#### Convert 'foreach' to 'for' \(RR0129\)

* **Syntax**: foreach statement
![Convert 'foreach' to 'for'](../../images/refactorings/ConvertForEachToFor.png)

#### Convert 'foreach' to 'for' and reverse loop \(RR0188\)

* **Syntax**: foreach statement

#### Before

```csharp
foreach (object item in items)
{
    yield return item;
}
```

#### After

```csharp
for (int i = items.Count - 1; i >= 0; i--)
{
    yield return items[i];
}
```

#### Convert 'HasFlag' call to bitwise operation \(RR0164\)

* **Syntax**: Enum\.HasFlag method invocation
![Convert 'HasFlag' call to bitwise operation](../../images/refactorings/ConvertHasFlagCallToBitwiseOperation.png)

#### Convert hexadecimal literal to decimal literal \(RR0132\)

* **Syntax**: hexadecimal literal
![Convert hexadecimal literal to decimal literal](../../images/refactorings/ConvertHexadecimalLiteralToDecimalLiteral.png)

#### Convert 'if' to ?: \(RR0166\)

* **Syntax**: if statement
* **Span**: top if keyword or selected if statement
![Convert 'if' to ?:](../../images/refactorings/ConvertIfToConditionalOperator.png)

#### Convert 'if' to 'switch' \(RR0133\)

* **Syntax**: if statement
* **Span**: top if keyword or selected if statement

#### Before

```csharp
var ch = stringReader.Read();

if (ch == 10 || ch == 13)
{
    return;
}
else
{
    stringBuilder.Append(ch);
}
```

#### After

```csharp
var ch = stringReader.Read();

switch (ch)
{
    case 10:
    case 13:
        {
            return;
        }

    default:
        {
            stringBuilder.Append(ch);
            break;
        }
}
```

#### Convert interpolated string to concatenation \(RR0193\)

* **Syntax**: interpolated string

#### Before

```csharp
string s = $"a{b}c";
```

#### After

```csharp
string s = "a" + b + "c";
```

#### Convert interpolated string to string literal \(RR0136\)

* **Syntax**: Interpolated string without any interpolation
![Convert interpolated string to string literal](../../images/refactorings/ConvertInterpolatedStringToStringLiteral.png)

#### Convert interpolated string to 'string\.Format' \(RR0201\)

* **Syntax**: interpolated string

#### Before

```csharp
$"name: {name,0:f}, value: {value}"
```

#### After

```csharp
string.Format("name: {0,0:f} value: {1}", name, value)
```

#### Convert lambda expression body to block body \(RR0039\)

* **Syntax**: lambda expression's expression body
* **Span**: body
![Convert lambda expression body to block body](../../images/refactorings/ConvertLambdaExpressionBodyToBlockBody.png)

#### Convert lambda expression body to expression\-body \(RR0154\)

* **Syntax**: lambda expression with block body with single single\-line statement
* **Span**: body
![Convert lambda expression body to expression-body](../../images/refactorings/ConvertLambdaExpressionBodyToExpressionBody.png)

#### Convert method group to lambda \(RR0137\)

* **Syntax**: method group

#### Before

```csharp
Func<object, object, object> func = Foo;
```

#### After

```csharp
Func<object, object, object> func = (f, g) => Foo(f, g)
```

#### Convert null literal to default expression \(RR0139\)

* **Syntax**: argument
![Convert null literal to default expression](../../images/refactorings/ConvertNullLiteralToDefaultExpression.png)

#### Convert regular string literal to verbatim string literal \(RR0142\)

* **Syntax**: regular string literal
![Convert regular string literal to verbatim string literal](../../images/refactorings/ConvertRegularStringLiteralToVerbatimStringLiteral.png)

#### Convert statements to if\-else \(RR0211\)

* **Syntax**: selected statements \(first statement must be 'if' statement\)

#### Before

```csharp
if (x)
    return 1;

if (y)
{
    return 2;
}
else if (z)
{
    return 3;
}

return 0;
```

#### After

```csharp
if (x)
{
    return 1;
}
else if (y)
{
    return 2;
}
else if (z)
{
    return 3;
}
else
{
    return 0;
}
```

#### Convert 'string\.Format' to interpolated string \(RR0145\)

* **Syntax**: string\.Format method
![Convert 'string.Format' to interpolated string](../../images/refactorings/ConvertStringFormatToInterpolatedString.png)

#### Convert 'switch' expression to 'switch' statement \(RR0214\)

* **Syntax**: switch expression
* **Span**: switch keyword

#### Before

```csharp
return dayOfWeek switch
{
    DayOfWeek.Monday => 1,
    DayOfWeek.Tuesday => 2,
    DayOfWeek.Wednesday => 3,
    DayOfWeek.Thursday => 4,
    DayOfWeek.Friday => 5,
    _ => throw new Exception(),
};
```

#### After

```csharp
switch (dayOfWeek)
{
    case DayOfWeek.Monday:
        return 1;
    case DayOfWeek.Tuesday:
        return 2;
    case DayOfWeek.Wednesday:
        return 3;
    case DayOfWeek.Thursday:
        return 4;
    case DayOfWeek.Friday:
        return 5;
    default:
        throw new Exception();
}
```

#### Convert 'switch' to 'if' \(RR0147\)

* **Syntax**: switch statement
* **Span**: switch keyword
![Convert 'switch' to 'if'](../../images/refactorings/ConvertSwitchToIf.png)

#### Convert verbatim string literal to regular string literal \(RR0148\)

* **Syntax**: verbatim string literal
![Convert verbatim string literal to regular string literal](../../images/refactorings/ConvertVerbatimStringLiteralToRegularStringLiteral.png)

#### Convert verbatim string literal to regular string literals \(RR0149\)

* **Syntax**: multiline verbatim string literal
![Convert verbatim string literal to regular string literals](../../images/refactorings/ConvertVerbatimStringLiteralToRegularStringLiterals.png)

#### Convert 'while' statement to 'do' statement \(RR0150\)

* **Syntax**: while statement
* **Span**: while keyword

#### Before

```csharp
while (condition)
{
}
```

#### After

```csharp
if (condition)
{
  do
  {
  } while (condition);
}
```

#### Convert 'while' statement to 'for' statement \(RR0151\)

* **Syntax**: while statement
* **Span**: while keyword or selected statement\(s\)
![Convert 'while' statement to 'for' statement](../../images/refactorings/ConvertWhileToFor.png)

#### Copy documentation comment from base member \(RR0029\)

* **Syntax**: constructor, method, property, indexer, event
![Copy documentation comment from base member](../../images/refactorings/CopyDocumentationCommentFromBaseMember.png)

![Copy documentation comment from base member](../../images/refactorings/CopyDocumentationCommentFromImplementedMember.png)

#### Duplicate argument \(RR0030\)

* **Syntax**: missing argument
![Duplicate argument](../../images/refactorings/DuplicateArgument.png)

#### Duplicate member \(RR0031\)

* **Syntax**: method, constructor, property, indexer, operator, event, namespace, class, struct, interface
* **Span**: opening or closing brace
![Duplicate member](../../images/refactorings/DuplicateMember.png)

#### Duplicate parameter \(RR0032\)

* **Syntax**: missing parameter
![Duplicate parameter](../../images/refactorings/DuplicateParameter.png)

#### Duplicate statement \(RR0033\)

* **Syntax**: do statement, fixed statement, for statement, foreach statement, checked statement, if statement, lock statement, switch statement, try statement, unchecked statement, unsafe statement, using statement, while statement
* **Span**: opening or closing brace
![Duplicate statement](../../images/refactorings/DuplicateStatement.png)

#### Duplicate switch section \(RR0212\)

* **Syntax**: switch section
* **Span**: close brace or empty line after switch section

#### Before

```csharp
switch (s)
{
    case "a":
        {
            // ...

            break;
        }
    default:
        {
            break;
        }
}
```

#### After

```csharp
switch (s)
{
    case "a":
        {
            // ...

            break;
        }
    case "a":
        {
            // ...

            break;
        }
    default:
        {
            break;
        }
}
```

#### Expand coalesce expression \(RR0035\)

* **Syntax**: coalesce expression
* **Span**: ?? operator
![Expand coalesce expression](../../images/refactorings/ExpandCoalesceExpression.png)

#### Expand compound assignment operator \(RR0034\)

* **Syntax**: compound assignment expression
* **Span**: operator
![Expand compound assignment operator](../../images/refactorings/ExpandCompoundAssignmentOperator.png)

#### Expand event \(RR0036\)

* **Syntax**: event field declaration
![Expand event](../../images/refactorings/ExpandEvent.png)

#### Expand initializer \(RR0038\)

* **Syntax**: initializer
![Expand initializer](../../images/refactorings/ExpandInitializer.png)

#### Expand property \(RR0040\)

* **Syntax**: auto\-property
![Expand property](../../images/refactorings/ExpandProperty.png)

#### Expand property and add backing field \(RR0041\)

* **Syntax**: auto\-property
![Expand property and add backing field](../../images/refactorings/ExpandPropertyAndAddBackingField.png)

#### Extract event handler method \(RR0203\)

* **Syntax**: lambda expression

#### Before

```csharp
void Foo()
{
  x.Changed += (s, e) => Bar();
}
```

#### After

```csharp
void Foo()
{
  x.Changed += Changed;
}

void OnChanged(object sender, EventArgs e)
{
  Bar();
}
```

#### Extract expression from condition \(RR0043\)

* **Syntax**: if statement, while statement
* **Span**: condition

#### Before

```csharp
if (x && y) // Select 'y'
{
}
```

#### After

```csharp
if(x)
{
    if (y)
    {
    }
}
```

- - -

#### Before

```csharp
if (x || y) // Select 'y'
{
}
```

#### After

```csharp
if(x)
{
}

if (y)
{
}
```

#### Extract generic type \(RR0044\)

* **Syntax**: generic name with single type argument
* **Span**: type argument
![Extract generic type](../../images/refactorings/ExtractGenericType.png)

#### Extract statement\(s\) \(RR0045\)

* **Syntax**: else clause, fixed statement, for statement, foreach statement, checked statement, if statement, lock statement, try statement, unsafe statement, using statement, while statement
![Extract statement(s)](../../images/refactorings/ExtractStatement.png)

#### Extract type declaration to a new file \(RR0046\)

* **Syntax**: class declaration, struct declaration, interface declaration, enum declaration, delegate declaration
* **Span**: identifier
![Extract type declaration to a new file](../../images/refactorings/ExtractTypeDeclarationToNewFile.png)

#### Format accessor braces \(RR0047\)

* **Syntax**: get accessor, set accessor, add accessor, remove accessor
* **Span**: block
![Format accessor braces](../../images/refactorings/FormatAccessorBracesOnMultipleLines.png)

![Format accessor braces](../../images/refactorings/FormatAccessorBracesOnSingleLine.png)

#### Generate base constructors \(RR0054\)

* **Syntax**: class declaration
* **Span**: identifier
![Generate base constructors](../../images/refactorings/GenerateBaseConstructors.png)

#### Generate combined enum member \(RR0055\)

* **Syntax**: enum declaration \(with FlagsAttribute\)
![Generate combined enum member](../../images/refactorings/GenerateCombinedEnumMember.png)

#### Generate enum member \(RR0056\)

* **Syntax**: enum declaration \(with FlagsAttribute\)
![Generate enum member](../../images/refactorings/GenerateEnumMember.png)

#### Generate enum values \(RR0057\)

* **Syntax**: enum declaration \(with FlagsAttribute\)
![Generate enum values](../../images/refactorings/GenerateEnumValues.png)

#### Generate event invoking method \(RR0058\)

* **Syntax**: event
* **Span**: identifier
![Generate event invoking method](../../images/refactorings/GenerateEventInvokingMethod.png)

#### Generate property for DebuggerDisplay attribute \(RR0204\)

* **Syntax**: DebuggerDisplay attribute

#### Before

```csharp
[DebuggerDisplay("A: {A} B: {B}")]
public class Foo
{
    public string A { get; }
    public string B { get; }
}
```

#### After

```csharp
DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Foo
{
    public string A { get; }
    public string B { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get { return $"A: {A} B: {B}"; }
    }
}
```

#### Implement custom enumerator \(RR0210\)

* **Syntax**: class that implements IEnumerable\<T>
* **Span**: identifier

#### Before

```csharp
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
}
```

#### After

```csharp
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
```

#### Implement IEquatable\<T> \(RR0179\)

* **Syntax**: class declaration, struct declaration, interface declaration
* **Span**: base list
![Implement IEquatable\<T>](../../images/refactorings/ImplementIEquatableOfT.png)

#### Initialize field from constructor \(RR0197\)

* **Syntax**: field declaration
* **Span**: identifier

#### Before

```csharp
public class Foo
{
    private string _bar;

    public Foo()
    {
    }

    public Foo(object parameter)
    {
    }

    public Foo(object parameter1, object parameter2)
        : this(parameter1)
    {
    }
}
```

#### After

```csharp
public class Foo
{
    private string _bar;

    public Foo(string bar)
    {
        _bar = bar;
    }

    public Foo(object parameter, string bar)
    {
        _bar = bar;
    }

    public Foo(object parameter1, object parameter2, string bar)
        : this(parameter1, bar)
    {
        _bar = bar;
    }
}
```

#### Initialize local with default value \(RR0060\)

* **Syntax**: local declaration without initializer
* **Span**: identifier
![Initialize local with default value](../../images/refactorings/InitializeLocalWithDefaultValue.png)

#### Inline alias expression \(RR0061\)

* **Syntax**: using alias directive
* **Span**: identifier
![Inline alias expression](../../images/refactorings/InlineAliasExpression.png)

#### Inline constant \(RR0181\)

* **Syntax**: constant declaration
![Inline constant](../../images/refactorings/InlineConstant.png)

#### Inline constant value \(RR0127\)

* **Syntax**: expression that has constant value

#### Before

```csharp
public const string Value = "x";

void Foo()
{
    string x = Value;
}
```

#### After

```csharp
public const string Value = "x";

void Foo()
{
    string x = "x";
}
```

#### Inline method \(RR0062\)

* **Syntax**: method invocation
![Inline method](../../images/refactorings/InlineMethod.png)

#### Inline property \(RR0198\)

* **Syntax**: property access
![Inline property](../../images/refactorings/InlineProperty.png)

#### Inline using static \(RR0180\)

* **Syntax**: using static directive
![Inline using static](../../images/refactorings/InlineUsingStatic.png)

#### Insert string interpolation \(RR0063\)

* **Syntax**: string literal, interpolated string
![Insert string interpolation](../../images/refactorings/InsertInterpolationIntoStringLiteral.png)

![Insert string interpolation](../../images/refactorings/InsertInterpolationIntoInterpolatedString.png)

#### Introduce and initialize field \(RR0064\)

* **Syntax**: constructor parameter
![Introduce and initialize field](../../images/refactorings/IntroduceAndInitializeField.png)

#### Introduce and initialize property \(RR0065\)

* **Syntax**: constructor parameter
![Introduce and initialize property](../../images/refactorings/IntroduceAndInitializeProperty.png)

#### Introduce constructor \(RR0066\)

* **Syntax**: field, property
![Introduce constructor](../../images/refactorings/IntroduceConstructor.png)

#### Introduce field to lock on \(RR0067\)

* **Syntax**: lock statement
* **Span**: missing expression
![Introduce field to lock on](../../images/refactorings/IntroduceFieldToLockOn.png)

#### Introduce local variable \(RR0068\)

* **Syntax**: expression statement, expression in using statement
![Introduce local variable](../../images/refactorings/IntroduceLocalVariable.png)

#### Invert binary expression \(RR0079\)

* **Syntax**: logical and/or expression
![Invert binary expression](../../images/refactorings/InvertBinaryExpression.png)

#### Invert boolean literal \(RR0080\)

* **Syntax**: boolean literal
![Invert boolean literal](../../images/refactorings/InvertBooleanLiteral.png)

#### Invert conditional expression \(RR0160\)

* **Syntax**: conditional expression
* **Span**: condition
![Invert conditional expression](../../images/refactorings/InvertConditionalExpression.png)

#### Invert if \(RR0189\)

* **Syntax**: if statement
* **Span**: if keyword

#### Before

```csharp
if (condition1)
{
    if (condition2)
    {
        Foo();
    }
}
```

#### After

```csharp
if (!condition1)
{
    return;
}

if (!condition2)
{
    return;
}

Foo();
```

- - -

#### Before

```csharp
if (!condition1)
{
    return;
}

if (!condition2)
{
    return;
}

Foo();
```

#### After

```csharp
if (condition1)
{
    if (condition2)
    {
        Foo();
    }
}
```

#### Invert if\-else \(RR0162\)

* **Syntax**: if\-else statement
* **Span**: if keyword
![Invert if-else](../../images/refactorings/InvertIfElse.png)

#### Invert is expression \(RR0081\)

* **Syntax**: is expression
* **Span**: operator
![Invert is expression](../../images/refactorings/InvertIsExpression.png)

#### Invert LINQ method call \(RR0116\)

* **Syntax**: System\.Linq\.Enumerable\.Any\(Func\<T, bool>\) or System\.Linq\.Enumerable\.All\(Func\<T, bool>\)
* **Span**: method name

#### Before

```csharp
if (items.Any(predicate))
{
}
```

#### After

```csharp
if (items.All(!predicate))
{
}
```

- - -

#### Before

```csharp
if (items.All(predicate))
{
}
```

#### After

```csharp
if (items.Any(!predicate))
{
}
```

#### Invert operator \(RR0082\)

* **Syntax**: \!=, &&, \|\|, \<, \<=, ==, >, >=
![Invert operator](../../images/refactorings/InvertOperator.png)

#### Invert prefix/postfix unary operator \(RR0134\)

* **Syntax**: prefix/postfix unary expression
* **Span**: operator token

#### Before

```csharp
int i = 0;

i++;
```

#### After

```csharp
int i = 0;

i--;
```

- - -

#### Before

```csharp
int i = 0;

++i;
```

#### After

```csharp
int i = 0;

--i;
```

#### Join string expressions \(RR0078\)

* **Syntax**: concatenated string expressions
![Join string expressions](../../images/refactorings/JoinStringExpressions.png)

![Join string expressions](../../images/refactorings/JoinStringLiterals.png)

![Join string expressions](../../images/refactorings/JoinStringLiteralsIntoMultilineStringLiteral.png)

#### Make member abstract \(RR0069\)

* **Syntax**: non\-abstract indexer/method/property in abstract class
* **Span**: indexer/method/property header
![Make member abstract](../../images/refactorings/MakeMemberAbstract.png)

#### Make member virtual \(RR0070\)

* **Syntax**: method declaration, indexer declaration
![Make member virtual](../../images/refactorings/MakeMemberVirtual.png)

#### Merge assignment expression with return statement \(RR0073\)

* **Syntax**: assignment expression followed with return statement
![Merge assignment expression with return statement](../../images/refactorings/MergeAssignmentExpressionWithReturnStatement.png)

#### Merge attributes \(RR0074\)

* **Syntax**: selected attribute lists
![Merge attributes](../../images/refactorings/MergeAttributes.png)

#### Merge 'if' statements \(RR0075\)

* **Syntax**: selected 'if' statements

#### Before

```csharp
if (x)
{
    return false;
}

if (y)
{
    return false;
}

return true;
```

#### After

```csharp
if (x || y)
{
    return false;
}

return true;
```

#### Merge if with parent if \(RR0196\)

* **Syntax**: if statement
* **Span**: if keyword

#### Before

```csharp
if (x)
{
    if (y)
    {
    }
}
else
{
}
```

#### After

```csharp
if (x && y)
{
}
else
{
}
```

#### Merge interpolation into interpolated string \(RR0076\)

* **Syntax**: interpolation
![Merge interpolation into interpolated string](../../images/refactorings/MergeInterpolationIntoInterpolatedString.png)

#### Merge local declarations \(RR0077\)

* **Syntax**: local declarations with same type
![Merge local declarations](../../images/refactorings/MergeLocalDeclarations.png)

#### Move unsafe context to containing declaration \(RR0202\)

* **Syntax**: unsafe declaration
* **Span**: unsafe modifier

#### Before

```csharp
public class Foo
{
  public unsafe void Bar()
  {
  }
}
```

#### After

```csharp
public unsafe class Foo
{
  public void Bar()
  {
  }
}
```

#### Notify when property change \(RR0083\)

* **Syntax**: property in class/struct that implements System\.ComponentModel\.INotifyPropertyChanged
* **Span**: setter
![Notify when property change](../../images/refactorings/NotifyWhenPropertyChange.png)

#### Parenthesize expression \(RR0084\)

* **Syntax**: selected expression
![Parenthesize expression](../../images/refactorings/ParenthesizeExpression.png)

#### Promote local to parameter \(RR0085\)

* **Syntax**: local declaration in method
![Promote local to parameter](../../images/refactorings/PromoteLocalToParameter.png)

#### Remove all comments \(RR0086\)

* **Syntax**: singleline/multiline comment, singleline/multiline documentation documentation comment
![Remove all comments](../../images/refactorings/RemoveAllComments.png)

#### Remove all comments \(except documentation comments\) \(RR0087\)

* **Syntax**: singleline/multiline comment
![Remove all comments (except documentation comments)](../../images/refactorings/RemoveAllCommentsExceptDocumentationComments.png)

#### Remove all documentation comments \(RR0088\)

* **Syntax**: singleline/multiline documentation comment
![Remove all documentation comments](../../images/refactorings/RemoveAllDocumentationComments.png)

#### Remove all member declarations \(RR0089\)

* **Syntax**: namespace, class, struct, interface
* **Span**: opening or closing brace
![Remove all member declarations](../../images/refactorings/RemoveAllMemberDeclarations.png)

#### Remove all preprocessor directives \(RR0090\)

* **Syntax**: preprocessor directive
![Remove all preprocessor directives](../../images/refactorings/RemoveAllPreprocessorDirectives.png)

#### Remove all region directives \(RR0091\)

* **Syntax**: region directive
![Remove all region directives](../../images/refactorings/RemoveAllRegionDirectives.png)

#### Remove all statements \(RR0092\)

* **Syntax**: method, constructor, operator
* **Span**: opening or closing brace
![Remove all statements](../../images/refactorings/RemoveAllStatements.png)

#### Remove all switch sections \(RR0093\)

* **Syntax**: switch statement
* **Span**: opening or closing brace
![Remove all switch sections](../../images/refactorings/RemoveAllSwitchSections.png)

#### Remove async/await \(RR0209\)

* **Syntax**: method declaration, local function, lambda, anonymous method
* **Span**: async keyword

#### Before

```csharp
class C
{
    async Task<object> FooAsync()
    {
        return await BarAsync().ConfigureAwait(false);
    }
}
```

#### After

```csharp
class C
{
    Task<object> FooAsync()
    {
        return BarAsync();
    }
}
```

#### Remove braces \(RR0094\)

* **Syntax**: do statement, else clause, fixed statement, for statement, foreach statement, if statement, lock statement, using statement, while statement
* **Span**: block with a single statement
![Remove braces](../../images/refactorings/RemoveBraces.png)

#### Remove braces from if\-else \(RR0095\)

* **Syntax**: if\-else chain
* **Span**: embedded statement
![Remove braces from if-else](../../images/refactorings/RemoveBracesFromIfElse.png)

#### Remove braces from switch section \(RR0096\)

* **Syntax**: switch section
* **Span**: case or default keyword
![Remove braces from switch section](../../images/refactorings/RemoveBracesFromSwitchSection.png)

#### Remove braces from switch sections \(RR0097\)

* **Syntax**: switch statement
* **Span**: case or default keyword
![Remove braces from switch sections](../../images/refactorings/RemoveBracesFromSwitchSections.png)

#### Remove comment \(RR0098\)

* **Syntax**: singleline/multiline comment, singleline/multiline xml documentation comment
![Remove comment](../../images/refactorings/RemoveComment.png)

#### Remove condition from last else clause \(RR0099\)

* **Syntax**: else clause
* **Span**: else keyword
![Remove condition from last else clause](../../images/refactorings/RemoveConditionFromLastElse.png)

#### Remove directive and related directives \(RR0100\)

* **Syntax**: preprocessor directive, region directive
![Remove directive and related directives](../../images/refactorings/RemoveDirectiveAndRelatedDirectives.png)

#### Remove empty lines \(RR0101\)

* **Syntax**: selected lines
![Remove empty lines](../../images/refactorings/RemoveEmptyLines.png)

#### Remove enum member value\(s\) \(RR0199\)

* **Syntax**: selected enum member\(s\)

#### Before

```csharp
public enum Foo
{
    One = 1,
    Two = 2,
    Three = 3
}
```

#### After

```csharp
public enum Foo
{
    One,
    Two,
    Three
}
```

#### Remove interpolation \(RR0102\)

* **Syntax**: string interpolation
* **Span**: opening or closing brace
![Remove interpolation](../../images/refactorings/RemoveInterpolation.png)

#### Remove member \(RR0103\)

* **Syntax**: method, constructor, property, indexer, operator, event, namespace, class, struct, interface
* **Span**: opening or closing brace
![Remove member](../../images/refactorings/RemoveMember.png)

#### Remove member declarations above/below \(RR0104\)

* **Syntax**: empty line between member declarations
![Remove member declarations above/below](../../images/refactorings/RemoveMemberDeclarations.png)

#### Remove parameter name from argument \(RR0105\)

* **Syntax**: selected argument\(s\)
![Remove parameter name from argument](../../images/refactorings/RemoveParameterNameFromArgument.png)

#### Remove parentheses \(RR0106\)

* **Syntax**: parenthesized expression
* **Span**: opening or closing parenthesis
![Remove parentheses](../../images/refactorings/RemoveParentheses.png)

#### Remove property initializer \(RR0107\)

* **Syntax**: property initializer
![Remove property initializer](../../images/refactorings/RemovePropertyInitializer.png)

#### Remove region \(RR0108\)

* **Syntax**: region directive
![Remove region](../../images/refactorings/RemoveRegion.png)

#### Remove statement \(RR0109\)

* **Syntax**: do statement, fixed statement, for statement, foreach statement, checked statement, if statement, lock statement, switch statement, try statement, unchecked statement, unsafe statement, using statement, while statement
* **Span**: open/close brace
![Remove statement](../../images/refactorings/RemoveStatement.png)

#### Remove statements from switch sections \(RR0110\)

* **Syntax**: selected switch sections
![Remove statements from switch sections](../../images/refactorings/RemoveStatementsFromSwitchSections.png)

#### Rename backing field according to property name \(RR0111\)

* **Syntax**: field identifier inside property declaration
![Rename backing field according to property name](../../images/refactorings/RenameBackingFieldAccordingToPropertyName.png)

#### Rename identifier according to type name \(RR0112\)

* **Syntax**: foreach statement, local/field/constant declaration
* **Span**: identifier
![Rename identifier according to type name](../../images/refactorings/RenameForEachIdentifierAccordingToTypeName.png)

![Rename identifier according to type name](../../images/refactorings/RenameFieldIdentifierAccordingToTypeName.png)

#### Rename method according to type name \(RR0113\)

* **Syntax**: method
![Rename method according to type name](../../images/refactorings/RenameMethodAccordingToTypeName.png)

#### Rename parameter according to its type name \(RR0114\)

* **Syntax**: parameter
* **Span**: parameter identifier
![Rename parameter according to its type name](../../images/refactorings/RenameParameterAccordingToTypeName.png)

#### Rename property according to type name \(RR0115\)

* **Syntax**: property identifier
![Rename property according to type name](../../images/refactorings/RenamePropertyAccordingToTypeName.png)

#### Replace as expression with cast expression \(RR0117\)

* **Syntax**: as expression
![Replace as expression with cast expression](../../images/refactorings/ReplaceAsWithCast.png)

#### Replace cast expression with as expression \(RR0118\)

* **Syntax**: cast expression
![Replace cast expression with as expression](../../images/refactorings/ReplaceCastWithAs.png)

#### Replace conditional expression with expression \(RR0119\)

* **Syntax**: conditional expression
* **Span**: selected true/false expression
![Replace conditional expression with expression](../../images/refactorings/ReplaceConditionalExpressionWithExpression.png)

#### Replace constant with field \(RR0121\)

* **Syntax**: constant declaration
![Replace constant with field](../../images/refactorings/ReplaceConstantWithField.png)

#### Replace equals expression with string\.Equals \(RR0124\)

* **Syntax**: equals expression, not equals expression
* **Span**: operator
![Replace equals expression with string.Equals](../../images/refactorings/ReplaceEqualsExpressionWithStringEquals.png)

#### Replace equals expression with string\.IsNullOrEmpty \(RR0125\)

* **Syntax**: equals expression, not equals expression
* **Span**: operator
![Replace equals expression with string.IsNullOrEmpty](../../images/refactorings/ReplaceEqualsExpressionWithStringIsNullOrEmpty.png)

#### Replace equals expression with string\.IsNullOrWhiteSpace \(RR0126\)

* **Syntax**: equals expression, not equals expression
* **Span**: operator
![Replace equals expression with string.IsNullOrWhiteSpace](../../images/refactorings/ReplaceEqualsExpressionWithStringIsNullOrWhiteSpace.png)

#### Replace interpolated string with interpolation expression \(RR0135\)

* **Syntax**: interpolated string with single interpolation and no text
* **Span**: interpolation
![Replace interpolated string with interpolation expression](../../images/refactorings/ReplaceInterpolatedStringWithInterpolationExpression.png)

#### Replace method with property \(RR0138\)

* **Syntax**: method
* **Span**: method header
![Replace method with property](../../images/refactorings/ReplaceMethodWithProperty.png)

#### Replace object creation with default value \(RR0185\)

* **Syntax**: object creation expression

#### Before

```csharp
var x = new object();
```

#### After

```csharp
object x = null;
```

- - -

#### Before

```csharp
var arr = new object[0];
```

#### After

```csharp
object[] arr = null;
```

#### Replace prefix operator to postfix operator \(RR0140\)

* **Syntax**: prefix/postfix unary expression
![Replace prefix operator to postfix operator](../../images/refactorings/ReplacePrefixOperatorWithPostfixOperator.png)

#### Replace property with method \(RR0141\)

* **Syntax**: read\-only property
* **Span**: property header
![Replace property with method](../../images/refactorings/ReplacePropertyWithMethod.png)

#### Reverse 'for' statement \(RR0152\)

* **Syntax**: for statement
![Reverse 'for' statement](../../images/refactorings/ReverseForStatement.png)

#### Simplify if \(RR0153\)

* **Syntax**: if statement
* **Span**: top if keyword or selected if statement
![Simplify if](../../images/refactorings/SimplifyIf.png)

#### Sort case labels \(RR0207\)

* **Syntax**: selected case labels with string literal or enum field

#### Before

```csharp
bool Foo(string s)
{
    switch (s)
    {
        case "d":
        case "b":
        case "a":
        case "c":
            return true;
        default:
            return false;
    }
}
```

#### After

```csharp
bool Foo(string s)
{
    switch (s)
    {
        case "a":
        case "b":
        case "c":
        case "d":
            return true;
        default:
            return false;
    }
}
```

#### Sort member declarations \(RR0155\)

* **Syntax**: namespace declarations, class declarations, struct declarations, interface declarations, enum declarations
* **Span**: selected member declarations
![Sort member declarations](../../images/refactorings/SortMembersByKind.png)

![Sort member declarations](../../images/refactorings/SortMembersByName.png)

![Sort member declarations](../../images/refactorings/SortEnumMembersByName.png)

![Sort member declarations](../../images/refactorings/SortEnumMembersByValue.png)

#### Split attributes \(RR0156\)

* **Syntax**: selected attribute list
![Split attributes](../../images/refactorings/SplitAttributes.png)

#### Split declaration and initialization \(RR0194\)

* **Syntax**: local variable declaration
* **Span**: equals token

#### Before

```csharp
var s = GetValue();
```

#### After

```csharp
string s;
s = GetValue();
```

#### Split if statement \(RR0184\)

* **Syntax**: if statement that has logical or expression as a condition
* **Span**: top if keyword or selected if statement
![Split if statement](../../images/refactorings/SplitIfStatement.png)

#### Split if\-else \(RR0190\)

* **Syntax**: if statement
* **Span**: selected if statement or topmost if keyword

#### Before

```csharp
if (condition1)
{
    return Foo1();
{
else if (condition2)
{
    return Foo2();
}
else
{
    return false;
}
```

#### After

```csharp
if (condition1)
{
    return Foo1();
{

if (condition2)
{
    return Foo2();
}

return false;
```

#### Split switch labels \(RR0157\)

* **Syntax**: selected switch labels
![Split switch labels](../../images/refactorings/SplitSwitchLabels.png)

#### Split variable declaration \(RR0158\)

* **Syntax**: local declaration, field declaration, event field declaration
![Split variable declaration](../../images/refactorings/SplitLocalDeclaration.png)

#### Swap binary operands \(RR0159\)

* **Syntax**: binary expression
* **Span**: binary operator

#### Before

```csharp
if (x && y)
{
{
```

#### After

```csharp
if (y && x)
{
{
```

#### Swap member declarations \(RR0161\)

* **Syntax**: empty line between member declarations
![Swap member declarations](../../images/refactorings/SwapMemberDeclarations.png)

#### Uncomment multi\-line comment \(RR0200\)

* **Syntax**: multi\-line comment

#### Before

```csharp
/*string s = null;*/
```

#### After

```csharp
string s = null;
```

#### Uncomment single\-line comment \(RR0163\)

* **Syntax**: single\-line comment\(s\)
![Uncomment single-line comment](../../images/refactorings/UncommentSingleLineComment.png)

#### Use "" instead of string\.Empty \(RR0168\)

* **Syntax**: string\.Empty field
![Use "" instead of string.Empty](../../images/refactorings/UseEmptyStringLiteralInsteadOfStringEmpty.png)

#### Use C\# 6\.0 dictionary initializer \(RR0191\)

* **Syntax**: collection initializer

#### Before

```csharp
var dic = new Dictionary<int, string>() { { 0, "0" } };
```

#### After

```csharp
var dic = new Dictionary<int, string>() { [0] = "0" };
```

#### Use coalesce expression instead of if \(RR0165\)

* **Syntax**: if statement
* **Span**: top if keyword or selected if statement
![Use coalesce expression instead of if](../../images/refactorings/UseCoalesceExpressionInsteadOfIf.png)

#### Use constant instead of field \(RR0128\)

* **Syntax**: read\-only field
![Use constant instead of field](../../images/refactorings/UseConstantInsteadOfField.png)

#### Use element access instead of 'First/Last'ElementAt' method \(RR0167\)

* **Syntax**: First/Last/ElementAt method invocation
* **Span**: method name
![Use element access instead of 'First/Last'ElementAt' method](../../images/refactorings/UseElementAccessInsteadOfEnumerableMethod.png)

#### Use enumerator explicitly \(RR0206\)

* **Syntax**: foreach statement
* **Span**: foreach keyword

#### Before

```csharp
foreach (var item in items)
{
    yield return item;
}
```

#### After

```csharp
using (var en = items.GetEnumerator())
{
    while (en.MoveNext())
    {
        yield return item;
    }
}
```

#### Use lambda expression instead of anonymous method \(RR0170\)

* **Syntax**: anonymous method
* **Span**: delegate keyword
![Use lambda expression instead of anonymous method](../../images/refactorings/UseLambdaExpressionInsteadOfAnonymousMethod.png)

#### Use List\<T> instead of yield \(RR0183\)

* **Syntax**: yield return, yield break
![Use List\<T> instead of yield](../../images/refactorings/UseListInsteadOfYield.png)

#### Use StringBuilder instead of concatenation \(RR0182\)

* **Syntax**: string concatenation
![Use StringBuilder instead of concatenation](../../images/refactorings/UseStringBuilderInsteadOfConcatenation.png)

#### Wrap arguments \(RR0048\)

* **Syntax**: argument list

#### Before

```csharp
M(x, y, z);
```

#### After

```csharp
M(
    x,
    y,
    z);
```

- - -

#### Before

```csharp
void M(
    x,
    y,
    z);
```

#### After

```csharp
void M(x, y, z);
```

#### Wrap binary expression \(RR0049\)

* **Syntax**: logical and/or expression, bitwise and/or expression

#### Before

```csharp
if (x && y && z)
{
}
```

#### After

```csharp
if (x
    && y
    && z)
{
}
```

- - -

#### Before

```csharp
if (x
    && y
    && z)
{
}
```

#### After

```csharp
if (x && y && z)
{
}
```

#### Wrap call chain \(RR0051\)

* **Syntax**: call chain

#### Before

```csharp
x.M().N().O()
```

#### After

```csharp
x
    .M()
    .N()
    .O()
```

- - -

#### Before

```csharp
x
    .M()
    .N()
    .O()
```

#### After

```csharp
x.M().N().O()
```

#### Wrap conditional expression \(RR0050\)

* **Syntax**: conditional expression

#### Before

```csharp
x ? y : z
```

#### After

```csharp
x
    ? y
    : z
```

#### Wrap constraint clauses \(RR0187\)

* **Syntax**: type parameter constraint clause

#### Before

```csharp
private void Foo<T1, T2, T3>() where T1 : class where T2 : class where T3 : class
{
}
```

#### After

```csharp
private void Foo<T1, T2, T3>()
    where T1 : class
    where T2 : class
    where T3 : class
{
}
```

#### Wrap in \#if directive \(RR0174\)

* **Syntax**: selected lines
![Wrap in #if directive](../../images/refactorings/WrapInIfDirective.png)

#### Wrap in condition \(RR0172\)

* **Syntax**: selected statements
![Wrap in condition](../../images/refactorings/WrapInCondition.png)

#### Wrap in else clause \(RR0173\)

* **Syntax**: statement
![Wrap in else clause](../../images/refactorings/WrapInElseClause.png)

#### Wrap in region \(RR0175\)

* **Syntax**: selected lines
![Wrap in region](../../images/refactorings/WrapInRegion.png)

#### Wrap in try\-catch \(RR0176\)

* **Syntax**: selected statements
![Wrap in try-catch](../../images/refactorings/WrapInTryCatch.png)

#### Wrap in using statement \(RR0177\)

* **Syntax**: local declaration of type that implements IDisposable
![Wrap in using statement](../../images/refactorings/WrapInUsingStatement.png)

#### Wrap initializer expressions \(RR0052\)

* **Syntax**: initializer

#### Before

```csharp
var x = new string[] { "a", "b", "c" }
```

#### After

```csharp
var x = new string[]
{
    "a",
    "b",
    "c"
}
```

- - -

#### Before

```csharp
var x = new string[]
{
    "a",
    "b",
    "c"
}
```

#### After

```csharp
var x = new string[] { "a", "b", "c" }
```

#### Wrap parameters \(RR0053\)

* **Syntax**: parameter list

#### Before

```csharp
void M(object x, object y, object z)
{
}
```

#### After

```csharp
void M(
    object x,
    object y,
    object z)
{
}
```

- - -

#### Before

```csharp
void M(
    object x,
    object y,
    object z)
{
}
```

#### After

```csharp
void M(object x, object y, object z)
{
}
```


*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*