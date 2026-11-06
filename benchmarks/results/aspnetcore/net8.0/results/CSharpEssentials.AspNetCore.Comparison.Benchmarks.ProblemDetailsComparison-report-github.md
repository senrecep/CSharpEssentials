```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                  | Categories  | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Bcl_TypedResultsProblem                 | ErrorResult | 11.85 μs | 0.216 μs | 0.231 μs |  1.00 |    0.03 | 0.4883 |   9.46 KB |        1.00 |
| CSharpEssentials_ToProblemResult        | ErrorResult | 14.92 μs | 0.291 μs | 0.471 μs |  1.26 |    0.05 | 0.6104 |  11.41 KB |        1.21 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_AddProblemDetails                   | Exception   | 53.93 μs | 0.711 μs | 0.665 μs |  1.00 |    0.02 | 0.7324 |  13.44 KB |        1.00 |
| CSharpEssentials_GlobalExceptionHandler | Exception   | 58.12 μs | 1.138 μs | 1.265 μs |  1.08 |    0.03 | 0.7324 |  13.77 KB |        1.02 |
| Hellang_UseProblemDetails               | Exception   | 50.37 μs | 0.773 μs | 0.723 μs |  0.93 |    0.02 | 0.7324 |  13.84 KB |        1.03 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_StatusCode                          | StatusCode  | 11.17 μs | 0.210 μs | 0.242 μs |  1.00 |    0.03 | 0.4883 |   9.47 KB |        1.00 |
| CSharpEssentials_StatusCode             | StatusCode  | 13.11 μs | 0.259 μs | 0.433 μs |  1.17 |    0.05 | 0.6104 |  10.43 KB |        1.10 |
| Hellang_StatusCode                      | StatusCode  | 20.29 μs | 0.401 μs | 0.782 μs |  1.82 |    0.08 | 0.7324 |  12.87 KB |        1.36 |
