```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| NoIdempotency | 11.05 μs | 0.215 μs | 0.201 μs |  1.00 |    0.03 | 0.4883 |      - |   9.47 KB |        1.00 |
| NewKey        | 30.04 μs | 0.594 μs | 0.751 μs |  2.72 |    0.08 | 0.7324 | 0.2441 |  14.69 KB |        1.55 |
| Replay        | 16.26 μs | 0.321 μs | 0.316 μs |  1.47 |    0.04 | 0.6104 |      - |  11.53 KB |        1.22 |
