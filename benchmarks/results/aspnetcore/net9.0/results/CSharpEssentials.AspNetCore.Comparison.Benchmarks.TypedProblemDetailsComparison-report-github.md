```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_TypedToHttpResult | 12.75 μs | 0.251 μs | 0.440 μs |  1.00 |    0.05 | 0.6104 |      - |   11.4 KB |        1.00 |
| CSharpEssentials_ToProblemResult   | 12.62 μs | 0.251 μs | 0.375 μs |  0.99 |    0.04 | 0.6104 |      - |  11.33 KB |        0.99 |
| HandWritten_TypedResultsProblem    | 11.17 μs | 0.219 μs | 0.243 μs |  0.88 |    0.03 | 0.6104 | 0.6104 |  10.28 KB |        0.90 |
