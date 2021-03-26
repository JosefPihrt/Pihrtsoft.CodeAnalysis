
# `generate-doc-root` Command

Generates root documentation file from specified assemblies.

## Synopsis

```shell
roslynator generate-doc-root
-h|--heading
-o|--output
[--depth]
[--file-log]
[--file-log-verbosity]
[--ignored-names]
[--ignored-parts]
[--ignored-projects]
[--include-containing-namespace]
[--include-system-namespace]
[--language]
[-m|--msbuild-path]
[--no-mark-obsolete]
[--no-precedence-for-system]
[-p|--properties]
[--projects]
[--root-directory-url]
[--scroll-to-content]
[-v|--verbosity]
[--visibility]
```

## Arguments

**`PROJECT|SOLUTION`**

The project or solution to analyze.

## Options

### Required Options

**`-h|--heading`** `<ROOT_FILE_HEADING>`

Defines a heading of the root documentation file.

**`-o|--output`** `<OUTPUT_DIRECTORY>`

Defines a path for the output directory.

### Optional Options

**`[--depth]`** `{member|type|namespace}`

Defines a depth of a documentation. Default value is `member`.

**`[--ignored-names]`** `<FULLY_QUALIFIED_METADATA_NAME>`

Defines a list of metadata names that should be excluded from a documentation. Namespace of type names can be specified.

**`[--ignored-parts]`** `{content namespaces class-hierarchy types other}`

Defines parts of a root documentation that should be excluded.

**`--ignored-projects`** <PROJECT_NAME>

Defines projects that should be skipped.

**`[--include-containing-namespace]`** `{class-hierarchy}`

Defines parts of a documentation that should include containing namespace.

**`[--include-system-namespace]`**

Indicates whether namespace should be included when a type is directly contained in namespace 'System'.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`-m|--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild (usually multiple installations of Visual Studio).

**`[--no-mark-obsolete]`**

Indicates whether obsolete types and members should not be marked as `[deprecated]`.

**`[--no-precedence-for-system]`**

Indicates whether symbols contained in `System` namespace should be ordered as any other symbols and not before other symbols.

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`[--root-directory-url]`** <ROOT_DIRECTORY_URL>

Defines a relative url to the documentation root directory.

**`[--scroll-to-content]`**

Indicates whether a link should lead to the top of the documentation content.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

**`[--visibility]`** `{public|internal|private}`

Defines a visibility of a type or a member. Default value is `public`.

## See Also

* [Roslynator Command-Line Interface](README.md)
