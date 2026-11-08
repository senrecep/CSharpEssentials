```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method        | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| NoIdempotency |  9.993 μs | 0.1362 μs | 0.1274 μs |  9.956 μs |  1.00 |    0.02 | 0.5493 |      - |   9.47 KB |        1.00 |
| NewKey        | 25.282 μs | 0.5494 μs | 1.5675 μs | 24.649 μs |  2.53 |    0.16 | 0.7324 | 0.2441 |  14.36 KB |        1.52 |
| Replay        | 14.720 μs | 0.2916 μs | 0.4791 μs | 14.586 μs |  1.47 |    0.05 | 0.6104 |      - |  11.34 KB |        1.20 |
