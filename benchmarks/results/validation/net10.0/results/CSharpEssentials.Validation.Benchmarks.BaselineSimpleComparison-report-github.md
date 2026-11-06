```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                        | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials**              | **False** |   **553.6 ns** |  **2.23 ns** |  **1.86 ns** |  **1.00** |    **0.00** | **0.0877** |      **-** |    **1472 B** |        **1.00** |
| FluentValidation              | False | 3,638.9 ns | 28.13 ns | 24.94 ns |  6.57 |    0.05 | 0.4234 | 0.0038 |    7136 B |        4.85 |
| DataAnnotations               | False | 2,374.5 ns | 12.08 ns | 11.30 ns |  4.29 |    0.02 | 0.1945 |      - |    3272 B |        2.22 |
| MicrosoftExtensionsValidation | False | 1,767.3 ns |  8.84 ns |  8.27 ns |  3.19 |    0.02 | 0.1221 |      - |    2056 B |        1.40 |
|                               |       |            |          |          |       |         |        |        |           |             |
| **CSharpEssentials**              | **True**  |   **311.0 ns** |  **1.23 ns** |  **1.15 ns** |  **1.00** |    **0.01** | **0.0334** |      **-** |     **560 B** |        **1.00** |
| FluentValidation              | True  |   364.9 ns |  1.94 ns |  1.72 ns |  1.17 |    0.01 | 0.0420 |      - |     704 B |        1.26 |
| DataAnnotations               | True  | 1,346.7 ns |  8.66 ns |  8.10 ns |  4.33 |    0.03 | 0.1354 |      - |    2288 B |        4.09 |
| MicrosoftExtensionsValidation | True  |   858.6 ns |  3.48 ns |  3.08 ns |  2.76 |    0.01 | 0.0439 |      - |     744 B |        1.33 |
