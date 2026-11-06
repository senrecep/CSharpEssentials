```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **108.90 ns** | **1.413 ns** | **1.322 ns** |  **1.00** |    **0.02** | **0.0200** |     **336 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 464.36 ns | 0.945 ns | 0.884 ns |  4.26 |    0.05 | 0.0467 |     784 B |        2.33 |
| ErrorOr                    | False   |  46.15 ns | 0.577 ns | 0.540 ns |  0.42 |    0.01 | 0.0110 |     184 B |        0.55 |
| FluentResults              | False   | 257.75 ns | 1.576 ns | 1.474 ns |  2.37 |    0.03 | 0.0424 |     712 B |        2.12 |
|                            |         |           |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **16.71 ns** | **0.072 ns** | **0.068 ns** |  **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  53.38 ns | 0.342 ns | 0.303 ns |  3.20 |    0.02 | 0.0095 |     160 B |        3.33 |
| ErrorOr                    | True    |  11.81 ns | 0.049 ns | 0.044 ns |  0.71 |    0.00 | 0.0019 |      32 B |        0.67 |
| FluentResults              | True    | 140.42 ns | 0.537 ns | 0.502 ns |  8.41 |    0.04 | 0.0219 |     368 B |        7.67 |
