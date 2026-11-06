```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** | **3,620.93 ns** |  **4.084 ns** |  **3.188 ns** |  **1.00** |    **0.00** | **0.0687** |    **1184 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False | 3,510.75 ns |  5.081 ns |  4.753 ns |  0.97 |    0.00 | 0.0648 |    1128 B |        0.95 |
| ErrorOr                    | False   | False | 3,527.63 ns |  5.746 ns |  5.094 ns |  0.97 |    0.00 | 0.0725 |    1216 B |        1.03 |
| FluentResults              | False   | False | 3,742.70 ns | 22.699 ns | 21.232 ns |  1.03 |    0.01 | 0.0877 |    1488 B |        1.26 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **5,330.68 ns** | **58.750 ns** | **49.059 ns** |  **1.00** |    **0.01** | **0.0916** |    **1568 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 5,285.87 ns | 87.289 ns | 81.650 ns |  0.99 |    0.02 | 0.0763 |    1504 B |        0.96 |
| ErrorOr                    | False   | True  | 5,256.61 ns | 93.062 ns | 87.050 ns |  0.99 |    0.02 | 0.0916 |    1577 B |        1.01 |
| FluentResults              | False   | True  | 5,349.56 ns | 72.091 ns | 67.434 ns |  1.00 |    0.02 | 0.1068 |    1857 B |        1.18 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **33.61 ns** |  **0.332 ns** |  **0.278 ns** |  **1.00** |    **0.01** | **0.0091** |     **152 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |    35.01 ns |  0.393 ns |  0.368 ns |  1.04 |    0.01 | 0.0091 |     152 B |        1.00 |
| ErrorOr                    | True    | False |    31.48 ns |  0.261 ns |  0.244 ns |  0.94 |    0.01 | 0.0091 |     152 B |        1.00 |
| FluentResults              | True    | False |    58.07 ns |  0.321 ns |  0.300 ns |  1.73 |    0.02 | 0.0153 |     256 B |        1.68 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **3,259.51 ns** | **31.758 ns** | **26.519 ns** |  **1.00** |    **0.01** | **0.0114** |     **241 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 3,265.89 ns | 24.128 ns | 22.569 ns |  1.00 |    0.01 | 0.0114 |     232 B |        0.96 |
| ErrorOr                    | True    | True  | 3,232.28 ns | 32.223 ns | 25.158 ns |  0.99 |    0.01 | 0.0114 |     216 B |        0.90 |
| FluentResults              | True    | True  | 3,338.83 ns | 63.262 ns | 56.080 ns |  1.02 |    0.02 | 0.0191 |     328 B |        1.36 |
