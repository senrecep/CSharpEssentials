```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **12.903 ns** | **0.0932 ns** | **0.0872 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  1.807 ns | 0.0043 ns | 0.0040 ns |  0.14 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 23.776 ns | 0.1285 ns | 0.1202 ns |  1.84 |    0.02 | 0.0052 |      88 B |        1.57 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.567 ns** | **0.0050 ns** | **0.0042 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  2.346 ns | 0.0033 ns | 0.0031 ns |  1.50 |    0.00 |      - |         - |          NA |
| ErrorOr                    | True    |  1.157 ns | 0.0090 ns | 0.0084 ns |  0.74 |    0.01 |      - |         - |          NA |
