```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **215.15 ns** | **0.964 ns** | **0.902 ns** |  **1.00** |    **0.01** | **0.0415** |     **696 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 327.26 ns | 4.213 ns | 3.940 ns |  1.52 |    0.02 | 0.0467 |     784 B |        1.13 |
| ErrorOr                    | False   |  33.47 ns | 0.258 ns | 0.228 ns |  0.16 |    0.00 | 0.0110 |     184 B |        0.26 |
| FluentResults              | False   | 189.72 ns | 1.209 ns | 1.131 ns |  0.88 |    0.01 | 0.0424 |     712 B |        1.02 |
|                            |         |           |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **15.21 ns** | **0.164 ns** | **0.153 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  28.79 ns | 0.199 ns | 0.177 ns |  1.89 |    0.02 | 0.0095 |     160 B |        2.00 |
| ErrorOr                    | True    |  11.17 ns | 0.067 ns | 0.062 ns |  0.73 |    0.01 | 0.0019 |      32 B |        0.40 |
| FluentResults              | True    |  96.92 ns | 0.456 ns | 0.404 ns |  6.37 |    0.07 | 0.0219 |     368 B |        4.60 |
