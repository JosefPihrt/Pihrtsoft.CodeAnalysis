
# `list-symbols` Command

List symbols from the specified project or solution.

## Synopsis

```shell
roslynator list-symbols <PROJECT|SOLUTION>
[--depth]
[--empty-line-between-members]
[--external-assemblies]
[--file-log]
[--file-log-verbosity]
[--group-by-assembly]
[--ignored-attributes]
[--ignored-parts]
[--ignored-projects]
[--ignored-symbols]
[--documentation]
[--indent-chars]
[--language]
[--layout]
[-m|--msbuild-path]
[-o|--output]
[--projects]
[-p|--properties]
[-v|--verbosity]
[--visibility]
[--wrap-list]
```

## Arguments

**`PROJECT|SOLUTION`**

The project or solution to analyze.

### Optional Options

**`[--depth]`** `{member|type|namespace}`

Defines a depth of a list of symbols. Allowed values are member, type or namespace. Default value is member.

**`[--empty-line-between-members]`**

Indicates whether an empty line should be added between two member definitions.

**`--external-assemblies`** <ASSEMBLY_FILE>

Defines file name/path to external assemblies that should be included.

**`[--group-by-assembly]`**

Indicates whether symbols should be grouped by assembly.

**`[--ignored-attributes]`** `<FULLY_QUALIFIED_METADATA_NAME>`

Defines a list of attributes that should be ignored.

**`--ignored-parts`** <PROJECT_NAME>

Defines parts of a symbol definition that should be excluded. Allowed values are containing-namespace, containing-namespace-in-type-hierarchy, attributes, assembly-attributes, attribute-arguments, accessibility, modifiers, parameter-name, parameter-default-value, base-type, base-interfaces, constraints, trailing-semicolon, trailing-comma.

**`--ignored-projects`** <PROJECT_NAME>

Defines projects that should be skipped.

**`[--ignored-symbols]`** `<FULLY_QUALIFIED_METADATA_NAME>`

Defines a list of symbols that should be ignored. Namespace of types can be specified.

**`[--documentation]`**

Indicates whether a documentation should be included.

**`[--indent-chars]`** `<INDENT_CHARS>`

Defines characters that should be used for indentation. Default value is four spaces.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`--layout`** `{namespace-list|namespace-hierarchy|type-hierarchy}`

Defines layout of a list of symbol definitions. Allowed values are namespace-list, namespace-hierarchy or type-hierarchy. Default value is namespace-list.

**`-m|--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild (usually multiple installations of Visual Studio).

**`[-o|--output]`** `<OUTPUT_PATH>`

Defines path to file(s) that will store a list of symbol definitions.

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

**`[--visibility]`** `{public|internal|private}`

Defines a visibility of a type or a member. Default value is `public`.

**`[--wrap-list]`**

Specifies syntax lists that should be wrapped. Allowed values are attributes, parameters, base-types and constraints.

## See Also

* [Roslynator Command-Line Interface](README.md)
