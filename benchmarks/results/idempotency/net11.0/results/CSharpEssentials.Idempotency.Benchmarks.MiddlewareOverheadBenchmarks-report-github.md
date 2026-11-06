```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| NoIdempotency | 12.88 μs | 0.255 μs | 0.373 μs |  1.00 |    0.04 | 0.6104 |      - |  10.75 KB |        1.00 |
| NewKey        | 30.46 μs | 0.599 μs | 0.690 μs |  2.37 |    0.08 | 0.7324 | 0.4883 |  15.39 KB |        1.43 |
| Replay        | 16.25 μs | 0.321 μs | 0.490 μs |  1.26 |    0.05 | 0.7324 |      - |  12.78 KB |        1.19 |
