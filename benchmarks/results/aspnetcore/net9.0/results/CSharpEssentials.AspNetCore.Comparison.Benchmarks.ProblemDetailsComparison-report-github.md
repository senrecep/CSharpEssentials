```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                  | Categories  | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Bcl_TypedResultsProblem                 | ErrorResult | 11.28 μs | 0.205 μs | 0.244 μs |  1.00 |    0.03 | 0.6104 |  10.28 KB |        1.00 |
| CSharpEssentials_ToProblemResult        | ErrorResult | 13.50 μs | 0.241 μs | 0.305 μs |  1.20 |    0.04 | 0.6104 |   11.3 KB |        1.10 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_AddProblemDetails                   | Exception   | 35.79 μs | 0.686 μs | 0.891 μs |  1.00 |    0.03 | 0.7324 |  12.54 KB |        1.00 |
| CSharpEssentials_GlobalExceptionHandler | Exception   | 38.95 μs | 0.706 μs | 0.660 μs |  1.09 |    0.03 | 0.7324 |  12.62 KB |        1.01 |
| Hellang_UseProblemDetails               | Exception   | 36.17 μs | 0.377 μs | 0.352 μs |  1.01 |    0.03 | 0.7324 |  13.24 KB |        1.06 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_StatusCode                          | StatusCode  | 11.26 μs | 0.212 μs | 0.198 μs |  1.00 |    0.02 | 0.6104 |   10.3 KB |        1.00 |
| CSharpEssentials_StatusCode             | StatusCode  | 12.19 μs | 0.234 μs | 0.287 μs |  1.08 |    0.03 | 0.6104 |  10.32 KB |        1.00 |
| Hellang_StatusCode                      | StatusCode  | 17.55 μs | 0.349 μs | 0.801 μs |  1.56 |    0.08 | 0.7324 |  12.58 KB |        1.22 |
