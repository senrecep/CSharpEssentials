```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** | **5,385.12 ns** |   **8.208 ns** |   **7.677 ns** |  **1.00** |    **0.00** | **0.0839** |    **1512 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False | 5,076.65 ns |   7.375 ns |   6.538 ns |  0.94 |    0.00 | 0.0839 |    1456 B |        0.96 |
| ErrorOr                    | False   | False | 5,217.23 ns |   7.404 ns |   6.926 ns |  0.97 |    0.00 | 0.0916 |    1544 B |        1.02 |
| FluentResults              | False   | False | 5,576.34 ns |  10.911 ns |   9.111 ns |  1.04 |    0.00 | 0.1068 |    1816 B |        1.20 |
|                            |         |       |             |            |            |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **6,718.95 ns** | **107.200 ns** | **100.275 ns** |  **1.00** |    **0.02** | **0.0916** |    **1601 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 6,591.55 ns | 113.897 ns | 106.539 ns |  0.98 |    0.02 | 0.0916 |    1537 B |        0.96 |
| ErrorOr                    | False   | True  | 6,604.87 ns |  84.645 ns |  79.177 ns |  0.98 |    0.02 | 0.0916 |    1609 B |        1.00 |
| FluentResults              | False   | True  | 6,896.05 ns |  71.915 ns |  67.269 ns |  1.03 |    0.02 | 0.1068 |    1889 B |        1.18 |
|                            |         |       |             |            |            |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **34.40 ns** |   **0.228 ns** |   **0.213 ns** |  **1.00** |    **0.01** | **0.0091** |     **152 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |    36.56 ns |   0.363 ns |   0.340 ns |  1.06 |    0.01 | 0.0091 |     152 B |        1.00 |
| ErrorOr                    | True    | False |    33.64 ns |   0.289 ns |   0.270 ns |  0.98 |    0.01 | 0.0091 |     152 B |        1.00 |
| FluentResults              | True    | False |    58.34 ns |   0.487 ns |   0.456 ns |  1.70 |    0.02 | 0.0153 |     256 B |        1.68 |
|                            |         |       |             |            |            |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  |   **823.95 ns** |   **3.615 ns** |   **3.381 ns** |  **1.00** |    **0.01** | **0.0143** |     **239 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  |   843.04 ns |  15.787 ns |  14.767 ns |  1.02 |    0.02 | 0.0134 |     231 B |        0.97 |
| ErrorOr                    | True    | True  |   824.57 ns |   5.018 ns |   4.694 ns |  1.00 |    0.01 | 0.0124 |     216 B |        0.90 |
| FluentResults              | True    | True  |   851.43 ns |   5.600 ns |   5.238 ns |  1.03 |    0.01 | 0.0191 |     328 B |        1.37 |
