```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                      | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_NonGeneric** | **False**     | **401.9 ns** | **2.16 ns** | **1.92 ns** |  **1.00** | **0.0067** |     **112 B** |        **1.00** |
| CSharpEssentials_Generic    | False     | 290.5 ns | 0.18 ns | 0.14 ns |  0.72 | 0.0048 |      80 B |        0.71 |
| Polly_Generic               | False     | 256.3 ns | 0.14 ns | 0.12 ns |  0.64 |      - |         - |        0.00 |
|                             |           |          |         |         |       |        |           |             |
| **CSharpEssentials_NonGeneric** | **True**      | **677.6 ns** | **1.15 ns** | **1.02 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| CSharpEssentials_Generic    | True      | 483.1 ns | 0.31 ns | 0.26 ns |  0.71 | 0.0048 |      80 B |        0.56 |
| Polly_Generic               | True      | 414.8 ns | 0.22 ns | 0.20 ns |  0.61 |      - |         - |        0.00 |
