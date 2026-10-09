# CSharpEssentials.Validation: Performance Benchmarks

**vs. FluentValidation 11.x · BenchmarkDotNet v0.14.0 · Apple M3 Pro · .NET 9 / 10 / 11**

---

## What Was Measured

23 head-to-head scenarios covering the main validation API surface:
validator construction, simple rules, string rules, comparable (range) rules, collection rules,
nested validators, cascade modes, conditional logic, regex, nullable types, async predicates,
wide models (12 fields), deep nesting (3 levels), and large collections (50 items).

Each scenario has a matching `CseXxxValidator` and `FvXxxValidator` built from the same
validation requirements, exercising equivalent logic on identical input data. The numbers come from the
reports committed under `benchmarks/results/{tfm}/results/` (the `default` job).

**Environment**

| Property | Value |
|---|---|
| Machine | Apple M3 Pro, 12 cores (arm64) |
| OS | macOS 26.6.2 (net9 run), macOS 26.3.1 (net10 and net11 runs) |
| Tool | BenchmarkDotNet v0.14.0, [MemoryDiagnoser] |
| .NET SDK | 11.0.100-preview.3.26207.106 |
| Runtimes tested | .NET 9.0.4, .NET 10.0.7, .NET 11.0.0 |
| FluentValidation | 11.11.0 |
| CSharpEssentials.Validation | The repository source, referenced as a project |

> Note: the benchmark project also targets net8.0, but no net8.0 report is included. Results for net9/10/11 are included.

---

## Executive Summary

| Finding | Details |
|---|---|
| **Construction is 650-810× faster** | `new()` costs 2.467 ns / 24 B for CSE and 1,995.479 ns / 9.6 KB for FV, which builds its rule tree in the constructor (809× on net9, 652× on net11) |
| **Invalid input is 6-8× faster** | In the model-level scenarios (Simple, Cascade, MultiRegex, LengthRange, Complex, Wide, Nullable) CSE is 6.1-8.3× faster on net9 and allocates 3.0-5.6× less. The single-rule `Must` invalid case is 3.1× faster. FV builds a `ValidationFailure` object per failed rule |
| **Faster in every scenario** | On valid or mixed input CSE is 1.4-3.3× faster across the three runtimes; the async predicate is the smallest gap (1.1×, equal on net10). CSE also allocates less in every scenario except Wide-Valid, where FV allocates 984 B against 1,816 B |
| **Collections: about 3× faster** | 50-item collection: 2.9× faster and 25% less allocation on net9 (34.83 KB vs 46.63 KB); `ForEach` over a small list: 3.1× faster |
| **Runtime improvements** | net10 and net11 lower the CSE allocation in most scenarios (for example LargeCollection 34.83 KB to 25.47 KB, Complex-Valid 2.86 KB to 2.06 KB) |

---

## Results by Scenario

Each table lists the mean on net9, net10 and net11, and the allocation on net9 and net10 (net11 allocation equals net10 in every
scenario except the constructors and the async predicate). The advantage rows divide the FV figure by the CSE figure.

### 1. Validator Construction

> How fast is `new MyValidator()`? This matters in DI-free or transient scenarios.

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Ctor | 2.467 ns | 2.186 ns | 2.473 ns | 24 B | 24 B |
| FV-Ctor | 1,995.479 ns | 1,652.300 ns | 1,612.950 ns | 9,624 B | 9,272 B |
| **CSE advantage** | **809×** | **756×** | **652×** | 401× less | 386× less |

A CSE validator holds no rule tree: its rules are written inside `Configure`, which runs on every `ValidateAsync` call, so `new` allocates only the 24 B validator object. FV builds its rule tree in the constructor, so each `new` costs about 2 μs and 9.6 KB. In DI a validator is usually built once per scope or lifetime, so this matters mainly for transient or per-request validators; the per-call cost is in the scenarios below.

---

