```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **24.654 ns** | **0.3845 ns** | **0.3597 ns** |  **1.00** |    **0.02** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  1.805 ns | 0.0026 ns | 0.0025 ns |  0.07 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 18.880 ns | 0.2173 ns | 0.2033 ns |  0.77 |    0.01 | 0.0052 |      88 B |        0.85 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.153 ns** | **0.0043 ns** | **0.0041 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  2.348 ns | 0.0030 ns | 0.0025 ns |  2.04 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |  1.150 ns | 0.0046 ns | 0.0043 ns |  1.00 |    0.00 |      - |         - |          NA |
