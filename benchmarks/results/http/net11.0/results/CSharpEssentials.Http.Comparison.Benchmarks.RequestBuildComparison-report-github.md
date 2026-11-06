```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                 | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Build |   537.55 ns |  2.878 ns |  2.692 ns |  1.00 |    0.01 | 0.0982 |    1656 B |        1.00 |
| Bcl_Build              |    99.34 ns |  0.596 ns |  0.558 ns |  0.18 |    0.00 | 0.0238 |     400 B |        0.24 |
| Flurl_Build            | 2,512.40 ns | 26.063 ns | 24.379 ns |  4.67 |    0.05 | 0.3281 |    5504 B |        3.32 |
| RestSharp_Build        | 2,098.22 ns |  8.836 ns |  7.833 ns |  3.90 |    0.02 | 0.2289 |    3920 B |        2.37 |
