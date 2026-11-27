```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **7.563 ns** | **0.0118 ns** | **0.0111 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   5.466 ns | 0.0044 ns | 0.0037 ns |  0.72 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  37.427 ns | 0.0977 ns | 0.0914 ns |  4.95 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   6.615 ns | 0.0408 ns | 0.0381 ns |  0.87 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   7.393 ns | 0.0097 ns | 0.0090 ns |  0.98 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 310.074 ns | 1.7353 ns | 1.6232 ns | 41.00 |    0.22 | 0.0596 |    1000 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |   **5.818 ns** | **0.0064 ns** | **0.0053 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   5.964 ns | 0.0133 ns | 0.0125 ns |  1.03 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  31.716 ns | 0.0581 ns | 0.0515 ns |  5.45 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   4.529 ns | 0.0120 ns | 0.0113 ns |  0.78 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |   8.124 ns | 0.0106 ns | 0.0089 ns |  1.40 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 244.909 ns | 1.4078 ns | 1.3169 ns | 42.10 |    0.22 | 0.0591 |     992 B |          NA |
