```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** |   **693.8 ns** |  **3.23 ns** |  **2.86 ns** |  **1.00** |    **0.01** | **0.0944** |    **1584 B** |        **1.00** |
| FluentValidation | False | 4,318.2 ns | 19.63 ns | 16.39 ns |  6.22 |    0.03 | 0.4349 |    7392 B |        4.67 |
| DataAnnotations  | False | 3,013.0 ns | 11.54 ns | 10.79 ns |  4.34 |    0.02 | 0.2136 |    3592 B |        2.27 |
|                  |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |   **372.2 ns** |  **1.27 ns** |  **1.06 ns** |  **1.00** |    **0.00** | **0.0372** |     **624 B** |        **1.00** |
| FluentValidation | True  |   437.9 ns |  2.97 ns |  2.78 ns |  1.18 |    0.01 | 0.0420 |     704 B |        1.13 |
| DataAnnotations  | True  | 1,752.0 ns | 12.08 ns | 11.30 ns |  4.71 |    0.03 | 0.1564 |    2632 B |        4.22 |
