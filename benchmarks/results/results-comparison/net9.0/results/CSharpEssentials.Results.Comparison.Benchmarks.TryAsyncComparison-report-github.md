```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |  **8,535.95 ns** |  **27.348 ns** |  **24.244 ns** |  **1.00** |    **0.00** | **0.0763** |    **1512 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |  8,389.92 ns |  13.918 ns |  12.338 ns |  0.98 |    0.00 | 0.0763 |    1456 B |        0.96 |
| ErrorOr                    | False   | False |  8,369.64 ns |  18.905 ns |  16.759 ns |  0.98 |    0.00 | 0.0916 |    1544 B |        1.02 |
| FluentResults              | False   | False |  8,653.26 ns |  16.285 ns |  15.233 ns |  1.01 |    0.00 | 0.1068 |    1816 B |        1.20 |
|                            |         |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **10,083.47 ns** | **107.582 ns** | **100.632 ns** |  **1.00** |    **0.01** | **0.0916** |    **1601 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 10,325.99 ns | 141.472 ns | 132.333 ns |  1.02 |    0.02 | 0.0916 |    1537 B |        0.96 |
| ErrorOr                    | False   | True  | 10,399.64 ns |  96.167 ns |  89.955 ns |  1.03 |    0.01 | 0.0916 |    1610 B |        1.01 |
| FluentResults              | False   | True  | 10,729.65 ns | 113.617 ns | 106.278 ns |  1.06 |    0.01 | 0.1068 |    1890 B |        1.18 |
|                            |         |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |     **38.84 ns** |   **0.154 ns** |   **0.128 ns** |  **1.00** |    **0.00** | **0.0091** |     **152 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |     40.61 ns |   0.201 ns |   0.179 ns |  1.05 |    0.01 | 0.0091 |     152 B |        1.00 |
| ErrorOr                    | True    | False |     36.09 ns |   0.250 ns |   0.233 ns |  0.93 |    0.01 | 0.0091 |     152 B |        1.00 |
| FluentResults              | True    | False |     73.02 ns |   0.522 ns |   0.488 ns |  1.88 |    0.01 | 0.0153 |     256 B |        1.68 |
|                            |         |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  |    **837.54 ns** |   **4.616 ns** |   **4.318 ns** |  **1.00** |    **0.01** | **0.0143** |     **239 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  |    845.71 ns |   5.319 ns |   4.975 ns |  1.01 |    0.01 | 0.0134 |     232 B |        0.97 |
| ErrorOr                    | True    | True  |    837.39 ns |   3.699 ns |   3.460 ns |  1.00 |    0.01 | 0.0124 |     216 B |        0.90 |
| FluentResults              | True    | True  |    875.22 ns |   5.312 ns |   4.969 ns |  1.05 |    0.01 | 0.0191 |     328 B |        1.37 |
