```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method        | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| NoIdempotency |  9.263 μs | 0.0999 μs | 0.0886 μs |  1.00 |    0.01 | 0.4883 |      - |   9.34 KB |        1.00 |
| NewKey        | 20.320 μs | 0.4029 μs | 0.5649 μs |  2.19 |    0.06 | 0.7324 | 0.4883 |  14.13 KB |        1.51 |
| Replay        | 12.781 μs | 0.2418 μs | 0.2374 μs |  1.38 |    0.03 | 0.6104 |      - |  11.19 KB |        1.20 |
