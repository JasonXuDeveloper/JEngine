# Section 9 — Source Generator Project Setup

## Overview

The JUI source generator project is a Roslyn IIncrementalGenerator targeting netstandard2.0. It houses all generators (ElementGenerator, StyleGenerator, InjectGenerator, BindingGenerator, EffectGenerator) in a single assembly alongside shared infrastructure: a UXML parser, a USS parser, diagnostic descriptors, and naming convention utilities. The incremental generator pattern ensures that only files affected by edits are re-processed, keeping IDE responsiveness high even in large projects.

## Dependencies

- Section 6 — Component Base Class (the generators emit partial class members that extend JUIComponent subclasses)

## File Structure

- `SourceGenerators/JEngine.JUI.Generators/JEngine.JUI.Generators.csproj`
- `SourceGenerators/JEngine.JUI.Generators/Parsers/UxmlParser.cs`
- `SourceGenerators/JEngine.JUI.Generators/Parsers/UssParser.cs`
- `SourceGenerators/JEngine.JUI.Generators/Parsers/NamingConventions.cs`
- `SourceGenerators/JEngine.JUI.Generators/DiagnosticDescriptors.cs`
- `SourceGenerators/JEngine.JUI.Generators/GeneratorUtilities.cs`

## API Design

### Project File (csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>
    <RootNamespace>JEngine.JUI.Generators</RootNamespace>
    <AssemblyName>JEngine.JUI.Generators</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.3.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

### UXML Parser

```csharp
namespace JEngine.JUI.Generators.Parsers;

/// <summary>
/// Parses UXML files and extracts named elements with their UI Toolkit types.
/// Used by ElementGenerator to produce typed fields and El constants.
/// </summary>
internal static class UxmlParser
{
    /// <summary>
    /// Represents a single named element extracted from a UXML file.
    /// </summary>
    internal readonly struct UxmlElement
    {
        /// <summary>The name attribute value from the UXML element (kebab-case).</summary>
        public string Name { get; }

        /// <summary>The UI Toolkit element type (e.g., "Label", "VisualElement", "Button").</summary>
        public string TypeName { get; }

        /// <summary>The namespace of the element type, or null for built-in types.</summary>
        public string TypeNamespace { get; }
    }

    /// <summary>
    /// Parses a UXML source text and returns all elements that have a non-empty name attribute.
    /// Elements without a name attribute are skipped.
    /// </summary>
    /// <param name="uxmlText">The raw UXML file content.</param>
    /// <returns>An immutable array of named UXML elements.</returns>
    internal static ImmutableArray<UxmlElement> Parse(string uxmlText);
}
```

### USS Parser

```csharp
namespace JEngine.JUI.Generators.Parsers;

/// <summary>
/// Parses USS (Unity Style Sheet) files and extracts class names and CSS custom properties.
/// Used by StyleGenerator to produce Cls constants and JUIVar typed accessors.
/// </summary>
internal static class UssParser
{
    /// <summary>A CSS class name extracted from a USS rule.</summary>
    internal readonly struct UssClass
    {
        /// <summary>The raw class name without the leading dot (e.g., "bar-fill--critical").</summary>
        public string RawName { get; }
    }

    /// <summary>A CSS custom property (variable) extracted from a USS rule.</summary>
    internal readonly struct UssVariable
    {
        /// <summary>The raw variable name including the -- prefix (e.g., "--bar-color").</summary>
        public string RawName { get; }

        /// <summary>The inferred USS value type (Color, Length, Float, String).</summary>
        public UssValueType ValueType { get; }
    }

    /// <summary>Supported USS variable value types for typed accessor generation.</summary>
    internal enum UssValueType
    {
        String,
        Color,
        Length,
        Float
    }

    /// <summary>
    /// Parses a USS source text and returns all class selectors found in rule declarations.
    /// </summary>
    /// <param name="ussText">The raw USS file content.</param>
    /// <returns>An immutable array of unique USS class names.</returns>
    internal static ImmutableArray<UssClass> ParseClasses(string ussText);

    /// <summary>
    /// Parses a USS source text and returns all CSS custom properties (--var-name: value).
    /// </summary>
    /// <param name="ussText">The raw USS file content.</param>
    /// <returns>An immutable array of USS variables with inferred types.</returns>
    internal static ImmutableArray<UssVariable> ParseVariables(string ussText);
}
```

