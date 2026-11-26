```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                             | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_TypedToHttpResult | 12.82 μs | 0.255 μs | 0.357 μs |  1.00 |    0.04 | 0.6104 |      - |  11.48 KB |        1.00 |
| CSharpEssentials_ToProblemResult   | 12.51 μs | 0.239 μs | 0.265 μs |  0.98 |    0.03 | 0.6104 |      - |  11.41 KB |        0.99 |
| HandWritten_TypedResultsProblem    | 10.84 μs | 0.206 μs | 0.229 μs |  0.85 |    0.03 | 0.6104 | 0.6104 |  10.33 KB |        0.90 |
