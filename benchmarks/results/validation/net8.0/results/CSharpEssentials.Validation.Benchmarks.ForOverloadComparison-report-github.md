```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method         | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **ForLambda**      | **False** | **1,606.9 ns** |  **5.37 ns** |  **5.03 ns** |  **1.00** |    **0.00** | **0.2766** | **0.0019** |    **4632 B** |        **1.00** |
| ForValue       | False | 1,070.7 ns |  4.96 ns |  4.64 ns |  0.67 |    0.00 | 0.2060 |      - |    3456 B |        0.75 |
| InlineValidate | False | 1,524.9 ns |  6.77 ns |  6.33 ns |  0.95 |    0.00 | 0.2766 | 0.0019 |    4632 B |        1.00 |
|                |       |            |          |          |       |         |        |        |           |             |
| **ForLambda**      | **True**  |   **775.5 ns** | **15.17 ns** | **14.19 ns** |  **1.00** |    **0.03** | **0.1059** |      **-** |    **1784 B** |        **1.00** |
| ForValue       | True  |   221.1 ns |  1.16 ns |  1.03 ns |  0.29 |    0.01 | 0.0362 |      - |     608 B |        0.34 |
| InlineValidate | True  |   733.1 ns | 10.38 ns |  9.71 ns |  0.95 |    0.02 | 0.1059 |      - |    1784 B |        1.00 |
