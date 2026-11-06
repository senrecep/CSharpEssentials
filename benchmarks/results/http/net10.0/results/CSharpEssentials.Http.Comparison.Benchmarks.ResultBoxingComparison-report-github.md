```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                      | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_NonGeneric** | **False**     | **302.4 ns** | **0.36 ns** | **0.32 ns** |  **1.00** | **0.0067** |     **112 B** |        **1.00** |
| CSharpEssentials_Generic    | False     | 254.4 ns | 0.25 ns | 0.22 ns |  0.84 | 0.0048 |      80 B |        0.71 |
| Polly_Generic               | False     | 228.1 ns | 0.09 ns | 0.08 ns |  0.75 |      - |         - |        0.00 |
|                             |           |          |         |         |       |        |           |             |
| **CSharpEssentials_NonGeneric** | **True**      | **517.2 ns** | **0.73 ns** | **0.68 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| CSharpEssentials_Generic    | True      | 412.6 ns | 0.21 ns | 0.18 ns |  0.80 | 0.0048 |      80 B |        0.56 |
| Polly_Generic               | True      | 373.1 ns | 0.19 ns | 0.17 ns |  0.72 |      - |         - |        0.00 |
