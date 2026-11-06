```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** | **1,426.7 ns** |  **9.48 ns** |  **8.87 ns** |  **1.00** |    **0.01** | **0.2766** | **0.0019** |    **4632 B** |        **1.00** |
| FluentValidation | False | 8,808.2 ns | 64.03 ns | 53.46 ns |  6.17 |    0.05 | 0.8698 | 0.0153 |   14720 B |        3.18 |
| DataAnnotations  | False | 6,221.8 ns | 14.17 ns | 12.56 ns |  4.36 |    0.03 | 0.4807 | 0.0076 |    8112 B |        1.75 |
|                  |       |            |          |          |       |         |        |        |           |             |
| **CSharpEssentials** | **True**  |   **728.0 ns** |  **5.89 ns** |  **5.51 ns** |  **1.00** |    **0.01** | **0.1059** |      **-** |    **1784 B** |        **1.00** |
| FluentValidation | True  |   984.9 ns |  8.75 ns |  8.19 ns |  1.35 |    0.01 | 0.0572 |      - |     984 B |        0.55 |
| DataAnnotations  | True  | 3,842.8 ns | 16.66 ns | 15.58 ns |  5.28 |    0.04 | 0.3433 |      - |    5864 B |        3.29 |
