```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                  | Categories  | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Bcl_TypedResultsProblem                 | ErrorResult | 13.16 μs | 0.214 μs | 0.190 μs |  1.00 |    0.02 | 0.6104 |  11.73 KB |        1.00 |
| CSharpEssentials_ToProblemResult        | ErrorResult | 14.52 μs | 0.290 μs | 0.573 μs |  1.10 |    0.05 | 0.7324 |   12.8 KB |        1.09 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_AddProblemDetails                   | Exception   | 26.67 μs | 0.361 μs | 0.320 μs |  1.00 |    0.02 | 0.7324 |  13.76 KB |        1.00 |
| CSharpEssentials_GlobalExceptionHandler | Exception   | 28.18 μs | 0.506 μs | 0.448 μs |  1.06 |    0.02 | 0.7324 |  13.86 KB |        1.01 |
| Hellang_UseProblemDetails               | Exception   | 26.97 μs | 0.406 μs | 0.360 μs |  1.01 |    0.02 | 0.7324 |  14.42 KB |        1.05 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_StatusCode                          | StatusCode  | 13.19 μs | 0.259 μs | 0.363 μs |  1.00 |    0.04 | 0.6104 |  11.72 KB |        1.00 |
| CSharpEssentials_StatusCode             | StatusCode  | 14.14 μs | 0.267 μs | 0.236 μs |  1.07 |    0.03 | 0.6104 |  11.78 KB |        1.00 |
| Hellang_StatusCode                      | StatusCode  | 16.63 μs | 0.330 μs | 0.643 μs |  1.26 |    0.06 | 0.7324 |  13.69 KB |        1.17 |
