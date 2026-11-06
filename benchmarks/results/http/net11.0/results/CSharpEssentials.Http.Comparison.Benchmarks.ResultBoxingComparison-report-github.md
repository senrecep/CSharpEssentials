```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                      | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_NonGeneric** | **False**     | **295.6 ns** | **0.38 ns** | **0.34 ns** |  **1.00** | **0.0067** |     **112 B** |        **1.00** |
| CSharpEssentials_Generic    | False     | 251.0 ns | 0.22 ns | 0.17 ns |  0.85 | 0.0048 |      80 B |        0.71 |
| Polly_Generic               | False     | 229.2 ns | 0.15 ns | 0.13 ns |  0.78 |      - |         - |        0.00 |
|                             |           |          |         |         |       |        |           |             |
| **CSharpEssentials_NonGeneric** | **True**      | **483.9 ns** | **0.36 ns** | **0.32 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| CSharpEssentials_Generic    | True      | 402.7 ns | 0.45 ns | 0.40 ns |  0.83 | 0.0048 |      80 B |        0.56 |
| Polly_Generic               | True      | 363.1 ns | 0.12 ns | 0.11 ns |  0.75 |      - |         - |        0.00 |
