```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** |   **536.6 ns** |  **2.14 ns** |  **2.01 ns** |  **1.00** |    **0.01** | **0.0648** |    **1096 B** |        **1.00** |
| FluentValidation | False | 3,224.1 ns | 16.34 ns | 15.28 ns |  6.01 |    0.04 | 0.1755 |    2936 B |        2.68 |
| DataAnnotations  | False | 1,459.2 ns |  6.08 ns |  4.74 ns |  2.72 |    0.01 | 0.0687 |    1248 B |        1.14 |
|                  |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |   **145.2 ns** |  **0.55 ns** |  **0.48 ns** |  **1.00** |    **0.00** | **0.0167** |     **280 B** |        **1.00** |
| FluentValidation | True  | 1,929.2 ns |  2.80 ns |  2.48 ns | 13.28 |    0.05 | 0.0610 |    1064 B |        3.80 |
| DataAnnotations  | True  | 1,181.6 ns |  6.39 ns |  5.97 ns |  8.14 |    0.05 | 0.0668 |    1144 B |        4.09 |
