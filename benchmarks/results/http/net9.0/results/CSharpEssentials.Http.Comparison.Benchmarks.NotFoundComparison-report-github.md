```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                    | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |    874.5 ns |   4.17 ns |   3.90 ns |  1.00 |    0.01 | 0.0963 |   1.58 KB |        1.00 |
| Bcl_NotFound              |    736.0 ns |   3.82 ns |   3.57 ns |  0.84 |    0.01 | 0.0820 |   1.35 KB |        0.86 |
| Refit_NotFound            |  3,222.9 ns |  42.70 ns |  39.94 ns |  3.69 |    0.05 | 0.2136 |   3.91 KB |        2.48 |
| Flurl_NotFound            |  4,104.0 ns |  22.28 ns |  20.84 ns |  4.69 |    0.03 | 0.2975 |   4.88 KB |        3.09 |
| RestSharp_NotFound        | 11,456.7 ns | 227.43 ns | 212.74 ns | 13.10 |    0.24 | 1.8311 |  30.34 KB |       19.22 |