### 2. Simple Validation (4 properties on `User`: strings, `int`, list)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Simple-Valid | 206.8 ns | 176.1 ns | 184.5 ns | 656 B | 608 B |
| FV-Simple-Valid | 371.4 ns | 321.7 ns | 313.9 ns | 704 B | 704 B |
| CSE-Simple-Invalid | 302.3 ns | 257.1 ns | 260.1 ns | 1,528 B | 1,544 B |
| FV-Simple-Invalid | 2,498.2 ns | 2,108.1 ns | 2,113.1 ns | 7,584 B | 7,264 B |
| **CSE advantage (Simple-Valid)** | **1.8×** | **1.8×** | **1.7×** | 7% less | 14% less |
| **CSE advantage (Simple-Invalid)** | **8.3×** | **8.2×** | **8.1×** | 5.0× less | 4.7× less |

**Valid path:** CSE is 1.8× faster and allocates 48 B less (656 B vs 704 B).  
**Invalid path:** CSE is **8.3× faster**, allocates **5.0× less**.

---

### 3. String Validation (NotEmpty + MinLength + MaxLength + Matches, NotEmpty + EmailAddress + Must)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-String | 165.3 ns | 151.8 ns | 159.5 ns | 456 B | 392 B |
| FV-String | 300.2 ns | 265.4 ns | 259.1 ns | 664 B | 664 B |
| **CSE advantage** | **1.8×** | **1.7×** | **1.6×** | 31% less | 41% less |

---

### 4. String Length Range (`Length(min, max)` on two fields)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-LengthRange-Valid | 83.71 ns | 64.46 ns | 67.12 ns | 456 B | 328 B |
| FV-LengthRange-Valid | 154.75 ns | 123.98 ns | 115.55 ns | 600 B | 600 B |
| CSE-LengthRange-Invalid | 146.40 ns | 139.86 ns | 145.09 ns | 912 B | 848 B |
| FV-LengthRange-Invalid | 973.32 ns | 814.11 ns | 800.15 ns | 3,872 B | 3,808 B |
| **CSE advantage (LengthRange-Valid)** | **1.8×** | **1.9×** | **1.7×** | 24% less | 45% less |
| **CSE advantage (LengthRange-Invalid)** | **6.6×** | **5.8×** | **5.5×** | 4.2× less | 4.5× less |

**Invalid path:** CSE is **6.6×** faster, **4.2×** less memory.

---

### 5. String Content (Contains / StartsWith / EndsWith)

> CSE has native built-in methods for `Contains` and `EndsWith`. The FV validator uses `Must(x => x.Contains(...))`-style lambdas for the same checks.

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-StringContent | 62.47 ns | 50.00 ns | 52.31 ns | 312 B | 248 B |
| FV-StringContent | 146.63 ns | 124.53 ns | 117.80 ns | 600 B | 600 B |
| **CSE advantage** | **2.3×** | **2.5×** | **2.3×** | 48% less | 2.4× less |

---

### 6. String Equality (Equal / NotEqual)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Equality | 90.28 ns | 73.00 ns | 77.45 ns | 464 B | 400 B |
| FV-Equality | 193.32 ns | 158.69 ns | 149.01 ns | 632 B | 632 B |
| **CSE advantage** | **2.1×** | **2.2×** | **1.9×** | 27% less | 37% less |

---

### 7. Comparable Validation (GreaterThan / GreaterThanOrEqualTo / LessThan / LessThanOrEqualTo / InclusiveBetween)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Comparable | 51.51 ns | 44.68 ns | 46.86 ns | 304 B | 240 B |
| FV-Comparable | 170.57 ns | 147.88 ns | 138.21 ns | 600 B | 600 B |
| **CSE advantage** | **3.3×** | **3.3×** | **2.9×** | 49% less | 2.5× less |

---

### 8. Exclusive Between

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-ExclusiveBetween | 47.22 ns | 39.93 ns | 41.80 ns | 304 B | 240 B |
| FV-ExclusiveBetween | 118.66 ns | 93.64 ns | 86.60 ns | 600 B | 600 B |
| **CSE advantage** | **2.5×** | **2.3×** | **2.1×** | 49% less | 2.5× less |

