```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                        | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials**              | **False** | **1,318.2 ns** | **11.99 ns** | **11.22 ns** |  **1.00** |    **0.01** | **0.2766** | **0.0019** |    **4632 B** |        **1.00** |
| FluentValidation              | False | 7,942.1 ns | 43.20 ns | 40.41 ns |  6.03 |    0.06 | 0.8240 | 0.0153 |   13952 B |        3.01 |
| DataAnnotations               | False | 5,973.8 ns | 19.95 ns | 17.69 ns |  4.53 |    0.04 | 0.4730 | 0.0076 |    8008 B |        1.73 |
| MicrosoftExtensionsValidation | False | 3,902.2 ns |  8.35 ns |  7.81 ns |  2.96 |    0.03 | 0.2060 |      - |    3480 B |        0.75 |
|                               |       |            |          |          |       |         |        |        |           |             |
| **CSharpEssentials**              | **True**  |   **655.6 ns** |  **3.36 ns** |  **3.14 ns** |  **1.00** |    **0.01** | **0.1059** |      **-** |    **1784 B** |        **1.00** |
| FluentValidation              | True  |   812.3 ns |  2.10 ns |  1.97 ns |  1.24 |    0.01 | 0.0582 |      - |     984 B |        0.55 |
| DataAnnotations               | True  | 3,439.9 ns | 13.50 ns | 12.63 ns |  5.25 |    0.03 | 0.3433 |      - |    5760 B |        3.23 |
| MicrosoftExtensionsValidation | True  | 2,132.5 ns |  7.04 ns |  6.59 ns |  3.25 |    0.02 | 0.0763 |      - |    1312 B |        0.74 |
