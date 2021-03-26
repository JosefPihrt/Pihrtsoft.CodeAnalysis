
# `generate-doc` Command

Generates documentation files from specified assemblies.

## Synopsis

```shell
roslynator generate-doc <PROJECT|SOLUTION>
-h|--heading
-o|--output
[--additional-xml-documentation]
[--depth]
[--file-log]
[--file-log-verbosity]
[--ignored-member-parts]
[--ignored-names]
[--ignored-namespace-parts]
[--ignored-projects]
[--ignored-root-parts]
[--ignored-type-parts]
[--include-all-derived-types]
[--include-containing-namespace]
[--include-ienumerable]
[--include-inherited-interface-members]
[--include-system-namespace]
[--inheritance-style]
[--language]
[--max-derived-types]
[-m|--msbuild-path]
[--no-delete]
[--no-mark-obsolete]
[--no-precedence-for-system]
[--no-wrap-base-types]
[--no-wrap-constraints]
[--omit-attribute-arguments]
[--omit-inherited-attributes]
[--omit-member-parts]
[--preferred-culture]
[--projects]
[-p|--properties]
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

**`[--additional-xml-documentation]`** `<XML_DOCUMENTATION_FILE>`

Defines one or more xml documentation files that should be included. These files can contain a documentation for namespaces, for instance.

**`[--depth]`** `{member|type|namespace}`

Defines a depth of a documentation. Default value is `member`.

**`[--ignored-member-parts]`** `{overloads containing-type containing-assembly obsolete-message summary declaration type-parameters parameters return-value implements attributes exceptions examples remarks see-also}`

Defines parts of a member documentation that should be excluded.

**`[--ignored-names]`** `<FULLY_QUALIFIED_METADATA_NAME>`

Defines a list of metadata names that should be excluded from a documentation. Namespace of type names can be specified.

**`[--ignored-namespace-parts]`** `{content containing-namespace summary examples remarks classes structs interfaces enums delegates see-also}`

Defines parts of a namespace documentation that should be excluded.

**`--ignored-projects`** <PROJECT_NAME>

Defines projects that should be skipped.

**`[--ignored-root-parts]`** `{content namespaces class-hierarchy types other}`

Defines parts of a root documentation that should be excluded.

**`[--ignored-type-parts]`** `{content containing-namespace containing-assembly obsolete-message summary declaration type-parameters parameters return-value inheritance attributes derived implements examples remarks constructors fields indexers properties methods operators events explicit-interface-implementations extension-methods classes structs interfaces enums delegates see-also}`

Defines parts of a type documentation that should be excluded.

**`[--include-all-derived-types]`**

Indicates whether all derived types should be included in the list of derived types. By default only types that directly inherits from a specified type are displayed.

**`[--include-containing-namespace]`** `{class-hierarchy containing-type parameter return-type base-type attribute derived-type implemented-interface implemented-member exception see-also all}`

Defines parts of a documentation that should include containing namespace.

**`[--include-ienumerable]`**

Indicates whether interface `System.Collections.IEnumerable` should be included in a documentation if a type also implements interface `System.Collections.Generic.IEnumerable<T>`.

**`[--include-inherited-interface-members]`**

Indicates whether inherited interface members should be displayed in a list of members.

**`[--include-system-namespace]`**

Indicates whether namespace should be included when a type is directly contained in namespace 'System'.

**`[--inheritance-style]`** `{horizontal|vertical}`

Defines a style of a type inheritance. Default value is `horizontal`.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`[--max-derived-types]`** <MAX_DERIVED_TYPES>

Defines maximum number derived types that should be displayed. Default value is `5`.

**`-m|--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild (usually multiple installations of Visual Studio).

**`[--no-delete]`**

Indicates whether output directory should not be deleted at the beginning of the process.

**`[--no-mark-obsolete]`**

Indicates whether obsolete types and members should not be marked as `[deprecated]`.

**`[--no-precedence-for-system]`**

Indicates whether symbols contained in `System` namespace should be ordered as any other symbols and not before other symbols.

**`[--no-wrap-base-list]`**

Indicates whether base types should not be wrapped.

**`[--no-wrap-constraints]`**

Indicates whether constraints should not be wrapped.

**`[--omit-attribute-arguments]`**

Indicates whether attribute arguments should be omitted when displaying an attribute.

**`[--omit-inherited-attributes]`**

Indicates whether inherited attributes should be omitted.

**`[--omit-member-parts]`**

Defines parts of member definition that should be omitted. Allowed values are constant-value, implements, inherited-from and overrides.

**`[--preferred-culture]`** <CULTURE_ID>

Defines culture that should be used when searching for xml documentation files.

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`[--scroll-to-content]`**

Indicates whether a link should lead to the top of the documentation content.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

**`[--visibility]`** `{public|internal|private}`

Defines a visibility of a type or a member. Default value is `public`.

## See Also

* [Roslynator Command-Line Interface](README.md)
