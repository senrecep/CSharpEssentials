```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                        | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials**              | **False** |   **499.1 ns** |  **2.60 ns** |  **2.43 ns** |  **1.00** |    **0.01** | **0.0858** |      **-** |    **1448 B** |        **1.00** |
| FluentValidation              | False | 3,372.4 ns | 10.73 ns | 10.04 ns |  6.76 |    0.04 | 0.4234 | 0.0038 |    7104 B |        4.91 |
| DataAnnotations               | False | 2,205.2 ns |  8.99 ns |  8.41 ns |  4.42 |    0.03 | 0.1907 |      - |    3240 B |        2.24 |
| MicrosoftExtensionsValidation | False | 1,495.4 ns |  6.26 ns |  5.55 ns |  3.00 |    0.02 | 0.1163 |      - |    1968 B |        1.36 |
|                               |       |            |          |          |       |         |        |        |           |             |
| **CSharpEssentials**              | **True**  |   **303.8 ns** |  **0.89 ns** |  **0.84 ns** |  **1.00** |    **0.00** | **0.0319** |      **-** |     **536 B** |        **1.00** |
| FluentValidation              | True  |   347.0 ns |  1.17 ns |  1.10 ns |  1.14 |    0.00 | 0.0420 |      - |     704 B |        1.31 |
| DataAnnotations               | True  | 1,262.3 ns |  4.20 ns |  3.51 ns |  4.15 |    0.02 | 0.1335 |      - |    2256 B |        4.21 |
| MicrosoftExtensionsValidation | True  |   802.7 ns |  6.98 ns |  6.53 ns |  2.64 |    0.02 | 0.0410 |      - |     696 B |        1.30 |