### Naming Conventions

```csharp
namespace JEngine.JUI.Generators.Parsers;

/// <summary>
/// Converts between CSS/UXML naming conventions and C# naming conventions.
/// </summary>
internal static class NamingConventions
{
    /// <summary>
    /// Converts a kebab-case name to PascalCase.
    /// "health-text" -> "HealthText"
    /// "bar-fill--critical" -> "BarFillCritical"
    /// </summary>
    internal static string KebabToPascal(string kebab);

    /// <summary>
    /// Converts a kebab-case name to camelCase.
    /// "health-text" -> "healthText"
    /// </summary>
    internal static string KebabToCamel(string kebab);

    /// <summary>
    /// Converts a BEM class name to PascalCase, treating block, element,
    /// and modifier segments as separate words.
    /// "bar-fill" -> "BarFill"
    /// "bar-fill--critical" -> "BarFillCritical"
    /// "player__name" -> "PlayerName"
    /// "player__name--active" -> "PlayerNameActive"
    /// </summary>
    internal static string BemToPascal(string bemName);

    /// <summary>
    /// Converts a CSS custom property name to PascalCase, stripping the -- prefix.
    /// "--bar-color" -> "BarColor"
    /// "--primary-bg" -> "PrimaryBg"
    /// </summary>
    internal static string CssVarToPascal(string cssVarName);
}
```

### Diagnostic Descriptors

