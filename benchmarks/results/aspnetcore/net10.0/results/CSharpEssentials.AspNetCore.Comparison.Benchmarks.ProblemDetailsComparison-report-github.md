```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                  | Categories  | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Bcl_TypedResultsProblem                 | ErrorResult | 10.99 μs | 0.193 μs | 0.161 μs |  1.00 |    0.02 | 0.6104 |  10.31 KB |        1.00 |
| CSharpEssentials_ToProblemResult        | ErrorResult | 12.65 μs | 0.249 μs | 0.332 μs |  1.15 |    0.03 | 0.6104 |  11.38 KB |        1.10 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_AddProblemDetails                   | Exception   | 28.31 μs | 0.504 μs | 0.471 μs |  1.00 |    0.02 | 0.7324 |  12.32 KB |        1.00 |
| CSharpEssentials_GlobalExceptionHandler | Exception   | 29.68 μs | 0.559 μs | 0.665 μs |  1.05 |    0.03 | 0.7324 |  12.39 KB |        1.01 |
| Hellang_UseProblemDetails               | Exception   | 28.05 μs | 0.375 μs | 0.313 μs |  0.99 |    0.02 | 0.7324 |  12.98 KB |        1.05 |
|                                         |             |          |          |          |       |         |        |           |             |
| Bcl_StatusCode                          | StatusCode  | 11.16 μs | 0.220 μs | 0.278 μs |  1.00 |    0.03 | 0.6104 |   10.3 KB |        1.00 |
| CSharpEssentials_StatusCode             | StatusCode  | 11.29 μs | 0.101 μs | 0.095 μs |  1.01 |    0.03 | 0.6104 |  10.35 KB |        1.00 |
| Hellang_StatusCode                      | StatusCode  | 15.38 μs | 0.306 μs | 0.658 μs |  1.38 |    0.07 | 0.7324 |   12.3 KB |        1.19 |
