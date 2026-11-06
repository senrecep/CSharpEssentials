```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Metadata  | Valid | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |---------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **Plain**     | **False** |   **475.177 ns** | **1.2129 ns** | **1.0752 ns** | **1.000** |    **0.00** | **0.0577** |     **968 B** |        **1.00** |
| FluentValidation | Plain     | False | 2,299.606 ns | 8.9746 ns | 8.3948 ns | 4.839 |    0.02 | 0.1564 |    2656 B |        2.74 |
| DataAnnotations  | Plain     | False | 1,145.331 ns | 2.9650 ns | 2.7735 ns | 2.410 |    0.01 | 0.0706 |    1192 B |        1.23 |
| HandWritten      | Plain     | False |     1.442 ns | 0.0082 ns | 0.0073 ns | 0.003 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |           |           |       |         |        |           |             |
| **CSharpEssentials** | **Plain**     | **True**  |   **122.387 ns** | **0.2967 ns** | **0.2775 ns** |  **1.00** |    **0.00** | **0.0119** |     **200 B** |        **1.00** |
| FluentValidation | Plain     | True  | 1,136.881 ns | 2.0580 ns | 1.7185 ns |  9.29 |    0.02 | 0.0477 |     816 B |        4.08 |
| DataAnnotations  | Plain     | True  |   745.837 ns | 3.1912 ns | 2.8289 ns |  6.09 |    0.03 | 0.0620 |    1048 B |        5.24 |
| HandWritten      | Plain     | True  |     1.434 ns | 0.0058 ns | 0.0052 ns |  0.01 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |           |           |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **False** |   **354.379 ns** | **1.2674 ns** | **1.1235 ns** | **1.000** |    **0.00** | **0.0548** |     **920 B** |        **1.00** |
| FluentValidation | Generated | False | 2,436.399 ns | 5.3125 ns | 4.7094 ns | 6.875 |    0.02 | 0.1602 |    2704 B |        2.94 |
| DataAnnotations  | Generated | False | 1,261.420 ns | 1.7589 ns | 1.5592 ns | 3.560 |    0.01 | 0.0706 |    1192 B |        1.30 |
| HandWritten      | Generated | False |     1.275 ns | 0.0049 ns | 0.0046 ns | 0.004 |    0.00 |      - |         - |        0.00 |
|                  |           |       |              |           |           |       |         |        |           |             |
| **CSharpEssentials** | **Generated** | **True**  |    **70.830 ns** | **0.3304 ns** | **0.3091 ns** |  **1.00** |    **0.01** | **0.0119** |     **200 B** |        **1.00** |
| FluentValidation | Generated | True  | 1,229.461 ns | 2.7394 ns | 2.4284 ns | 17.36 |    0.08 | 0.0515 |     864 B |        4.32 |
| DataAnnotations  | Generated | True  |   941.853 ns | 2.9860 ns | 2.7931 ns | 13.30 |    0.07 | 0.0620 |    1048 B |        5.24 |
| HandWritten      | Generated | True  |     1.272 ns | 0.0047 ns | 0.0044 ns |  0.02 |    0.00 |      - |         - |        0.00 |
