```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Metadata  | Valid | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |---------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **Plain**     | **False** |   **332.980 ns** |  **1.1281 ns** |  **1.0001 ns** | **1.000** |    **0.00** | **0.0467** |     **784 B** |        **1.00** |
| FluentValidation | Plain     | False | 1,819.170 ns | 11.2318 ns | 10.5062 ns | 5.463 |    0.03 | 0.1526 |    2584 B |        3.30 |
| DataAnnotations  | Plain     | False |   796.986 ns |  1.9579 ns |  1.5286 ns | 2.394 |    0.01 | 0.0648 |    1096 B |        1.40 |
| HandWritten      | Plain     | False |     1.316 ns |  0.0149 ns |  0.0139 ns | 0.004 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials** | **Plain**     | **True**  |    **74.420 ns** |  **0.2943 ns** |  **0.2753 ns** |  **1.00** |    **0.01** | **0.0081** |     **136 B** |        **1.00** |
| FluentValidation | Plain     | True  |   909.886 ns |  3.0934 ns |  2.8936 ns | 12.23 |    0.06 | 0.0458 |     816 B |        6.00 |
| DataAnnotations  | Plain     | True  |   573.525 ns |  2.0995 ns |  1.9638 ns |  7.71 |    0.04 | 0.0553 |     936 B |        6.88 |
| HandWritten      | Plain     | True  |     1.309 ns |  0.0053 ns |  0.0044 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **False** |   **230.591 ns** |  **0.9703 ns** |  **0.8601 ns** | **1.000** |    **0.01** | **0.0439** |     **736 B** |        **1.00** |
| FluentValidation | Generated | False | 1,908.844 ns |  9.1332 ns |  8.0964 ns | 8.278 |    0.05 | 0.1526 |    2632 B |        3.58 |
| DataAnnotations  | Generated | False |   958.122 ns |  3.1816 ns |  2.9761 ns | 4.155 |    0.02 | 0.0610 |    1096 B |        1.49 |
| HandWritten      | Generated | False |     1.317 ns |  0.0040 ns |  0.0038 ns | 0.006 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |            |            |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **True**  |    **49.331 ns** |  **0.1886 ns** |  **0.1764 ns** |  **1.00** |    **0.00** | **0.0081** |     **136 B** |        **1.00** |
| FluentValidation | Generated | True  |   985.342 ns |  5.0989 ns |  4.5200 ns | 19.97 |    0.11 | 0.0515 |     864 B |        6.35 |
| DataAnnotations  | Generated | True  |   670.654 ns |  3.4167 ns |  3.0288 ns | 13.60 |    0.08 | 0.0534 |     936 B |        6.88 |
| HandWritten      | Generated | True  |     1.326 ns |  0.0053 ns |  0.0050 ns |  0.03 |    0.00 |      - |         - |        0.00 |
