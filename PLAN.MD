# 📋 CSharpEssentials Multi-Targeting Refactoring Plan

## 🎯 Objective
Refactor all **CSharpEssentials** packages to support **.NET Standard 2.1** (for broad compatibility) while preserving **.NET 9** (and future .NET 10) optimizations and features using **Multi-Targeting** and **Conditional Compilation**.

---

## 🛠️ Technical Strategy

### 1. Target Frameworks Configuration
Update all `.csproj` files to support multiple targets.

```xml
<PropertyGroup>
    <TargetFrameworks>net9.0;netstandard2.1</TargetFrameworks>
    <!-- Optional: Enable latest C# features for older targets where possible (Polyfills may be needed) -->
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

### 2. Conditional Compilation Symbols
Use preprocessor directives to separate code logic:

- `NET9_0_OR_GREATER`: For code specific to .NET 9, 10, etc.
- `NETSTANDARD2_1`: For code specific to .NET Standard 2.1.

### 3. Handling Missing C# Features in .NET Standard 2.1
.NET Standard 2.1 officially supports C# 8.0. Some newer features (C# 9-13) require "Polyfills" or manual refactoring.

#### A. Primary Constructors (C# 12)
**Problem:** Not available in C# 8.0 (standard for ns2.1).
**Solution:** Convert to explicit constructors or use `#if` (Refactoring to explicit is cleaner for shared code).

```csharp
// ❌ .NET 9 Style
public class User(string name) 
{ 
    public string Name { get; } = name; 
}

// ✅ Compatible Style (Refactor to this)
public class User
{
    public User(string name) 
    {
        Name = name;
    }
    public string Name { get; }
}
```

#### B. Collection Expressions (C# 12)
**Problem:** `[]` syntax not available in older versions.
**Solution:** Use `new List<T>()` or `Array.Empty<T>()`.

```csharp
// ❌ .NET 9
List<string> list = ["a", "b"];

// ✅ Compatible
List<string> list = new List<string> { "a", "b" };
```

#### C. File-Scoped Namespaces (C# 10)
**Status:** Can be used in .NET Standard 2.1 if `<LangVersion>latest</LangVersion>` is set. **Recommendation:** Keep them if you set LangVersion, otherwise convert to Block Scoped.

#### D. Records & Init Only Setters (C# 9)
**Status:** Requires `IsExternalInit` polyfill for .NET Standard 2.1.
**Plan:** Add a `Polyfills.cs` file in `CSharpEssentials.Core` (internal) to enable this across the library.

```csharp
// Polyfills.cs
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
```

#### E. Argument Validation
**Problem:** `ArgumentNullException.ThrowIfNull` is .NET 6+.
**Solution:** Use conditional compilation or extension method backport.

```csharp
#if NET6_0_OR_GREATER
    ArgumentNullException.ThrowIfNull(value);
#else
    if (value is null) throw new ArgumentNullException(nameof(value));
#endif
```

---

## 🗓️ Step-by-Step Execution Plan

### Phase 1: Infrastructure & Core (The Foundation)

**Goal:** Enable multi-targeting in the base package and set up polyfills.

1.  **`CSharpEssentials.Core`**
    -   [ ] Update `.csproj` to `<TargetFrameworks>net9.0;netstandard2.1</TargetFrameworks>`.
    -   [ ] Add `Polyfills.cs` (internal) to support `init` properties and `records` in ns2.1.
    -   [ ] Refactor `Extensions` methods.
        -   Replace `ArgumentNullException.ThrowIfNull`.
        -   Replace Collection Expressions `[]`.
    -   [ ] **Result:** Core compiles for both targets.

2.  **`CSharpEssentials.Time`**
    -   [ ] Update `.csproj`.
    -   [ ] `TimeProvider` is .NET 8+. For ns2.1, add dependency: `<PackageReference Include="Microsoft.Bcl.TimeProvider" Version="..." Condition="'$(TargetFramework)' == 'netstandard2.1'" />`.
    -   [ ] Refactor `DateTimeProvider` to use BCL shim for ns2.1.

### Phase 2: Functional Types (Heavy Record Usage)

**Goal:** Ensure `Result`, `Maybe`, and `Error` work seamlessly.

3.  **`CSharpEssentials.Errors`**
    -   [ ] Update `.csproj`.
    -   [ ] Check `Error` record struct usage. Ensure `Polyfills.cs` is present or linked.
    -   [ ] Refactor Collection expressions in `Error.cs`.

