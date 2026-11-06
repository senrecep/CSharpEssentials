```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                             | Categories        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |------------------ |----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Error             | Error             | 170.78 ns | 1.695 ns | 1.586 ns |  1.00 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| ErrorOr_Error                      | Error             | 168.83 ns | 1.790 ns | 1.674 ns |  0.99 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| FluentResults_Error                | Error             |  89.03 ns | 1.004 ns | 0.939 ns |  0.52 |    0.01 | 0.0229 |      - |     384 B |        0.47 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_ErrorWithMetadata | ErrorWithMetadata | 487.71 ns | 3.125 ns | 2.923 ns |  1.00 |    0.01 | 0.1192 |      - |    2008 B |        1.00 |
| ErrorOr_ErrorWithMetadata          | ErrorWithMetadata | 221.38 ns | 2.107 ns | 1.868 ns |  0.45 |    0.00 | 0.0763 | 0.0002 |    1280 B |        0.64 |
| FluentResults_ErrorWithMetadata    | ErrorWithMetadata | 312.14 ns | 2.591 ns | 2.297 ns |  0.64 |    0.01 | 0.0749 |      - |    1256 B |        0.63 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_Result            | Result            |  11.32 ns | 0.136 ns | 0.120 ns |  1.00 |    0.01 | 0.0029 |      - |      48 B |        1.00 |
| CSharpFunctionalExtensions_Result  | Result            |  33.67 ns | 0.110 ns | 0.098 ns |  2.98 |    0.03 | 0.0029 |      - |      48 B |        1.00 |
| ErrorOr_Result                     | Result            | 486.55 ns | 5.044 ns | 4.471 ns | 42.99 |    0.58 | 0.2089 | 0.0010 |    3496 B |       72.83 |
| FluentResults_Result               | Result            |  71.67 ns | 0.389 ns | 0.364 ns |  6.33 |    0.07 | 0.0157 |      - |     264 B |        5.50 |