```csharp
namespace JEngine.JUI.Generators;

/// <summary>
/// All diagnostic descriptors emitted by JUI source generators.
/// IDs are prefixed with "JUI" and grouped by generator:
///   JUI001-JUI019: ElementGenerator
///   JUI020-JUI039: StyleGenerator
///   JUI040-JUI059: InjectGenerator
///   JUI060-JUI079: BindingGenerator
///   JUI080-JUI099: EffectGenerator
///   JUI100-JUI110: ValidateGenerator / cross-cutting
/// </summary>
internal static class DiagnosticDescriptors
{
    // ── ElementGenerator (JUI001-JUI019) ──────────────────────────────

    /// <summary>Element name attribute is empty string.</summary>
    internal static readonly DiagnosticDescriptor BindElementNameEmpty;       // JUI001, Warning

    /// <summary>Duplicate element name in UXML file.</summary>
    internal static readonly DiagnosticDescriptor DuplicateElementName;       // JUI002, Warning

    /// <summary>UXML file referenced in [UIComponent] not found.</summary>
    internal static readonly DiagnosticDescriptor UxmlFileNotFound;           // JUI003, Error

    /// <summary>UXML file contains malformed XML.</summary>
    internal static readonly DiagnosticDescriptor UxmlParseError;             // JUI004, Error

    /// <summary>Element type could not be resolved to a known UI Toolkit type.</summary>
    internal static readonly DiagnosticDescriptor UnknownElementType;         // JUI005, Warning

    /// <summary>Element name maps to a C# keyword after PascalCase conversion.</summary>
    internal static readonly DiagnosticDescriptor ElementNameIsKeyword;       // JUI006, Error

    /// <summary>Element name contains characters invalid for a C# identifier.</summary>
    internal static readonly DiagnosticDescriptor ElementNameInvalidChars;    // JUI007, Error

    /// <summary>UXML namespace prefix could not be resolved.</summary>
    internal static readonly DiagnosticDescriptor UnresolvedUxmlNamespace;    // JUI008, Warning

    // ── StyleGenerator (JUI020-JUI039) ────────────────────────────────

    /// <summary>USS file referenced in [UIComponent] not found.</summary>
    internal static readonly DiagnosticDescriptor UssFileNotFound;            // JUI020, Error

    /// <summary>USS file contains syntax errors.</summary>
    internal static readonly DiagnosticDescriptor UssParseError;              // JUI021, Error

    /// <summary>Duplicate class name in USS file.</summary>
    internal static readonly DiagnosticDescriptor DuplicateClassName;         // JUI022, Warning

    /// <summary>CSS variable name maps to a C# keyword after conversion.</summary>
    internal static readonly DiagnosticDescriptor CssVarNameIsKeyword;        // JUI023, Error

    /// <summary>CSS variable value type could not be inferred.</summary>
    internal static readonly DiagnosticDescriptor CssVarTypeUnknown;          // JUI024, Info

    /// <summary>Class name contains characters invalid for a C# identifier.</summary>
    internal static readonly DiagnosticDescriptor ClassNameInvalidChars;      // JUI025, Error

    // ── InjectGenerator (JUI040-JUI059) ───────────────────────────────

    /// <summary>[Inject] applied to a type not registered in any ancestor scope.</summary>
    internal static readonly DiagnosticDescriptor InjectTypeNotProvided;      // JUI040, Warning

    /// <summary>[Inject] applied to a non-reference type (value types not supported).</summary>
    internal static readonly DiagnosticDescriptor InjectValueType;            // JUI041, Error

    /// <summary>[Inject] field is not readonly, risking accidental reassignment.</summary>
    internal static readonly DiagnosticDescriptor InjectFieldNotReadonly;     // JUI042, Info

    /// <summary>[Inject] applied to a static member.</summary>
    internal static readonly DiagnosticDescriptor InjectOnStaticMember;       // JUI043, Error

    /// <summary>Class with [Inject] does not extend JUIComponent.</summary>
    internal static readonly DiagnosticDescriptor InjectNotOnComponent;       // JUI044, Error

    /// <summary>Duplicate [Inject] for the same type in one component.</summary>
    internal static readonly DiagnosticDescriptor InjectDuplicateType;        // JUI045, Warning

    // ── BindingGenerator (JUI060-JUI079) ──────────────────────────────

    /// <summary>[Bind] references an element name not found in El constants.</summary>
    internal static readonly DiagnosticDescriptor BindElementNotFound;        // JUI060, Error

    /// <summary>[Bind] converter type does not implement IValueConverter.</summary>
    internal static readonly DiagnosticDescriptor BindConverterInvalid;       // JUI061, Error

    /// <summary>[Bind] signal type and element property type are incompatible.</summary>
    internal static readonly DiagnosticDescriptor BindTypeMismatch;           // JUI062, Error

    /// <summary>[BindSync] applied to a read-only signal (Sync requires read-write).</summary>
    internal static readonly DiagnosticDescriptor BindSyncOnReadOnly;         // JUI063, Error

    /// <summary>[Bind] or [BindSync] applied to a non-signal field.</summary>
    internal static readonly DiagnosticDescriptor BindNotASignal;             // JUI064, Error

    /// <summary>[Bind] mode is Pull but no callback method was specified.</summary>
    internal static readonly DiagnosticDescriptor BindPullNoCallback;         // JUI065, Error

    /// <summary>[BindSync] applied to a type that does not support two-way binding.</summary>
    internal static readonly DiagnosticDescriptor BindSyncUnsupportedType;    // JUI066, Warning

    // ── EffectGenerator (JUI080-JUI099) ───────────────────────────────

    /// <summary>[Effect] Watch array references a member that is not a signal.</summary>
    internal static readonly DiagnosticDescriptor EffectWatchNotSignal;       // JUI080, Error

    /// <summary>[Effect] When references a member that is not a boolean signal.</summary>
    internal static readonly DiagnosticDescriptor EffectWhenNotBoolSignal;    // JUI081, Error

    /// <summary>[Effect] method has parameters (must be parameterless).</summary>
    internal static readonly DiagnosticDescriptor EffectMethodHasParams;      // JUI082, Error

    /// <summary>[Effect] Watch array is empty.</summary>
    internal static readonly DiagnosticDescriptor EffectWatchEmpty;           // JUI083, Warning

    /// <summary>[Effect] applied to a static method.</summary>
    internal static readonly DiagnosticDescriptor EffectOnStaticMethod;       // JUI084, Error

    /// <summary>[Effect] class does not extend JUIComponent.</summary>
    internal static readonly DiagnosticDescriptor EffectNotOnComponent;       // JUI085, Error

    /// <summary>[Effect] When and WhenAll/WhenAny are mutually exclusive.</summary>
    internal static readonly DiagnosticDescriptor EffectConflictingCondition; // JUI086, Error

    /// <summary>[Effect] WhenAll/WhenAny array contains non-boolean signal.</summary>
    internal static readonly DiagnosticDescriptor EffectConditionNotBool;     // JUI087, Error

    // ── Cross-Cutting / Validate (JUI100-JUI110) ─────────────────────

    /// <summary>[UIComponent] attribute is missing UXML path argument.</summary>
    internal static readonly DiagnosticDescriptor UIComponentMissingUxml;     // JUI100, Error

    /// <summary>Class with generator attributes is not marked partial.</summary>
    internal static readonly DiagnosticDescriptor ClassNotPartial;            // JUI101, Error

    /// <summary>Class with generator attributes does not inherit JUIComponent.</summary>
    internal static readonly DiagnosticDescriptor ClassNotComponent;          // JUI102, Error

    /// <summary>Generated code would conflict with an existing member name.</summary>
    internal static readonly DiagnosticDescriptor MemberNameConflict;         // JUI103, Warning

    /// <summary>Multiple [UIComponent] attributes on the same class.</summary>
    internal static readonly DiagnosticDescriptor DuplicateUIComponent;       // JUI104, Error

    /// <summary>Additional file (UXML/USS) could not be loaded by the generator.</summary>
    internal static readonly DiagnosticDescriptor AdditionalFileLoadError;    // JUI105, Error

    /// <summary>Source generator version mismatch with runtime assembly.</summary>
    internal static readonly DiagnosticDescriptor VersionMismatch;            // JUI106, Warning

    /// <summary>Generated partial method has no corresponding user implementation.</summary>
    internal static readonly DiagnosticDescriptor PartialMethodNotImpl;       // JUI107, Info

    /// <summary>UXML path uses backslashes (not portable).</summary>
    internal static readonly DiagnosticDescriptor PathNotPortable;            // JUI108, Warning

    /// <summary>Attribute argument is null or whitespace.</summary>
    internal static readonly DiagnosticDescriptor AttributeArgNullOrEmpty;    // JUI109, Error

    /// <summary>Validation rule is incompatible with the target member type.</summary>
    internal static readonly DiagnosticDescriptor ValidateRuleIncompatible;   // JUI110, Error
}
```

