```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Build |   641.8 ns | 5.48 ns | 5.13 ns |  1.00 |    0.01 | 0.0982 |    1648 B |        1.00 |
| Bcl_Build              |   127.1 ns | 1.34 ns | 1.26 ns |  0.20 |    0.00 | 0.0234 |     392 B |        0.24 |
| Flurl_Build            | 2,738.7 ns | 8.58 ns | 8.02 ns |  4.27 |    0.04 | 0.3242 |    5472 B |        3.32 |
| RestSharp_Build        | 2,300.7 ns | 6.12 ns | 5.11 ns |  3.58 |    0.03 | 0.2327 |    3936 B |        2.39 |
