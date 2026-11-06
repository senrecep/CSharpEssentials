```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                        | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials**              | **False** | **1,298.4 ns** | **13.39 ns** | **11.87 ns** |  **1.00** |    **0.01** | **0.2708** | **0.0019** |    **4536 B** |        **1.00** |
| FluentValidation              | False | 8,061.0 ns | 46.87 ns | 43.84 ns |  6.21 |    0.06 | 0.8240 | 0.0153 |   13856 B |        3.05 |
| DataAnnotations               | False | 5,678.6 ns | 22.86 ns | 21.38 ns |  4.37 |    0.04 | 0.4654 | 0.0076 |    7912 B |        1.74 |
| MicrosoftExtensionsValidation | False | 3,439.1 ns | 12.99 ns | 12.15 ns |  2.65 |    0.03 | 0.2060 |      - |    3472 B |        0.77 |
|                               |       |            |          |          |       |         |        |        |           |             |
| **CSharpEssentials**              | **True**  |   **656.7 ns** |  **2.76 ns** |  **2.58 ns** |  **1.00** |    **0.01** | **0.1001** |      **-** |    **1688 B** |        **1.00** |
| FluentValidation              | True  |   780.1 ns |  1.97 ns |  1.84 ns |  1.19 |    0.01 | 0.0582 |      - |     984 B |        0.58 |
| DataAnnotations               | True  | 3,346.6 ns | 17.08 ns | 15.98 ns |  5.10 |    0.03 | 0.3357 |      - |    5664 B |        3.36 |
| MicrosoftExtensionsValidation | True  | 1,810.9 ns |  4.30 ns |  3.81 ns |  2.76 |    0.01 | 0.0763 |      - |    1304 B |        0.77 |
