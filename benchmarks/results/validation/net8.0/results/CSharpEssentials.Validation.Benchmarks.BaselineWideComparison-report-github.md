```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** | **1,613.8 ns** |  **6.63 ns** |  **6.20 ns** |  **1.00** |    **0.01** | **0.2766** | **0.0019** |    **4632 B** |        **1.00** |
| FluentValidation | False | 9,061.2 ns | 34.04 ns | 31.84 ns |  5.61 |    0.03 | 0.8698 | 0.0153 |   14720 B |        3.18 |
| DataAnnotations  | False | 7,389.3 ns | 14.70 ns | 13.75 ns |  4.58 |    0.02 | 0.5264 | 0.0076 |    8880 B |        1.92 |
|                  |       |            |          |          |       |         |        |        |           |             |
| **CSharpEssentials** | **True**  |   **743.2 ns** | **14.58 ns** | **16.79 ns** |  **1.00** |    **0.03** | **0.1059** |      **-** |    **1784 B** |        **1.00** |
| FluentValidation | True  | 1,110.3 ns |  2.61 ns |  2.44 ns |  1.49 |    0.03 | 0.0572 |      - |     984 B |        0.55 |
| DataAnnotations  | True  | 5,714.8 ns | 15.18 ns | 14.20 ns |  7.69 |    0.17 | 0.3967 |      - |    6744 B |        3.78 |
