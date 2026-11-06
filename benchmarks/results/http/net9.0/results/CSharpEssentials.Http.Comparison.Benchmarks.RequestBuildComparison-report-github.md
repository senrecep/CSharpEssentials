```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Build |   722.7 ns |  3.43 ns |  3.20 ns |  1.00 |    0.01 | 0.0982 |    1648 B |        1.00 |
| Bcl_Build              |   161.4 ns |  0.92 ns |  0.81 ns |  0.22 |    0.00 | 0.0234 |     392 B |        0.24 |
| Flurl_Build            | 3,518.5 ns | 13.04 ns | 12.20 ns |  4.87 |    0.03 | 0.3357 |    5624 B |        3.41 |
| RestSharp_Build        | 2,873.0 ns |  9.53 ns |  8.45 ns |  3.98 |    0.02 | 0.2518 |    4224 B |        2.56 |
