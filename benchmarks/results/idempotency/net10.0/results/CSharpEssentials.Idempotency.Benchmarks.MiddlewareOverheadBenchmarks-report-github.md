```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| NoIdempotency | 10.34 μs | 0.180 μs | 0.160 μs |  1.00 |    0.02 | 0.4883 |      - |   9.34 KB |        1.00 |
| NewKey        | 25.35 μs | 0.492 μs | 0.567 μs |  2.45 |    0.07 | 0.7324 | 0.2441 |  14.48 KB |        1.55 |
| Replay        | 14.61 μs | 0.288 μs | 0.342 μs |  1.41 |    0.04 | 0.6104 | 0.6104 |  11.38 KB |        1.22 |
