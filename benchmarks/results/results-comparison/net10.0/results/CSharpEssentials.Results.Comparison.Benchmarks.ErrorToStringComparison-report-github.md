```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                             | Categories        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |------------------ |----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Error             | Error             | 175.50 ns | 1.485 ns | 1.390 ns |  1.00 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| ErrorOr_Error                      | Error             | 170.33 ns | 0.873 ns | 0.682 ns |  0.97 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| FluentResults_Error                | Error             |  93.97 ns | 0.637 ns | 0.596 ns |  0.54 |    0.01 | 0.0229 |      - |     384 B |        0.47 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_ErrorWithMetadata | ErrorWithMetadata | 498.34 ns | 2.561 ns | 2.396 ns |  1.00 |    0.01 | 0.1192 |      - |    2008 B |        1.00 |
| ErrorOr_ErrorWithMetadata          | ErrorWithMetadata | 222.33 ns | 2.816 ns | 2.199 ns |  0.45 |    0.00 | 0.0763 | 0.0002 |    1280 B |        0.64 |
| FluentResults_ErrorWithMetadata    | ErrorWithMetadata | 319.48 ns | 1.751 ns | 1.552 ns |  0.64 |    0.00 | 0.0749 |      - |    1256 B |        0.63 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_Result            | Result            |  11.61 ns | 0.231 ns | 0.216 ns |  1.00 |    0.03 | 0.0029 |      - |      48 B |        1.00 |
| CSharpFunctionalExtensions_Result  | Result            |  34.73 ns | 0.266 ns | 0.222 ns |  2.99 |    0.06 | 0.0029 |      - |      48 B |        1.00 |
| ErrorOr_Result                     | Result            | 507.70 ns | 4.898 ns | 4.582 ns | 43.73 |    0.87 | 0.2089 | 0.0010 |    3496 B |       72.83 |
| FluentResults_Result               | Result            |  76.67 ns | 0.672 ns | 0.629 ns |  6.60 |    0.13 | 0.0157 |      - |     264 B |        5.50 |
