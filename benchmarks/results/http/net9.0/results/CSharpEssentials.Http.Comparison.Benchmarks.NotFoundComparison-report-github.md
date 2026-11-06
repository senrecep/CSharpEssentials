```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                    | Mean        | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |  1,130.8 ns |   8.49 ns |  7.94 ns |  1.00 |    0.01 | 0.1049 |   1.74 KB |        1.00 |
| Bcl_NotFound              |    811.1 ns |  10.51 ns |  9.83 ns |  0.72 |    0.01 | 0.0820 |   1.35 KB |        0.78 |
| Refit_NotFound            |  3,133.2 ns |  18.13 ns | 15.14 ns |  2.77 |    0.02 | 0.2136 |   3.91 KB |        2.24 |
| Flurl_NotFound            |  4,145.6 ns |  29.63 ns | 26.26 ns |  3.67 |    0.03 | 0.2975 |   4.88 KB |        2.80 |
| RestSharp_NotFound        | 11,622.2 ns | 110.51 ns | 92.28 ns | 10.28 |    0.11 | 1.8311 |  30.34 KB |       17.41 |
