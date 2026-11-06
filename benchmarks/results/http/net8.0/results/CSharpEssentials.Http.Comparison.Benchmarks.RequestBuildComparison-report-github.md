```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Build |   733.4 ns |  3.40 ns |  2.66 ns |  1.00 |    0.00 | 0.0982 |    1648 B |        1.00 |
| Bcl_Build              |   189.1 ns |  1.47 ns |  1.37 ns |  0.26 |    0.00 | 0.0234 |     392 B |        0.24 |
| Flurl_Build            | 3,413.9 ns | 23.38 ns | 21.87 ns |  4.66 |    0.03 | 0.3357 |    5624 B |        3.41 |
| RestSharp_Build        | 2,981.3 ns | 10.35 ns |  9.17 ns |  4.07 |    0.02 | 0.2594 |    4376 B |        2.66 |
