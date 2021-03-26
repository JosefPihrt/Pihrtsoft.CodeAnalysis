
# `fix` Command

Fixes diagnostics in the specified project or solution.

## Synopsis

```shell
roslynator fix <PROJECT|SOLUTION>
[-a|--analyzer-assemblies]
[--batch-size]
[--culture]
[--diagnostic-fixer-map]
[--diagnostic-fix-map]
[--diagnostics-fixable-one-by-one]
[--file-banner]
[--fix-scope]
[--format]
[--ignore-analyzer-references]
[--ignore-compiler-errors]
[--ignored-compiler-diagnostics]
[--ignored-diagnostics]
[--ignored-projects]
[--language]
[--file-log]
[--file-log-verbosity]
[--max-iterations]
[-m|--msbuild-path]
[--projects]
[-p|--properties]
[--severity-level]
[--supported-diagnostics]
[-v|--verbosity]
```

## Arguments

**`PROJECT|SOLUTION`**

The project or solution to fix.

### Optional Options

**`-a|--analyzer-assemblies`** <PATH>

Defines one or more paths to:

* analyzer assembly
* directory that should be searched recursively for analyzer assemblies

**`--batch-size`** <BATCH_SIZE>

Defines maximum number of diagnostics that can be fixed in one batch.

**`--culture`** <CULTURE_ID>

Defines culture that should be used to display diagnostic message.

**`--diagnostic-fixer-map`** <DIAGNOSTIC_ID=FIXER_FULL_NAME>

Defines mapping between diagnostic and its fixer ((CodeFixProvider)[https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.codefixes.codefixprovider?view=roslyn-dotnet]).

If there are two (or more) fixers for a diagnostic and both provide a fix it is necessary to determine which one should be used to fix the diagnostic.
Set verbosity to 'diagnostic' to see which diagnostics cannot be fixed due to multiple fixers.

**`--diagnostic-fix-map`** <DIAGNOSTIC_ID=EQUIVALENCE_KEY>

Defines mapping between diagnostic and its fix ((CodeAction)[https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.codeactions.codeaction?view=roslyn-dotnet]).

If there are two (or more) fixes for a diagnostic it is necessary to determine which one should be used to fix the diagnostic.
Set verbosity to 'diagnostic' to see which diagnostics cannot be fixed due to multiple fixes.

**`--diagnostics-fixable-one-by-one`** <DIAGNOSTIC_ID>

Defines diagnostics that can be fixed even if there is no (FixAllProvider)[https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md] for them.

**`--file-banner`** <FILE_BANNER>

Defines text that should be at the top of the source file.

**`--fix-scope`** `{project|document}`

Defines a fix scope. Default value is `project`.

**`--format`**

Indicates whether each document should be formatted.

**`--ignore-analyzer-references`**

Indicates whether Roslynator should ignore analyzers that are referenced in projects.

**`--ignore-compiler-errors`**

Indicates whether fixing should continue even if compilation has errors.

**`--ignored-compiler-diagnostics`** <DIAGNOSTIC_ID>

Defines compiler diagnostics that should be ignored even if `--ignore-compiler-errors` is not set.

**`--ignored-diagnostics`** <DIAGNOSTIC_ID>

Defines diagnostics that should not be fixed.

**`--ignored-projects`** <PROJECT_NAME>

Defines projects that should not be fixed.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`--max-iterations`** <MAX_ITERATIONS>

Defines maximum numbers of fixing iterations.

**`-m|--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild (usually multiple installations of Visual Studio).

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`--severity-level`** `{hidden|info|warning|error}`

Defines minimally required severity for a diagnostic. Default value is `info`.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

## See Also

* [Roslynator Command-Line Interface](README.md)
