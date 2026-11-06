```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Metadata  | Valid | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |---------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **Plain**     | **False** |   **330.136 ns** |  **1.0565 ns** |  **0.8823 ns** | **1.000** |    **0.00** | **0.0467** |     **784 B** |        **1.00** |
| FluentValidation | Plain     | False | 1,869.646 ns |  7.1716 ns |  5.9886 ns | 5.663 |    0.02 | 0.1545 |    2592 B |        3.31 |
| DataAnnotations  | Plain     | False |   877.089 ns |  1.9006 ns |  1.7778 ns | 2.657 |    0.01 | 0.0658 |    1104 B |        1.41 |
| HandWritten      | Plain     | False |     1.271 ns |  0.0037 ns |  0.0035 ns | 0.004 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials** | **Plain**     | **True**  |    **72.593 ns** |  **0.0753 ns** |  **0.0668 ns** |  **1.00** |    **0.00** | **0.0081** |     **136 B** |        **1.00** |
| FluentValidation | Plain     | True  |   912.618 ns |  2.2128 ns |  1.9616 ns | 12.57 |    0.03 | 0.0486 |     816 B |        6.00 |
| DataAnnotations  | Plain     | True  |   619.671 ns |  1.8674 ns |  1.4579 ns |  8.54 |    0.02 | 0.0563 |     944 B |        6.94 |
| HandWritten      | Plain     | True  |     1.356 ns |  0.0188 ns |  0.0176 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **False** |   **243.459 ns** |  **1.5745 ns** |  **1.4728 ns** | **1.000** |    **0.01** | **0.0439** |     **736 B** |        **1.00** |
| FluentValidation | Generated | False | 2,004.561 ns | 12.5045 ns | 11.0849 ns | 8.234 |    0.07 | 0.1564 |    2640 B |        3.59 |
| DataAnnotations  | Generated | False | 1,018.847 ns |  5.0737 ns |  4.2368 ns | 4.185 |    0.03 | 0.0648 |    1104 B |        1.50 |
| HandWritten      | Generated | False |     1.284 ns |  0.0118 ns |  0.0104 ns | 0.005 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **True**  |    **50.615 ns** |  **0.1972 ns** |  **0.1748 ns** |  **1.00** |    **0.00** | **0.0081** |     **136 B** |        **1.00** |
| FluentValidation | Generated | True  |   973.457 ns |  3.5988 ns |  3.1902 ns | 19.23 |    0.09 | 0.0515 |     864 B |        6.35 |
| DataAnnotations  | Generated | True  |   664.971 ns |  2.4224 ns |  2.1474 ns | 13.14 |    0.06 | 0.0563 |     944 B |        6.94 |
| HandWritten      | Generated | True  |     1.277 ns |  0.0017 ns |  0.0014 ns |  0.03 |    0.00 |      - |         - |        0.00 |