## Data Structures

- `UxmlElement` -- immutable struct holding `Name`, `TypeName`, `TypeNamespace` for each named element extracted from a UXML file.
- `UssClass` -- immutable struct holding `RawName` for each CSS class selector.
- `UssVariable` -- immutable struct holding `RawName` and `UssValueType` for each CSS custom property.
- `UssValueType` -- enum: `String`, `Color`, `Length`, `Float`.
- All diagnostic descriptors are `static readonly DiagnosticDescriptor` fields with unique IDs in the `JUI` prefix range.

## Implementation Notes

- **IIncrementalGenerator, not ISourceGenerator**: All generators implement `IIncrementalGenerator` and register syntax/semantic providers via `IncrementalGeneratorInitializationContext.RegisterSourceOutput`. This avoids full-project re-analysis on each keystroke.
- **Additional files pipeline**: UXML and USS files are registered as additional files in the Unity project (via `.asmdef` or `.csproj` configuration). The generators access them through `context.AdditionalTextsProvider` filtered by extension.
- **Caching via `IncrementalValueProvider`**: Parsed UXML/USS results are cached using `Select` and `Collect` on the incremental pipeline. Re-parsing only occurs when the file content changes (content-based equality).
- **UXML parsing strategy**: Uses `System.Xml.Linq.XDocument` (available in netstandard2.0) to parse UXML. The parser extracts `name` attributes and maps element tag names to UI Toolkit types using a built-in type map (e.g., `ui:Label` -> `UnityEngine.UIElements.Label`).
- **USS parsing strategy**: Uses a simple regex-based lexer (not a full CSS parser). It extracts `.class-name` selectors from rule heads and `--variable-name: value` declarations from rule bodies. The value type is inferred from the value text: `#hex` or `rgb(...)` -> Color, `Npx` or `N%` -> Length, bare number -> Float, else String.
- **Naming collision avoidance**: After kebab-to-PascalCase conversion, generated identifiers are checked against C# reserved keywords. If a collision is found, a `@` prefix is added or JUI006/JUI023 is reported depending on severity.
- **Diagnostic severity levels**: Errors prevent code generation for the affected class. Warnings and Info diagnostics are reported but do not block generation.