4.  **`CSharpEssentials.Results`**
    -   [ ] Update `.csproj`.
    -   [ ] The `Result<T>` struct might use `System.Text.Json` features.
    -   [ ] Ensure `JsonSerializerOptions` compatibility (some options might be newer).

5.  **`CSharpEssentials.Maybe`** & **`CSharpEssentials.Any`**
    -   [ ] Update `.csproj`.
    -   [ ] Refactor pattern matching if using C# 9+ relational patterns that compiler can't lower efficiently (usually fine with `latest`).

### Phase 3: Domain & Logic

6.  **`CSharpEssentials.Rules`**
    -   [ ] Update `.csproj`.
    -   [ ] Check for `required` keyword usage (C# 11). Needs `RequiredMemberAttribute` polyfill or removal.

7.  **`CSharpEssentials.Entity`**
    -   [ ] Update `.csproj`.
    -   [ ] Check `EntityBase` for primary constructors. Refactor to standard constructors.

### Phase 4: Infrastructure & Serialization

8.  **`CSharpEssentials.Json`**
    -   [ ] Update `.csproj`.
    -   [ ] **Challenge:** `System.Text.Json` in .NET 9 has new APIs.
    -   [ ] **Strategy:**
        -   Add `<PackageReference Include="System.Text.Json" Version="9.0.0" />` for `netstandard2.1` target to bring it close to .NET 9 capabilities.
        -   Use `#if NET9_0_OR_GREATER` for any APIs that strictly don't exist in the NuGet package version.

9.  **`CSharpEssentials.GcpSecretManager`**
    -   [ ] Update `.csproj`.
    -   [ ] Check `Google.Cloud.SecretManager.V1` compatibility with ns2.1 (should be fine).

### Phase 5: Framework Integration (The Hardest Part)

10. **`CSharpEssentials.AspNetCore`**
    -   [ ] Update `.csproj`.
    -   [ ] **Challenge:** `Microsoft.AspNetCore.App` is not available in `netstandard2.1`.
    -   [ ] **Strategy:**
        -   Target `net9.0` AND `netcoreapp3.1` (or keep it `net9.0` only if you don't need legacy support).
        -   *Alternatively*, target `netstandard2.1` and add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` is NOT possible.
        -   **Decision:** `AspNetCore` package likely needs to target `net8.0`/`net9.0` directly, or include `Microsoft.AspNetCore.*` NuGet packages manually for `netstandard2.1` (Not recommended).
        -   **Revised Plan:** Target `net6.0` (LTS) and `net9.0`. .NET Standard 2.1 is usually not appropriate for *middleware* libraries that depend on `HttpContext`.

11. **`CSharpEssentials.EntityFrameworkCore`**
    -   [ ] Update `.csproj`.
    -   [ ] Target `net9.0` and `netstandard2.1`.
    -   [ ] Add dependency `<PackageReference Include="Microsoft.EntityFrameworkCore" Version="[Version]" />`.
        -   For `net9.0` -> Version 9.x
        -   For `netstandard2.1` -> Version 8.x or lower?
        -   *Note:* EF Core 9 targets .NET 8. EF Core 8 targets .NET 8. EF Core 3.1 targets ns2.0.
        -   **Decision:** You might need to stick to `net8.0` / `net9.0` for this package if you want modern EF Core features.

### Phase 6: Final Polish

12. **`CSharpEssentials` (Meta Package)**
    -   [ ] Update `.csproj` to include the specific TFMs of all dependencies.

---

## 🧩 Common Refactoring Patterns Reference

### 1. Argument Null Check
```csharp
public void Method(string input)
{
#if NET6_0_OR_GREATER
    ArgumentNullException.ThrowIfNull(input);
#else
    if (input is null) throw new ArgumentNullException(nameof(input));
#endif
}
```

### 2. Collection Initialization
```csharp
// Shared Code (safest approach)
public IEnumerable<Error> Errors { get; } = new List<Error>();

// OR Conditional
#if NET8_0_OR_GREATER
    public int[] Numbers => [1, 2, 3];
#else
    public int[] Numbers => new[] { 1, 2, 3 };
#endif
```

### 3. Json Serialization
```csharp
// Some options might differ
var options = new JsonSerializerOptions();
#if NET9_0_OR_GREATER
    options.RespectNullableAnnotations = true;
#endif
```

## 📝 Checklist for Each PR

- [ ] `.csproj` updated with `<TargetFrameworks>`.
- [ ] Code compiles under `net9.0`.
- [ ] Code compiles under `netstandard2.1`.
- [ ] `dotnet test` passes for both targets.
- [ ] `README.md` updated if requirements changed.
