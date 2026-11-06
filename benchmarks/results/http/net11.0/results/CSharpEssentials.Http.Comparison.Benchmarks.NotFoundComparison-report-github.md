```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                    | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |   839.4 ns |  3.31 ns |  2.93 ns |  1.00 |    0.00 | 0.1068 |      - |   1.75 KB |        1.00 |
| Bcl_NotFound              |   618.8 ns |  2.72 ns |  2.55 ns |  0.74 |    0.00 | 0.0830 |      - |   1.37 KB |        0.78 |
| Refit_NotFound            | 2,673.4 ns |  7.31 ns |  6.48 ns |  3.18 |    0.01 | 0.2136 |      - |   3.92 KB |        2.24 |
| Flurl_NotFound            | 3,743.6 ns |  8.08 ns |  7.55 ns |  4.46 |    0.02 | 0.3052 |      - |   5.04 KB |        2.88 |
| RestSharp_NotFound        | 9,246.7 ns | 42.64 ns | 35.61 ns | 11.02 |    0.06 | 1.8311 | 0.1221 |  30.06 KB |       17.17 |