---

### 9. Collection Validation (NotEmpty / MinCount / MaxCount / CountBetween on a `List`)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Collection | 56.23 ns | 48.48 ns | 49.22 ns | 312 B | 248 B |
| FV-Collection | 169.45 ns | 140.10 ns | 134.40 ns | 640 B | 640 B |
| **CSE advantage** | **3.0×** | **2.9×** | **2.7×** | 2.1× less | 2.6× less |

---

### 10. Custom Predicate (`Must`)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Must-Valid | 49.09 ns | 36.93 ns | 39.35 ns | 312 B | 248 B |
| FV-Must-Valid | 115.63 ns | 94.43 ns | 86.95 ns | 600 B | 600 B |
| CSE-Must-Invalid | 73.27 ns | 57.25 ns | 60.52 ns | 504 B | 440 B |
| FV-Must-Invalid | 228.93 ns | 180.09 ns | 180.35 ns | 1,008 B | 1,008 B |
| **CSE advantage (Must-Valid)** | **2.4×** | **2.6×** | **2.2×** | 48% less | 2.4× less |
| **CSE advantage (Must-Invalid)** | **3.1×** | **3.1×** | **3.0×** | 2.0× less | 2.3× less |

---

### 11. Async Predicate (`MustAsync`)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-MustAsync | 753.3 ns | 1.092 μs | 934.0 ns | 912 B | 848 B |
| FV-MustAsync | 851.5 ns | 1.142 μs | 1,050.1 ns | 1,472 B | 1,472 B |
| **CSE advantage** | **1.1×** | **~equal** | **1.1×** | 38% less | 42% less |

> Async overhead narrows the time gap: on net10 the two are about equal. The allocation advantage persists on every runtime.

---

### 12. Cascade Stop (stop on first failure)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Cascade-Stop | 110.4 ns | 112.3 ns | 113.6 ns | 704 B | 720 B |
| FV-Cascade-Stop | 818.8 ns | 691.9 ns | 696.3 ns | 3,424 B | 3,360 B |
| **CSE advantage** | **7.4×** | **6.2×** | **6.1×** | 4.9× less | 4.7× less |

---

### 13. Cascade Continue (all rules run, multiple failures)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Cascade-Continue | 209.7 ns | 200.0 ns | 195.0 ns | 1.06 KB | 1.08 KB |
| FV-Cascade-Continue | 1,692.2 ns | 1,443.8 ns | 1,481.1 ns | 5.91 KB | 5.72 KB |
| **CSE advantage** | **8.1×** | **7.2×** | **7.6×** | 5.6× less | 5.3× less |

This is the most impactful scenario for real applications. When a form has several invalid fields,
CSE collects all errors into a plain list inside a `Result<T>`. FV allocates a `ValidationFailure`
per failed rule, each carrying the interpolated message, severity, and metadata, inside its
rule pipeline. (Neither side throws: FV's `Validate()` returns a `ValidationResult`; the gap is
purely per-failure object allocation and pipeline overhead, not exception handling.)

---

### 14. Conditional Validation (`if` on `IsBusiness` vs FV `When`)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Conditional-Business | 166.2 ns | 143.3 ns | 149.8 ns | 648 B | 520 B |
| FV-Conditional-Business | 261.2 ns | 219.8 ns | 211.3 ns | 696 B | 696 B |
| CSE-Conditional-Personal | 131.7 ns | 113.5 ns | 119.1 ns | 488 B | 360 B |
| FV-Conditional-Personal | 232.3 ns | 195.8 ns | 187.6 ns | 664 B | 664 B |
| **CSE advantage (Conditional-Business)** | **1.6×** | **1.5×** | **1.4×** | 7% less | 25% less |
| **CSE advantage (Conditional-Personal)** | **1.8×** | **1.7×** | **1.6×** | 27% less | 46% less |

CSE is 1.6× and 1.8× faster on net9 (business and personal branch) and allocates less on both.

---

