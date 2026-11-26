```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **8.902 ns** | **0.0187 ns** | **0.0175 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   5.478 ns | 0.0056 ns | 0.0053 ns |  0.62 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  30.930 ns | 0.0714 ns | 0.0668 ns |  3.47 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   6.610 ns | 0.0236 ns | 0.0221 ns |  0.74 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   7.346 ns | 0.0199 ns | 0.0166 ns |  0.83 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 310.874 ns | 1.4770 ns | 1.3816 ns | 34.92 |    0.16 | 0.0596 |    1000 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |   **8.076 ns** | **0.0081 ns** | **0.0071 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   6.538 ns | 0.0066 ns | 0.0055 ns |  0.81 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  31.756 ns | 0.0377 ns | 0.0334 ns |  3.93 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   4.516 ns | 0.0031 ns | 0.0028 ns |  0.56 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |   7.708 ns | 0.0137 ns | 0.0122 ns |  0.95 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 250.991 ns | 3.6771 ns | 3.2597 ns | 31.08 |    0.39 | 0.0591 |     992 B |          NA |
