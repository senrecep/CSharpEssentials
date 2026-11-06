```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** |   **422.1 ns** | **2.39 ns** | **2.11 ns** |  **1.00** |    **0.01** | **0.0567** |     **952 B** |        **1.00** |
| FluentValidation | False | 2,507.4 ns | 7.65 ns | 7.16 ns |  5.94 |    0.03 | 0.1717 |    2872 B |        3.02 |
| DataAnnotations  | False | 1,239.2 ns | 1.77 ns | 1.57 ns |  2.94 |    0.01 | 0.0687 |    1160 B |        1.22 |
|                  |       |            |         |         |       |         |        |           |             |
| **CSharpEssentials** | **True**  |   **116.0 ns** | **0.32 ns** | **0.30 ns** |  **1.00** |    **0.00** | **0.0129** |     **216 B** |        **1.00** |
| FluentValidation | True  | 1,407.2 ns | 2.80 ns | 2.62 ns | 12.13 |    0.04 | 0.0629 |    1064 B |        4.93 |
| DataAnnotations  | True  |   988.1 ns | 1.74 ns | 1.45 ns |  8.51 |    0.02 | 0.0610 |    1040 B |        4.81 |
