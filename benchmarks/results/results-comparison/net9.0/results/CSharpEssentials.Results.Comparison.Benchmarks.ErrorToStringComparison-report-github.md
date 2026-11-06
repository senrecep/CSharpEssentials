```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                             | Categories        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------- |------------------ |----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Error             | Error             | 167.11 ns | 1.209 ns | 1.130 ns |  1.00 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| ErrorOr_Error                      | Error             | 165.79 ns | 1.988 ns | 1.859 ns |  0.99 |    0.01 | 0.0486 |      - |     816 B |        1.00 |
| FluentResults_Error                | Error             | 114.18 ns | 1.413 ns | 1.322 ns |  0.68 |    0.01 | 0.0229 |      - |     384 B |        0.47 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_ErrorWithMetadata | ErrorWithMetadata | 487.95 ns | 4.544 ns | 4.028 ns |  1.00 |    0.01 | 0.1192 |      - |    2008 B |        1.00 |
| ErrorOr_ErrorWithMetadata          | ErrorWithMetadata | 210.34 ns | 1.857 ns | 1.646 ns |  0.43 |    0.00 | 0.0763 | 0.0002 |    1280 B |        0.64 |
| FluentResults_ErrorWithMetadata    | ErrorWithMetadata | 362.69 ns | 0.995 ns | 0.882 ns |  0.74 |    0.01 | 0.0749 |      - |    1256 B |        0.63 |
|                                    |                   |           |          |          |       |         |        |        |           |             |
| CSharpEssentials_Result            | Result            |  11.33 ns | 0.127 ns | 0.106 ns |  1.00 |    0.01 | 0.0029 |      - |      48 B |        1.00 |
| CSharpFunctionalExtensions_Result  | Result            |  34.17 ns | 0.097 ns | 0.086 ns |  3.02 |    0.03 | 0.0029 |      - |      48 B |        1.00 |
| ErrorOr_Result                     | Result            | 506.15 ns | 6.480 ns | 6.061 ns | 44.66 |    0.66 | 0.2089 | 0.0010 |    3496 B |       72.83 |
| FluentResults_Result               | Result            |  99.13 ns | 1.269 ns | 1.187 ns |  8.75 |    0.13 | 0.0172 |      - |     288 B |        6.00 |
