```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **3.925 ns** | **0.0053 ns** | **0.0047 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   4.318 ns | 0.0112 ns | 0.0099 ns |  1.10 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  30.629 ns | 0.0202 ns | 0.0168 ns |  7.80 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   3.362 ns | 0.0051 ns | 0.0042 ns |  0.86 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   7.531 ns | 0.0246 ns | 0.0218 ns |  1.92 |    0.01 |      - |         - |          NA |
| FluentResults              | False   | 295.085 ns | 2.0661 ns | 1.9326 ns | 75.18 |    0.48 | 0.0596 |    1000 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |   **5.386 ns** | **0.0080 ns** | **0.0071 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   4.513 ns | 0.0038 ns | 0.0034 ns |  0.84 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  31.302 ns | 0.0217 ns | 0.0192 ns |  5.81 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   4.809 ns | 0.0078 ns | 0.0069 ns |  0.89 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  12.281 ns | 0.0056 ns | 0.0053 ns |  2.28 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 216.722 ns | 1.7477 ns | 1.6348 ns | 40.23 |    0.30 | 0.0591 |     992 B |          NA |
