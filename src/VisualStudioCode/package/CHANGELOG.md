## 3.2.0 (2021-04-26)

* Publish [Roslynator Testing Framework](https://www.nuget.org/packages/Roslynator.Testing.CSharp.Xunit)
* Support editorconfig to configure analyzer options ([commit](https://github.com/JosefPihrt/Roslynator/commit/da88ce64e0b3975ad69e05a1d4cdcc761f358a09))
* Update references to Roslyn API to 3.8.0
* A bunch of bug fixes

### Analyzers

* Add option to invert analyzer [RCS1016](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1016.md) ([commit](https://github.com/JosefPihrt/Roslynator/commit/67a0fc5cfe9dd793cc6e504513ed6805678c1739))
* Add more cases to analyzer [RCS1218](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1218.md) ([commit](https://github.com/JosefPihrt/Roslynator/commit/37e8edb7a2eefdd4a7749dd6a3f5b473ebbdcc0a))
* Convert `!= null` to `is not null` ([RCS1248](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1248.md)) ([commit](https://github.com/JosefPihrt/Roslynator/commit/432a8fea3147447536dbb8fac47598ad1db68158))

### Code Fixes

* Add code fix for CS7036 ([commit](https://github.com/JosefPihrt/Roslynator/commit/9eae7307b9cab96c2d91e97aef8bda098c7e92d9))
* Add code fix for CS8632 ([commit](https://github.com/JosefPihrt/Roslynator/commit/2c1d9ca64d2305e1ce278e1db6563d82582c4613))
* Improve code fix for CS0029, CS0246 ([commit](https://github.com/JosefPihrt/Roslynator/commit/5557ad29412b5f758cb97da6e298e1f4b0d49e3d))
* Add option for code fix for CS1591 ([commit](https://github.com/JosefPihrt/Roslynator/commit/089dbed656556a526f236dce75eadffb4e1d78a0))

## 3.1.0 (2021-01-04)

* Add analyzer [RCS0056](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS0056.md) (Line is too long)
* Add option to suppress diagnostic from Unity script methods (RCS1213)
* Consider syntax `var foo = Foo.Parse(value)` as having obvious type `Foo`
* Update references to Roslyn API to 3.7.0

## 3.0.1 (2020-10-19)

* Add analyzer [RCS0055](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS0055.md) (Fix formatting of a binary expression chain)
* Add analyzer [RCS0054](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS0054.md) (Fix formatting of a call chain)
* Add analyzer [RCS0053](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS0053.md) (Fix formatting of a list)
* Add analyzer [RCS0052](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS0052.md) (Add newline before equals sign instead of after it (or vice versa))
* Add analyzer [RCS1248](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1248.md) (Use 'is null' pattern instead of comparison (or vice versa)) ([issue](https://github.com/JosefPihrt/Roslynator/issues/458))
* Add analyzer [RCS1247](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1247.md) (Fix documentation comment tag)
* Add analyzer option [RCS1207i](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1207i.md) (Convert method group to anonymous function)
* Add analyzer option [RCS1090i](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1090i.md) (Remove call to 'ConfigureAwait')
* Add analyzer option [RCS1018i](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1018i.md) (Remove accessibility modifiers) ([issue](https://github.com/JosefPihrt/Roslynator/issues/260))
* Add analyzer option [RCS1014i](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1014i.md) (Use implicitly typed array)
* Add analyzer option [RCS1014a](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1014a.md) (Use implicitly typed array (when type is obvious))
* Add analyzer option [RCS1078i](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1078i.md) (Use string.Empty instead of "")
* Add analyzer option [RCS1016a](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1016a.md) (Convert expression-body to block body when expression is multi-line)
* Add analyzer option [RCS1016b](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1016b.md) (Convert expression-body to block body when declaration is multi-line)
* Disable by default analyzer [RCS1207i](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1207i.md) (Convert method group to anonymous function)
* Remove analyzer [RCS1219](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1219.md) (Call 'Enumerable.Skip' and 'Enumerable.Any' instead of 'Enumerable.Count')
* Rename analyzer "Avoid 'null' on left side of binary expression" to "Constant values should be placed on right side of comparisons" [RCS1098](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1098.md)
* Rename analyzer "Simplify boolean expression" to "Unnecessary null check" [RCS1199](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1199.md) ([issue](https://github.com/JosefPihrt/Roslynator/issues/373))

* More syntax is considered as having obvious type:
  * string literal
  * character literal
  * boolean literal
  * implicit array creation that contains only expressions whose type is obvious

## 3.0.0 (2020-06-16)

* Update references to Roslyn API to 3.5.0
* Introduce concept of "[Analyzer Options](https://github.com/JosefPihrt/Roslynator/blob/master/docs/AnalyzerOptions.md)"
* Reassign ID for some analyzers.
  * See "[How to: Migrate Analyzers to Version 3.0](https://github.com/JosefPihrt/Roslynator/blob/master/docs/HowToMigrateAnalyzersToVersion3.md)"
* Remove references to Roslynator assemblies from omnisharp.json on uninstall

## 2.9.0 (2020-03-13)

* Switch to Roslyn 3.x libraries
* Add `Directory.Build.props` file
* Add open configuration commands to Command Palette (VS Code) ([PR](https://github.com/JosefPihrt/Roslynator/pull/648))

### Bug Fixes

* Fix key duplication/handle camel case names in `omnisharp.json` ([PR](https://github.com/JosefPihrt/Roslynator/pull/645))
* Use prefix unary operator instead of postfix unary operator ([RCS1089](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1089.md)) ([issue](https://github.com/JosefPihrt/Roslynator/issues/639))
* Cast of `this` to its interface cannot be null ([RCS1202](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1202.md)) ([issue](https://github.com/JosefPihrt/Roslynator/issues/640))
* Do not remove braces in switch section if it contains 'using variable' ([RCS1031](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1031.md)) ([issue](https://github.com/JosefPihrt/Roslynator/issues/632))

### New Analyzers

* [RCS1242](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1242.md) (DoNotPassNonReadOnlyStructByReadOnlyReference).
* [RCS1243](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1243.md) (DuplicateWordInComment).
* [RCS1244](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1244.md) (SimplifyDefaultExpression).
* [RCS1245](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1245.md) (SimplifyConditionalExpression2) ([issue](https://github.com/JosefPihrt/Roslynator/issues/612)).

### Analyzers

* Disable analyzer [RCS1057](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1057.md) by default ([issue](https://github.com/JosefPihrt/Roslynator/issues/590)).
* Merge analyzer [RCS1156](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1156.md) with [RCS1113](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1113.md) ([issue](https://github.com/JosefPihrt/Roslynator/issues/650)).
  * `x == ""` should be replaced with `string.IsNullOrEmpty(x)`
* Improve analyzer [RCS1215](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1215.md) ([commit](https://github.com/JosefPihrt/Roslynator/commit/0fdd97f9a62463f8b004abeb17a8b8509374c35a)).
  * `x == double.NaN` should be replaced with `double.IsNaN(x)`
* Enable [RCS1169](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1169.md) and [RCS1170](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1170.md) if the type is read-only struct ([commit](https://github.com/JosefPihrt/Roslynator/commit/f34e105433dbc65686369adf712b0b99d93eaef7)).
* Improve analyzer [RCS1077](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1077.md) ([commit](https://github.com/JosefPihrt/Roslynator/commit/3ee275442cb16f6a9104b42d582ba7d76d6df88c)).
  * `x.OrderBy(y => y).Reverse()` can be simplified to `x.OrderByDescending(y => y)`
  * `x.SelectMany(y => y).Count()` can be simplified to `x.Sum(y => y.Count)` if `x` has `Count` or `Length` property
* Improve analyzer [RCS1161](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1161.md) - Declare explicit enum value using `<<` operator ([commit](https://github.com/JosefPihrt/Roslynator/commit/6b78496efe1a2f2678f2ef2a71986e2bee006863)).
* Improve analyzer [RCS1036](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1036.md) - remove empty line between documentation comment and declaration ([commit](https://github.com/JosefPihrt/Roslynator/commit/de0f1205671281679866e92edd9337a7416409e6)).
* Improve analyzer [RCS1037](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1037.md) - remove trailing white-space from documentation comment ([commit](https://github.com/JosefPihrt/Roslynator/commit/c3f7d193ee37d04de7e2c698aab7f3e1e6350e80)).
* Improve analyzer [RCS1143](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1143.md) ([commit](https://github.com/JosefPihrt/Roslynator/commit/4c4281ebdf8eb0aa1a77d5e5bfda71bc66cce1df))
  * `x?.M() ?? default(int?)` can be simplified to `x?.M()` if `x` is a nullable struct.
* Improve analyzer [RCS1206](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1206.md) ([commit](https://github.com/JosefPihrt/Roslynator/commit/88dd4cea4df07f036a8296511410ccff70f8fefe))
  * `(x != null) ? x.M() : default(int?)` can be simplified to `x?.M()` if `x` is a nullable struct.

## 2.3.0 (2019-12-28)

* Automatically update configuration in omnisharp.json (VS Code) ([PR](https://github.com/JosefPihrt/Roslynator/pull/623)).

## 2.2.0 (2019-09-28)

* Enable configuration for non-Windows systems (VS Code).

### Analyzers

* Disable analyzer [RCS1029](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1029.md) (FormatBinaryOperatorOnNextLine) by default.

## 2.1.4 (2019-08-13)

* Initial release of Roslynator for VS Code.
