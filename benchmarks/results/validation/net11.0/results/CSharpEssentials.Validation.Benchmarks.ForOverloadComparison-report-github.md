```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method         | Valid | Mean       | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |------ |-----------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
| **ForLambda**      | **False** | **1,264.8 ns** | **11.32 ns** | **10.59 ns** |  **1.00** | **0.2708** | **0.0019** |    **4536 B** |        **1.00** |
| ForValue       | False |   800.9 ns |  4.66 ns |  4.36 ns |  0.63 | 0.2060 | 0.0010 |    3456 B |        0.76 |
| InlineValidate | False | 1,228.2 ns |  6.51 ns |  6.08 ns |  0.97 | 0.2708 | 0.0019 |    4536 B |        1.00 |
|                |       |            |          |          |       |        |        |           |             |
| **ForLambda**      | **True**  |   **646.6 ns** |  **4.09 ns** |  **3.83 ns** |  **1.00** | **0.1001** |      **-** |    **1688 B** |        **1.00** |
| ForValue       | True  |   179.7 ns |  1.34 ns |  1.26 ns |  0.28 | 0.0362 |      - |     608 B |        0.36 |
| InlineValidate | True  |   666.9 ns |  8.21 ns |  7.68 ns |  1.03 | 0.1001 |      - |    1688 B |        1.00 |
