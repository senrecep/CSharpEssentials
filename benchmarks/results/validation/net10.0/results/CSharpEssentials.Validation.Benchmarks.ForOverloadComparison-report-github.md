```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method         | Valid | Mean       | Error    | StdDev  | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |------ |-----------:|---------:|--------:|------:|-------:|-------:|----------:|------------:|
| **ForLambda**      | **False** | **1,349.4 ns** |  **3.27 ns** | **3.06 ns** |  **1.00** | **0.2766** | **0.0019** |    **4632 B** |        **1.00** |
| ForValue       | False |   848.1 ns | 10.04 ns | 8.90 ns |  0.63 | 0.2060 | 0.0010 |    3456 B |        0.75 |
| InlineValidate | False | 1,310.6 ns | 10.16 ns | 9.50 ns |  0.97 | 0.2766 | 0.0019 |    4632 B |        1.00 |
|                |       |            |          |         |       |        |        |           |             |
| **ForLambda**      | **True**  |   **664.1 ns** |  **3.80 ns** | **3.56 ns** |  **1.00** | **0.1059** |      **-** |    **1784 B** |        **1.00** |
| ForValue       | True  |   203.6 ns |  3.03 ns | 2.84 ns |  0.31 | 0.0362 |      - |     608 B |        0.34 |
| InlineValidate | True  |   694.3 ns |  6.08 ns | 5.69 ns |  1.05 | 0.1059 |      - |    1784 B |        1.00 |
