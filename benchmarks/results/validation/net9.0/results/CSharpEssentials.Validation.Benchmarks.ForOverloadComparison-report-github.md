```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method         | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **ForLambda**      | **False** | **1,432.5 ns** | **13.67 ns** | **12.79 ns** |  **1.00** |    **0.01** | **0.2766** | **0.0019** |    **4632 B** |        **1.00** |
| ForValue       | False |   905.2 ns |  7.58 ns |  7.09 ns |  0.63 |    0.01 | 0.2060 | 0.0010 |    3456 B |        0.75 |
| InlineValidate | False | 1,512.6 ns |  5.09 ns |  4.77 ns |  1.06 |    0.01 | 0.2766 | 0.0019 |    4632 B |        1.00 |
|                |       |            |          |          |       |         |        |        |           |             |
| **ForLambda**      | **True**  |   **749.0 ns** | **14.95 ns** | **14.69 ns** |  **1.00** |    **0.03** | **0.1059** |      **-** |    **1784 B** |        **1.00** |
| ForValue       | True  |   202.4 ns |  0.85 ns |  0.80 ns |  0.27 |    0.01 | 0.0362 |      - |     608 B |        0.34 |
| InlineValidate | True  |   729.7 ns |  7.68 ns |  6.81 ns |  0.97 |    0.02 | 0.1059 |      - |    1784 B |        1.00 |
