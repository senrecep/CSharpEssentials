```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_TypedToHttpResult | 14.11 μs | 0.280 μs | 0.526 μs |  1.00 |    0.05 | 0.6104 |      - |  11.51 KB |        1.00 |
| CSharpEssentials_ToProblemResult   | 14.17 μs | 0.270 μs | 0.657 μs |  1.01 |    0.06 | 0.6104 |      - |  11.44 KB |        0.99 |
| HandWritten_TypedResultsProblem    | 12.31 μs | 0.241 μs | 0.382 μs |  0.87 |    0.04 | 0.6104 | 0.6104 |  10.39 KB |        0.90 |
