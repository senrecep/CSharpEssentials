```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **81.97 ns** | **0.708 ns** | **0.662 ns** |  **1.00** |    **0.01** | **0.0200** |     **336 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 356.16 ns | 1.754 ns | 1.641 ns |  4.35 |    0.04 | 0.0467 |     784 B |        2.33 |
| ErrorOr                    | False   |  36.77 ns | 0.259 ns | 0.230 ns |  0.45 |    0.00 | 0.0110 |     184 B |        0.55 |
| FluentResults              | False   | 197.59 ns | 1.072 ns | 1.003 ns |  2.41 |    0.02 | 0.0424 |     712 B |        2.12 |
|                            |         |           |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **13.91 ns** | **0.077 ns** | **0.072 ns** |  **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  33.03 ns | 0.376 ns | 0.352 ns |  2.37 |    0.03 | 0.0095 |     160 B |        3.33 |
| ErrorOr                    | True    |  11.36 ns | 0.061 ns | 0.057 ns |  0.82 |    0.01 | 0.0019 |      32 B |        0.67 |
| FluentResults              | True    |  97.63 ns | 0.579 ns | 0.542 ns |  7.02 |    0.05 | 0.0219 |     368 B |        7.67 |