### 15. Nested Validation (1 level: Order → Address)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Nested | 177.8 ns | 141.9 ns | 140.9 ns | 712 B | 520 B |
| FV-Nested | 440.2 ns | 380.3 ns | 375.9 ns | 1,272 B | 1,272 B |
| **CSE advantage** | **2.5×** | **2.7×** | **2.7×** | 44% less | 2.4× less |

---

### 16. Deep Nested Validation (3 levels: Order → Address → Street)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-DeepNested | 304.6 ns | 244.6 ns | 247.1 ns | 1.23 KB | 872 B |
| FV-DeepNested | 853.9 ns | 716.7 ns | 703.5 ns | 2.04 KB | 2,088 B |
| **CSE advantage** | **2.8×** | **2.9×** | **2.8×** | 40% less | 2.4× less |

---

### 17. Collection Item Validation (ForEach / RuleForEach)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-CollectionItem | 345.7 ns | 286.5 ns | 283.4 ns | 1.66 KB | 1.22 KB |
| FV-CollectionItem | 1,065.6 ns | 890.4 ns | 871.3 ns | 2.81 KB | 2.81 KB |
| **CSE advantage** | **3.1×** | **3.1×** | **3.1×** | 41% less | 2.3× less |

---

### 18. Large Collection (50 items, each with 3 rules)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-LargeCollection | 7.618 μs | 5.811 μs | 5.703 μs | 34.83 KB | 25.47 KB |
| FV-LargeCollection | 21.944 μs | 18.024 μs | 17.690 μs | 46.63 KB | 46.63 KB |
| **CSE advantage** | **2.9×** | **3.1×** | **3.1×** | 25% less | 45% less |

CSE wins on both time and memory on every tested runtime. On net10 and net11 its allocation drops from 34.83 KB to 25.47 KB and the time gap widens from 2.9× to 3.1×.

---

### 19. Complex Validation (full Order with nested Address + items + conditionals)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Complex-Valid | 827.8 ns | 668.6 ns | 640.9 ns | 2.86 KB | 2.06 KB |
| FV-Complex-Valid | 2,193.4 ns | 1,871.0 ns | 1,859.6 ns | 3.6 KB | 3.6 KB |
| CSE-Complex-Invalid | 1,343.4 ns | 1,114.6 ns | 1,136.4 ns | 7.13 KB | 6.63 KB |
| FV-Complex-Invalid | 8,763.4 ns | 7,434.1 ns | 7,622.4 ns | 23.2 KB | 22.32 KB |
| **CSE advantage (Complex-Valid)** | **2.6×** | **2.8×** | **2.9×** | 21% less | 43% less |
| **CSE advantage (Complex-Invalid)** | **6.5×** | **6.7×** | **6.7×** | 3.3× less | 3.4× less |

**Valid path:** CSE is 2.6× faster and allocates 21% less.  
**Invalid path:** CSE is 6.5× faster and allocates 3.3× less.

---

### 20. Wide Model (12 string fields, `NotEmpty` + `MaxLength` on each)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Wide-Valid | 450.5 ns | 388.1 ns | 393.2 ns | 1,816 B | 1,704 B |
| FV-Wide-Valid | 839.0 ns | 694.4 ns | 686.9 ns | 984 B | 984 B |
| CSE-Wide-Invalid | 730.8 ns | 635.5 ns | 653.0 ns | 4,664 B | 4,616 B |
| FV-Wide-Invalid | 5,160.1 ns | 4,449.0 ns | 4,416.9 ns | 14,720 B | 13,952 B |
| **CSE advantage (Wide-Valid)** | **1.9×** | **1.8×** | **1.7×** | FV 1.8× less | FV 1.7× less |
| **CSE advantage (Wide-Invalid)** | **7.1×** | **7.0×** | **6.8×** | 3.2× less | 3.0× less |

**Valid path:** CSE is 1.9× faster on time, but FV allocates less memory here (984 B vs 1,816 B). Every `rules.For(() => model.Fx)` call creates a `RuleChain<T, TProp>` and a closure delegate, which adds up over 12 fields, while FV builds its rule tree once, in the validator constructor. `rules.For(model.F1, nameof(model.F1))` skips the delegate.  
**Invalid path:** CSE is 7.1× faster and allocates 3.2× less; FV's per-failure allocation is amplified across all 12 fields.

