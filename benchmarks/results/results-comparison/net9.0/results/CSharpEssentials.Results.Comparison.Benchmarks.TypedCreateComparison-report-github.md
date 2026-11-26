```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Typed_CachedError     | CachedError |  2.9322 ns | 0.0035 ns | 0.0031 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Result_CachedError    | CachedError | 16.5638 ns | 0.3259 ns | 0.3880 ns |  5.65 |    0.13 | 0.0033 | 0.0007 |      56 B |          NA |
| CSharpEssentials_Any_CachedError       | CachedError |  2.8949 ns | 0.0040 ns | 0.0037 ns |  0.99 |    0.00 |      - |      - |         - |          NA |
| OneOf_CachedError                      | CachedError |  2.9114 ns | 0.0058 ns | 0.0054 ns |  0.99 |    0.00 |      - |      - |         - |          NA |
| Dunet_CachedError                      | CachedError | 12.5873 ns | 0.0445 ns | 0.0394 ns |  4.29 |    0.01 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_CachedError                    | CachedError | 24.7607 ns | 0.4261 ns | 0.3986 ns |  8.44 |    0.13 | 0.0052 | 0.0017 |      88 B |          NA |
| CSharpFunctionalExtensions_CachedError | CachedError |  2.6052 ns | 0.0021 ns | 0.0019 ns |  0.89 |    0.00 |      - |      - |         - |          NA |
| FluentResults_CachedError              | CachedError | 33.7394 ns | 0.1999 ns | 0.1870 ns | 11.51 |    0.06 | 0.0072 | 0.0023 |     120 B |          NA |
|                                        |             |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_Typed_NewError        | NewError    |  8.9831 ns | 0.0671 ns | 0.0628 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| CSharpEssentials_Result_NewError       | NewError    |  9.8149 ns | 0.0825 ns | 0.0771 ns |  1.09 |    0.01 | 0.0033 | 0.0007 |      56 B |        1.40 |
| CSharpEssentials_Any_NewError          | NewError    |  8.9074 ns | 0.0448 ns | 0.0397 ns |  0.99 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| OneOf_NewError                         | NewError    |  9.2933 ns | 0.0351 ns | 0.0312 ns |  1.03 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| Dunet_NewError                         | NewError    | 18.3141 ns | 0.1141 ns | 0.1068 ns |  2.04 |    0.02 | 0.0038 | 0.0009 |      64 B |        1.60 |
| ErrorOr_NewError                       | NewError    | 17.0623 ns | 0.1170 ns | 0.1095 ns |  1.90 |    0.02 | 0.0052 | 0.0017 |      88 B |        2.20 |
| CSharpFunctionalExtensions_NewError    | NewError    |  9.3451 ns | 0.0812 ns | 0.0760 ns |  1.04 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| FluentResults_NewError                 | NewError    | 69.5102 ns | 0.6382 ns | 0.5970 ns |  7.74 |    0.08 | 0.0162 | 0.0161 |     272 B |        6.80 |
|                                        |             |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_Typed_Ok              | Ok          |  0.5902 ns | 0.0005 ns | 0.0004 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Result_Ok             | Ok          |  0.3432 ns | 0.0004 ns | 0.0003 ns |  0.58 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Any_Ok                | Ok          |  8.4475 ns | 0.0503 ns | 0.0471 ns | 14.31 |    0.08 | 0.0014 | 0.0001 |      24 B |          NA |
| OneOf_Ok                               | Ok          |  0.7609 ns | 0.0014 ns | 0.0013 ns |  1.29 |    0.00 |      - |      - |         - |          NA |
| Dunet_Ok                               | Ok          | 10.8468 ns | 0.0519 ns | 0.0485 ns | 18.38 |    0.08 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_Ok                             | Ok          |  0.3421 ns | 0.0007 ns | 0.0007 ns |  0.58 |    0.00 |      - |      - |         - |          NA |
| CSharpFunctionalExtensions_Ok          | Ok          |  0.5916 ns | 0.0005 ns | 0.0005 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| FluentResults_Ok                       | Ok          | 38.7773 ns | 0.1839 ns | 0.1630 ns | 65.70 |    0.27 | 0.0067 | 0.0016 |     112 B |          NA |