## Source Generator Notes

This section IS the source generator setup. Key architectural decisions:

1. **Single assembly, multiple generators**: All generators live in one assembly (`JEngine.JUI.Generators.dll`) to share parsers, diagnostics, and utilities. Each generator is a separate class implementing `IIncrementalGenerator` with its own `[Generator]` attribute.

2. **Incremental pipeline structure**: Each generator follows this pattern:
   ```
   AdditionalTexts (UXML/USS) ──┐
                                 ├── Combine ── Transform ── RegisterSourceOutput
   SyntaxProvider (attributes) ──┘
   ```

3. **Unity integration**: The compiled DLL is placed in `Packages/com.jasonxudeveloper.jengine.ui/Plugins/JEngine.JUI.Generators.dll` with a `.meta` file that sets:
   - `includeAllPlatforms: false`
   - `includeRoslynAnalyzer: true`
   - All platform checkboxes unchecked (analyzer-only, not runtime)

4. **Build pipeline**:
   ```
   dotnet build SourceGenerators/JEngine.JUI.Generators/
       → bin/Release/netstandard2.0/JEngine.JUI.Generators.dll
       → post-build copies to Packages/.../Plugins/
   ```

5. **Test infrastructure**: Generator tests use `Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver` to run generators against in-memory compilations:
   ```csharp
   var generator = new ElementGenerator();
   GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
   driver = driver.RunGeneratorsAndUpdateCompilation(compilation,
       out var outputCompilation, out var diagnostics);
   ```

## Usage Examples

```csharp
// The developer writes:
[UIComponent("PlayerHUD.uxml", "PlayerHUD.uss")]
public partial class PlayerHUD : JUIComponent
{
    // ... component code
}

// The generator pipeline:
// 1. AdditionalTextsProvider finds PlayerHUD.uxml and PlayerHUD.uss
// 2. SyntaxProvider finds PlayerHUD class with [UIComponent] attribute
// 3. UxmlParser.Parse() extracts named elements from the UXML
// 4. UssParser.ParseClasses() extracts class selectors from the USS
// 5. UssParser.ParseVariables() extracts CSS custom properties
// 6. ElementGenerator emits El class and typed fields (Section 10)
// 7. StyleGenerator emits Cls class and JUIVar class (Section 10)
// 8. InjectGenerator emits InitScope() (Section 11)
// 9. BindingGenerator emits InitBindings() (Section 11)
// 10. EffectGenerator emits InitEffects() (Section 12)
```

```xml
<!-- Example .asmdef additional files configuration -->
<!-- Unity registers .uxml and .uss as additional files automatically -->
<!-- when the generator DLL is referenced as a Roslyn analyzer -->
```

