
# `lloc` Command

Counts logical lines of code in the specified project or solution.

## Synopsis

```shell
roslynator lloc <PROJECT|SOLUTION>
[--ignored-projects]
[--include-comments]
[-g|--include-generated-code]
[--include-preprocessor-directives]
[--include-whitespace]
[--language]
[--file-log]
[--file-log-verbosity]
[-m|--msbuild-path]
[--projects]
[-p|--properties]
[-v|--verbosity]
```

## Arguments

**`PROJECT|SOLUTION`**

Path to one or more project/solution files.

### Optional Options

**`--ignored-projects`** <PROJECT_NAME>

Defines project names that should not be fixed.

**`--include-comments`**

Indicates whether a line that contains only comment should be counted.

**`-g|--include-generated-code`**

Indicates whether generated code should be counted.

**`--include-preprocessor-directives`**

Indicates whether preprocessor directive line should be counted.

**`--include-whitespace`**

Indicates whether white-space line should be counted.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`-m|--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild (usually multiple installations of Visual Studio).

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

## Redirected/Piped Input

Redirected/piped input will be used as a list of project/solution paths separated with newlines.

## See Also

* [Roslynator Command-Line Interface](README.md)
