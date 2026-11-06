```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Metadata  | Valid | Mean         | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |---------- |------ |-------------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **Plain**     | **False** |   **443.807 ns** |  **1.6027 ns** | **1.4992 ns** | **1.000** |    **0.00** | **0.0529** |     **888 B** |        **1.00** |
| FluentValidation | Plain     | False | 2,174.562 ns |  9.6230 ns | 8.5305 ns | 4.900 |    0.02 | 0.1564 |    2656 B |        2.99 |
| DataAnnotations  | Plain     | False |   990.302 ns |  3.1656 ns | 2.9611 ns | 2.231 |    0.01 | 0.0668 |    1120 B |        1.26 |
| HandWritten      | Plain     | False |     1.437 ns |  0.0068 ns | 0.0060 ns | 0.003 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |           |       |         |        |           |             |
| **CSharpEssentials** | **Plain**     | **True**  |   **102.996 ns** |  **0.2437 ns** | **0.2280 ns** |  **1.00** |    **0.00** | **0.0119** |     **200 B** |        **1.00** |
| FluentValidation | Plain     | True  | 1,107.453 ns |  2.1585 ns | 2.0190 ns | 10.75 |    0.03 | 0.0477 |     816 B |        4.08 |
| DataAnnotations  | Plain     | True  |   709.908 ns |  1.8697 ns | 1.6574 ns |  6.89 |    0.02 | 0.0572 |     960 B |        4.80 |
| HandWritten      | Plain     | True  |     1.591 ns |  0.0063 ns | 0.0056 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |           |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **False** |   **341.526 ns** |  **1.0185 ns** | **0.9028 ns** | **1.000** |    **0.00** | **0.0501** |     **840 B** |        **1.00** |
| FluentValidation | Generated | False | 2,286.515 ns | 10.6198 ns | 9.9338 ns | 6.695 |    0.03 | 0.1602 |    2704 B |        3.22 |
| DataAnnotations  | Generated | False | 1,122.955 ns |  8.1691 ns | 7.6414 ns | 3.288 |    0.02 | 0.0668 |    1120 B |        1.33 |
| HandWritten      | Generated | False |     1.442 ns |  0.0088 ns | 0.0082 ns | 0.004 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |           |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **True**  |    **68.770 ns** |  **0.2693 ns** | **0.2519 ns** |  **1.00** |    **0.01** | **0.0119** |     **200 B** |        **1.00** |
| FluentValidation | Generated | True  | 1,219.284 ns |  3.6785 ns | 3.2609 ns | 17.73 |    0.08 | 0.0515 |     864 B |        4.32 |
| DataAnnotations  | Generated | True  |   831.465 ns |  1.9183 ns | 1.7944 ns | 12.09 |    0.05 | 0.0572 |     960 B |        4.80 |
| HandWritten      | Generated | True  |     1.437 ns |  0.0114 ns | 0.0106 ns |  0.02 |    0.00 |      - |         - |        0.00 |
