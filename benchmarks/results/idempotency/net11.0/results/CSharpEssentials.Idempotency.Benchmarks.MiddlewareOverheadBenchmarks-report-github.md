```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method        | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| NoIdempotency |  9.695 μs | 0.1935 μs | 0.3390 μs |  9.508 μs |  1.00 |    0.05 | 0.6104 |      - |  10.77 KB |        1.00 |
| NewKey        | 21.487 μs | 0.3886 μs | 0.6599 μs | 21.326 μs |  2.22 |    0.10 | 0.7324 | 0.2441 |  15.69 KB |        1.46 |
| Replay        | 13.388 μs | 0.1730 μs | 0.1445 μs | 13.415 μs |  1.38 |    0.05 | 0.7324 |      - |   12.6 KB |        1.17 |
