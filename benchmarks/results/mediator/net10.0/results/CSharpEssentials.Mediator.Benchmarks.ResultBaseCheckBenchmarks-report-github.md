```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **ClassResponse**     | **False**   | **0.4359 ns** | **0.0057 ns** | **0.0053 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Result            | False   | 0.2967 ns | 0.0157 ns | 0.0140 ns |  0.68 |    0.03 |         - |          NA |
| Typed             | False   | 0.7307 ns | 0.0020 ns | 0.0018 ns |  1.68 |    0.02 |         - |          NA |
| Typed_Constrained | False   | 2.7454 ns | 0.0076 ns | 0.0071 ns |  6.30 |    0.08 |         - |          NA |
|                   |         |           |           |           |       |         |           |             |
| **ClassResponse**     | **True**    | **0.4463 ns** | **0.0058 ns** | **0.0054 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Result            | True    | 0.2964 ns | 0.0162 ns | 0.0152 ns |  0.66 |    0.03 |         - |          NA |
| Typed             | True    | 0.7357 ns | 0.0017 ns | 0.0013 ns |  1.65 |    0.02 |         - |          NA |
| Typed_Constrained | True    | 2.7422 ns | 0.0097 ns | 0.0090 ns |  6.15 |    0.07 |         - |          NA |
