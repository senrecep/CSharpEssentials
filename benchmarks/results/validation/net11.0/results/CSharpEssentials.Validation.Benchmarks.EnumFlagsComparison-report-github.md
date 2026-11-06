```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** |   **382.3 ns** |  **1.93 ns** |  **1.81 ns** |  **1.00** |    **0.01** | **0.0567** |     **952 B** |        **1.00** |
| FluentValidation | False | 2,328.3 ns | 13.20 ns | 12.35 ns |  6.09 |    0.04 | 0.1678 |    2864 B |        3.01 |
| DataAnnotations  | False | 1,188.0 ns |  6.07 ns |  5.38 ns |  3.11 |    0.02 | 0.0687 |    1152 B |        1.21 |
|                  |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |   **102.3 ns** |  **0.25 ns** |  **0.20 ns** |  **1.00** |    **0.00** | **0.0129** |     **216 B** |        **1.00** |
| FluentValidation | True  | 1,372.0 ns |  2.94 ns |  2.61 ns | 13.41 |    0.04 | 0.0610 |    1064 B |        4.93 |
| DataAnnotations  | True  |   854.1 ns |  3.99 ns |  3.73 ns |  8.35 |    0.04 | 0.0610 |    1032 B |        4.78 |
