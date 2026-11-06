```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                      | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_NonGeneric** | **False**     | **302.5 ns** | **0.33 ns** | **0.29 ns** |  **1.00** | **0.0067** |     **112 B** |        **1.00** |
| CSharpEssentials_Generic    | False     | 266.0 ns | 0.37 ns | 0.34 ns |  0.88 | 0.0048 |      80 B |        0.71 |
| Polly_Generic               | False     | 247.6 ns | 0.09 ns | 0.08 ns |  0.82 |      - |         - |        0.00 |
|                             |           |          |         |         |       |        |           |             |
| **CSharpEssentials_NonGeneric** | **True**      | **505.3 ns** | **0.68 ns** | **0.60 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| CSharpEssentials_Generic    | True      | 444.9 ns | 0.46 ns | 0.36 ns |  0.88 | 0.0048 |      80 B |        0.56 |
| Polly_Generic               | True      | 397.7 ns | 0.38 ns | 0.31 ns |  0.79 |      - |         - |        0.00 |
