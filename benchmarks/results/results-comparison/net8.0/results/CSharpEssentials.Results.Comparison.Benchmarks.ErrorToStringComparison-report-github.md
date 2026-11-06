```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                             | Categories        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |------------------ |----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Error             | Error             | 169.27 ns | 1.903 ns | 1.780 ns |  1.00 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| ErrorOr_Error                      | Error             | 172.77 ns | 1.563 ns | 1.385 ns |  1.02 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| FluentResults_Error                | Error             | 116.80 ns | 1.179 ns | 1.045 ns |  0.69 |    0.01 | 0.0229 |      - |     384 B |        0.47 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_ErrorWithMetadata | ErrorWithMetadata | 686.75 ns | 5.268 ns | 4.928 ns |  1.00 |    0.01 | 0.1326 |      - |    2224 B |        1.00 |
| ErrorOr_ErrorWithMetadata          | ErrorWithMetadata | 226.38 ns | 2.580 ns | 2.154 ns |  0.33 |    0.00 | 0.0763 | 0.0002 |    1280 B |        0.58 |
| FluentResults_ErrorWithMetadata    | ErrorWithMetadata | 400.80 ns | 1.381 ns | 1.292 ns |  0.58 |    0.00 | 0.0749 |      - |    1256 B |        0.56 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_Result            | Result            |  16.03 ns | 0.178 ns | 0.166 ns |  1.00 |    0.01 | 0.0043 |      - |      72 B |        1.00 |
| CSharpFunctionalExtensions_Result  | Result            |  51.51 ns | 0.158 ns | 0.148 ns |  3.21 |    0.03 | 0.0029 |      - |      48 B |        0.67 |
| ErrorOr_Result                     | Result            | 523.00 ns | 7.272 ns | 6.802 ns | 32.64 |    0.53 | 0.2098 | 0.0010 |    3520 B |       48.89 |
| FluentResults_Result               | Result            | 115.71 ns | 0.510 ns | 0.478 ns |  7.22 |    0.08 | 0.0176 |      - |     296 B |        4.11 |