```csharp
// Generator test example
[Test]
public void ElementGenerator_EmitsElClass_ForNamedElements()
{
    var uxmlContent = @"
        <ui:UXML xmlns:ui=""UnityEngine.UIElements"">
            <ui:Label name=""health-text"" />
            <ui:VisualElement name=""health-fill"" />
        </ui:UXML>";

    var source = @"
        using JEngine.JUI;
        [UIComponent(""test.uxml"")]
        public partial class TestComponent : JUIComponent { }";

    var compilation = CreateCompilation(source);
    var additionalTexts = new[] { new InMemoryAdditionalText("test.uxml", uxmlContent) };

    GeneratorDriver driver = CSharpGeneratorDriver.Create(new ElementGenerator())
        .AddAdditionalTexts(ImmutableArray.CreateRange(additionalTexts));

    driver.RunGeneratorsAndUpdateCompilation(compilation,
        out var outputCompilation, out var diagnostics);

    var generatedSource = outputCompilation.SyntaxTrees
        .First(t => t.FilePath.Contains("TestComponent.El.g.cs"));

    var text = generatedSource.GetText().ToString();
    Assert.That(text, Does.Contain("public const string HealthText = \"health-text\""));
    Assert.That(text, Does.Contain("public const string HealthFill = \"health-fill\""));
}
```

## Test Plan

1. **UXML parser extracts named elements**: Parse a UXML file with named and unnamed elements, verify only named elements are returned.
2. **UXML parser resolves element types**: Parse `<ui:Label>`, `<ui:Button>`, `<ui:VisualElement>`, verify correct `TypeName` for each.
3. **UXML parser handles malformed XML**: Pass invalid XML, verify `UxmlParseError` diagnostic is emitted and no crash.
4. **USS parser extracts class selectors**: Parse a USS file with `.foo`, `.bar-baz`, `.bar-baz--modifier`, verify all are returned.
5. **USS parser extracts CSS variables**: Parse `--my-color: #ff0000; --my-size: 16px;`, verify types inferred as Color and Length.
6. **USS parser handles syntax errors**: Pass malformed USS, verify `UssParseError` diagnostic and graceful degradation.
7. **NamingConventions kebab-to-PascalCase**: Verify `health-text` -> `HealthText`, `a-b-c` -> `ABC`.
8. **NamingConventions BEM-to-PascalCase**: Verify `bar-fill--critical` -> `BarFillCritical`, `player__name` -> `PlayerName`.
9. **NamingConventions CSS var**: Verify `--bar-color` -> `BarColor`.
10. **NamingConventions keyword collision**: Verify `class` is detected as a C# keyword and appropriately handled.
11. **Diagnostic IDs are unique**: Verify no duplicate IDs across all diagnostic descriptors.
12. **Incremental caching**: Run generator twice with unchanged file, verify the generator does not re-parse (cache hit).
13. **Incremental invalidation**: Run generator, modify UXML content, run again, verify re-parse occurs and output changes.
14. **DLL loads in Unity as analyzer**: Verify the `.meta` file marks the DLL as a Roslyn analyzer, not a runtime assembly.

## Acceptance Criteria

- [ ] Project targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 4.3+
- [ ] All generators implement `IIncrementalGenerator`, not `ISourceGenerator`
- [ ] `UxmlParser` extracts named elements with types from valid UXML files
- [ ] `UssParser` extracts class selectors and CSS custom properties with inferred types
- [ ] `NamingConventions` correctly converts kebab-case, BEM, and CSS variable names to PascalCase/camelCase
- [ ] All diagnostic descriptors have unique IDs in the `JUI` prefix range (JUI001-JUI110)
- [ ] Diagnostic severity levels are appropriate: Error blocks generation, Warning/Info do not
- [ ] Generator DLL builds and copies to `Plugins/` folder with correct `.meta` configuration
- [ ] Generator tests use `CSharpGeneratorDriver` for in-memory compilation testing
- [ ] Incremental pipeline caches parsed results and only re-processes changed files
- [ ] C# keyword collisions are detected and reported via diagnostics
- [ ] All internal APIs have XML documentation
