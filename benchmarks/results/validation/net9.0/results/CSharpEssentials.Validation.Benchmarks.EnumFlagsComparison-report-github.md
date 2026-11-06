```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** |   **566.5 ns** | **7.38 ns** | **6.55 ns** |  **1.00** |    **0.02** | **0.0582** |     **976 B** |        **1.00** |
| FluentValidation | False | 3,062.1 ns | 6.96 ns | 6.51 ns |  5.41 |    0.06 | 0.1755 |    2936 B |        3.01 |
| DataAnnotations  | False | 1,382.8 ns | 8.54 ns | 7.57 ns |  2.44 |    0.03 | 0.0687 |    1176 B |        1.20 |
|                  |       |            |         |         |       |         |        |           |             |
| **CSharpEssentials** | **True**  |   **134.0 ns** | **0.68 ns** | **0.60 ns** |  **1.00** |    **0.01** | **0.0167** |     **280 B** |        **1.00** |
| FluentValidation | True  | 1,854.2 ns | 6.35 ns | 5.94 ns | 13.84 |    0.07 | 0.0629 |    1064 B |        3.80 |
| DataAnnotations  | True  | 1,063.6 ns | 5.71 ns | 5.34 ns |  7.94 |    0.05 | 0.0629 |    1056 B |        3.77 |