---

### 21. Multiple Regex (`NotEmpty` + 4 `Matches` rules on the same field, Cascade Continue)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-MultiRegex-Valid | 163.8 ns | 143.5 ns | 141.5 ns | 320 B | 256 B |
| FV-MultiRegex-Valid | 377.8 ns | 336.2 ns | 335.6 ns | 632 B | 632 B |
| CSE-MultiRegex-Invalid | 238.6 ns | 203.9 ns | 209.7 ns | 1,104 B | 1,040 B |
| FV-MultiRegex-Invalid | 1,462.4 ns | 1,298.5 ns | 1,286.8 ns | 4,872 B | 4,680 B |
| **CSE advantage (MultiRegex-Valid)** | **2.3×** | **2.3×** | **2.4×** | 49% less | 2.5× less |
| **CSE advantage (MultiRegex-Invalid)** | **6.1×** | **6.4×** | **6.1×** | 4.4× less | 4.5× less |

---

### 22. Precompiled Regex (static `Regex` instances with `RegexOptions.Compiled`)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Regex-Precompiled | 141.8 ns | 124.9 ns | 127.6 ns | 320 B | 256 B |
| FV-Regex-Precompiled | 288.6 ns | 229.7 ns | 227.1 ns | 632 B | 632 B |
| **CSE advantage** | **2.0×** | **1.8×** | **1.8×** | 49% less | 2.5× less |

---

### 23. Nullable Validation (`int?` / `decimal?` with `NotNull` and range rules)

| Benchmark | Mean (net9) | Mean (net10) | Mean (net11) | Alloc (net9) | Alloc (net10) |
|---|---:|---:|---:|---:|---:|
| CSE-Nullable-Valid | 85.38 ns | 68.70 ns | 69.54 ns | 472 B | 344 B |
| FV-Nullable-Valid | 226.69 ns | 205.33 ns | 206.84 ns | 600 B | 600 B |
| CSE-Nullable-Invalid | 143.37 ns | 115.05 ns | 118.61 ns | 984 B | 856 B |
| FV-Nullable-Invalid | 981.24 ns | 813.24 ns | 859.12 ns | 2,976 B | 2,848 B |
| **CSE advantage (Nullable-Valid)** | **2.7×** | **3.0×** | **3.0×** | 21% less | 43% less |
| **CSE advantage (Nullable-Invalid)** | **6.8×** | **7.1×** | **7.2×** | 3.0× less | 3.3× less |

---

## Cross-Runtime Trend (.NET 9 → 10 → 11)

| Scenario | .NET 9 | .NET 10 | .NET 11 |
|---|---:|---:|---:|
| CSE-Simple-Invalid | 302.3 ns | 257.1 ns | 260.1 ns |
| FV-Simple-Invalid | 2,498.2 ns | 2,108.1 ns | 2,113.1 ns |
| CSE-LargeCollection | 7.618 μs | 5.811 μs | 5.703 μs |
| FV-LargeCollection | 21.944 μs | 18.024 μs | 17.690 μs |
| CSE-CollectionItem | 345.7 ns | 286.5 ns | 283.4 ns |
| FV-CollectionItem | 1,065.6 ns | 890.4 ns | 871.3 ns |
| CSE-DeepNested | 304.6 ns | 244.6 ns | 247.1 ns |
| FV-DeepNested | 853.9 ns | 716.7 ns | 703.5 ns |
| CSE-Ctor | 2.467 ns | 2.186 ns | 2.473 ns |
| FV-Ctor | 1,995.479 ns | 1,652.300 ns | 1,612.950 ns |

Key observation: both libraries get faster on net10 (roughly 15-25% in these rows) and net11 is flat against net10. The allocation story differs: CSE's allocation falls on net10 (LargeCollection 34.83 KB to 25.47 KB), FV's stays the same (46.63 KB).

