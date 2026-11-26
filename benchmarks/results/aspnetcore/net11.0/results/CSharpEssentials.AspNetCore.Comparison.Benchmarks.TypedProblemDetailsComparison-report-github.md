```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                             | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_TypedToHttpResult | 13.00 μs | 0.255 μs | 0.283 μs |  1.00 |    0.03 | 0.7324 |  12.92 KB |        1.00 |
| CSharpEssentials_ToProblemResult   | 12.49 μs | 0.250 μs | 0.307 μs |  0.96 |    0.03 | 0.7324 |  12.85 KB |        0.99 |
| HandWritten_TypedResultsProblem    | 12.76 μs | 0.255 μs | 0.357 μs |  0.98 |    0.03 | 0.6104 |  11.82 KB |        0.92 |
