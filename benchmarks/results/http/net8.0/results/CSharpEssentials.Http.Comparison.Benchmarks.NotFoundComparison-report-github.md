```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                    | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |  1,172.5 ns |   3.69 ns |   3.08 ns |  1.00 |    0.00 | 0.1106 |   1.81 KB |        1.00 |
| Bcl_NotFound              |    781.8 ns |   2.66 ns |   2.36 ns |  0.67 |    0.00 | 0.0820 |   1.35 KB |        0.75 |
| Refit_NotFound            |  3,512.0 ns |  21.04 ns |  19.68 ns |  3.00 |    0.02 | 0.2365 |   3.92 KB |        2.16 |
| Flurl_NotFound            |  4,263.0 ns |  11.53 ns |  10.22 ns |  3.64 |    0.01 | 0.2975 |   4.88 KB |        2.69 |
| RestSharp_NotFound        | 12,188.0 ns | 159.33 ns | 133.05 ns | 10.39 |    0.11 | 1.8311 |  30.47 KB |       16.80 |
