```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Valid | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** |   **674.2 ns** |  **2.42 ns** |  **2.15 ns** |  **1.00** |    **0.00** | **0.0916** |    **1536 B** |        **1.00** |
| FluentValidation | False | 3,982.8 ns | 19.87 ns | 18.59 ns |  5.91 |    0.03 | 0.4349 |    7392 B |        4.81 |
| DataAnnotations  | False | 2,560.4 ns |  6.73 ns |  6.29 ns |  3.80 |    0.01 | 0.1945 |    3312 B |        2.16 |
|                  |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |   **368.3 ns** |  **1.08 ns** |  **1.01 ns** |  **1.00** |    **0.00** | **0.0372** |     **624 B** |        **1.00** |
| FluentValidation | True  |   410.8 ns |  1.26 ns |  1.18 ns |  1.12 |    0.00 | 0.0420 |     704 B |        1.13 |
| DataAnnotations  | True  | 1,447.9 ns |  2.73 ns |  2.56 ns |  3.93 |    0.01 | 0.1373 |    2328 B |        3.73 |