---

## When FV Uses Less Memory (Valid Path Only)

Across all 23 scenarios there is one case where FV allocates less than CSE: the valid path of the wide model.

| Scenario | CSE alloc | FV alloc | Reason |
|---|---:|---:|---|
| Wide-Valid | 1,816 B | 984 B | Each `For(() => ...)` creates a `RuleChain<T, TProp>` and a closure delegate, 12 times per call |

FV builds its rule tree once, in the validator constructor, and reuses it. CSE builds nothing up front and pays a small
per-property cost on every call instead. The trade-off pays off on invalid input (CSE is 7.1× faster and allocates 3.2× less on the same wide model) and
on every other valid path. For most real-world workloads (user-facing forms, API request validation, write commands),
invalid input is common. Wide models like this one (12 fields) on purely valid paths are the one case where FV's constructor-built
design produces a lower allocation footprint. `rules.For(value, nameof(...))` avoids the delegate for hot paths.

---

## Summary Table (net9)

Speed and Memory are the FV figure divided by the CSE figure.

| Scenario | CSE | FV | Speed | Memory |
|---|---:|---:|---:|---:|
| ValidatorConstruction | 2.467 ns | 1,995.479 ns | **809×** | **401×** |
| Cascade-Continue | 209.7 ns | 1,692.2 ns | **8.1×** | **5.6×** |
| Simple-Invalid | 302.3 ns | 2,498.2 ns | **8.3×** | **5.0×** |
| Cascade-Stop | 110.4 ns | 818.8 ns | **7.4×** | **4.9×** |
| Wide-Invalid | 730.8 ns | 5,160.1 ns | **7.1×** | **3.2×** |
| Nullable-Invalid | 143.37 ns | 981.24 ns | **6.8×** | **3.0×** |
| LengthRange-Invalid | 146.40 ns | 973.32 ns | **6.6×** | **4.2×** |
| Complex-Invalid | 1,343.4 ns | 8,763.4 ns | **6.5×** | **3.3×** |
| MultiRegex-Invalid | 238.6 ns | 1,462.4 ns | **6.1×** | **4.4×** |
| Must-Invalid | 73.27 ns | 228.93 ns | **3.1×** | **2.0×** |
| CollectionItem | 345.7 ns | 1,065.6 ns | **3.1×** | **1.7×** |
| LargeCollection (50) | 7.618 μs | 21.944 μs | **2.9×** | **1.3×** |
| Comparable | 51.51 ns | 170.57 ns | **3.3×** | **2.0×** |
| Deep Nested | 304.6 ns | 853.9 ns | **2.8×** | **1.7×** |
| Complex-Valid | 827.8 ns | 2,193.4 ns | **2.6×** | **1.3×** |
| Wide-Valid | 450.5 ns | 839.0 ns | **1.9×** | FV 1.8× less |
| Async Predicate | 753.3 ns | 851.5 ns | **1.1×** | **1.6×** |

---

## How to Reproduce

```bash
cd benchmarks
chmod +x run-benchmarks.sh
./run-benchmarks.sh
```

Results are stored per-framework under `benchmarks/results/{tfm}/`.

Quick run (~10 min, net9 only, key scenarios, Short job):

```bash
./run-benchmarks.sh --quick
```

To run a single scenario:

```bash
cd benchmarks/CSharpEssentials.Validation.Benchmarks
dotnet run -c Release --framework net9.0 -- \
  --filter "*SimpleValidation*" \
  --exporters json github
```

Raw JSON + HTML + GitHub Markdown reports are in `benchmarks/results/{tfm}/results/`.

> **Harness note:** CSE benchmark methods consume the returned `ValueTask` through a `Run()` helper
> (`ValueTaskRunner.cs`) that reads completed `ValueTask`s directly, matching real `await` usage, instead of
> `.AsTask().GetAwaiter().GetResult()`, which allocates a `Task` even on the sync-completed fast path. The net9 reports
> were refreshed after that change; the net10 and net11 reports were not, so they come from an earlier run of the suite.
