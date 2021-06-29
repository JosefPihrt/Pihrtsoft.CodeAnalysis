
# `spellcheck` Command

Searches the specified project or solution for possible misspellings or typos.

## Synopsis

```shell
roslynator spellcheck <PROJECT|SOLUTION>
[--case-sensitive]
[--culture]
[-d|--dry-run]
[--file-log]
[--file-log-verbosity]
[--ignored-projects]
[--ignored-scope]
[--include-generated-code]
[--interactive]
[--language]
[--min-word-length]
[-m|--msbuild-path]
[--projects]
[-p|--properties]
[--scope]
[-v|--verbosity]
[--visibility]
--words
```

## Arguments

**`PROJECT|SOLUTION`**

Path to one or more project/solution files.

### Required Options

**`--words`** <PATH>

Specified path to file and/or directory that contains list of allowed words.

### Optional Options

**`--case-sensitive`**

Specifies case-sensitive matching.

**`--culture`** <CULTURE_ID>

Defines culture that should be used to display diagnostic message.

**`-d|--dry-run`**

Display misspellings and typos but do not save changes to a disk.
 
**`--ignored-projects`** <PROJECT_NAME>

Defines projects that should not be analyzed.

**`--ignored-scope`** <SCOPE>

Defines syntax that should not be analyzed. Allowed values are `comment`, `type`, `member`, `local`, `parameter`, `non-symbol` and `symbol`.

**`--include-generated-code`**

Indicates whether generated code should be spellchecked.

**`--interactive`**

Enable editing of a replacement.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`--min-word-length`** <NUM>

Specifies minimal word length to be checked. Default value is 3.

**`-m|--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild. This is usually required when multiple versions of Visual Studio are installed.

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`--scope`** <SCOPE>

Defines syntax that should be analyzed. Allowed values are `comment`, `type`, `member`, `local`, `parameter`, `non-symbol` and `symbol`.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

**`--visibility`** <VISIBILITY>

Defines a  maximal visibility of a symbol to be fixable. Allowed values are `public`, `internal` or `private`. Default value is `public`.

## Redirected/Piped Input

Redirected/piped input will be used as a list of project/solution paths separated with newlines.

## How to Suppress Spellchecking

Possible misspelling or typo is reported as a diagnostic `RCS2001`.
Thus it is possible to [suppress](../HowToConfigureAnalyzers.md#how-to-suppress-a-diagnostic) it as any other diagnostic. 

## List of Allowed Words

* It is required to specify one or more wordlists (parameter `--words`).
* Wordlist is defined as a text file that contains list of values separated with newlines.
* Each value is either a valid word (for example `misspell`) or a fix in a format `<ERROR>: <FIX>` (for example `mispell: misspell`).
* Word matching is case-insensitive by default (use option `--case-sensitive` to specify case-sensitive matching).
* It is recommended to use [Wordb](https://github.com/JosefPihrt/Wordb/tree/main/data) wordlists that are specifically tailored to be used for spellchecking.

## See Also

* [Roslynator Command-Line Interface](README.md)
