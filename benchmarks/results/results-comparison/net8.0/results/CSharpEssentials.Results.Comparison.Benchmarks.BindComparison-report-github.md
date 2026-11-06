```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **2.0426 ns** | **0.0007 ns** | **0.0006 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   8.9011 ns | 0.0015 ns | 0.0013 ns |   4.36 |    0.00 |      - |         - |          NA |
| ErrorOr                    | False   |   2.9230 ns | 0.0194 ns | 0.0181 ns |   1.43 |    0.01 |      - |         - |          NA |
| FluentResults              | False   |  81.8065 ns | 0.4306 ns | 0.4028 ns |  40.05 |    0.19 | 0.0129 |     216 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.8896 ns** | **0.0078 ns** | **0.0065 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   9.2949 ns | 0.0830 ns | 0.0736 ns |  10.45 |    0.11 |      - |         - |          NA |
| ErrorOr                    | True    |   1.7044 ns | 0.0151 ns | 0.0141 ns |   1.92 |    0.02 |      - |         - |          NA |
| FluentResults              | True    | 156.2902 ns | 0.8387 ns | 0.7846 ns | 175.69 |    1.50 | 0.0210 |     352 B |          NA |
