```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Typed_CachedError     | CachedError |  3.5903 ns | 0.0709 ns | 0.1399 ns |  3.4864 ns |  1.00 |    0.05 |      - |      - |         - |          NA |
| CSharpEssentials_Result_CachedError    | CachedError | 18.8305 ns | 0.0856 ns | 0.0801 ns | 18.7961 ns |  5.25 |    0.20 | 0.0033 | 0.0007 |      56 B |          NA |
| CSharpEssentials_Any_CachedError       | CachedError |  3.4305 ns | 0.0670 ns | 0.0894 ns |  3.4726 ns |  0.96 |    0.04 |      - |      - |         - |          NA |
| OneOf_CachedError                      | CachedError |  3.7568 ns | 0.0334 ns | 0.0297 ns |  3.7648 ns |  1.05 |    0.04 |      - |      - |         - |          NA |
| Dunet_CachedError                      | CachedError | 12.3117 ns | 0.0409 ns | 0.0382 ns | 12.3077 ns |  3.43 |    0.13 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_CachedError                    | CachedError | 24.3050 ns | 0.1000 ns | 0.0935 ns | 24.3001 ns |  6.78 |    0.26 | 0.0052 | 0.0017 |      88 B |          NA |
| CSharpFunctionalExtensions_CachedError | CachedError |  3.5888 ns | 0.0714 ns | 0.1323 ns |  3.5002 ns |  1.00 |    0.05 |      - |      - |         - |          NA |
| FluentResults_CachedError              | CachedError | 33.4813 ns | 0.1217 ns | 0.1016 ns | 33.4990 ns |  9.34 |    0.36 | 0.0072 | 0.0023 |     120 B |          NA |
|                                        |             |            |           |           |            |       |         |        |        |           |             |
| CSharpEssentials_Typed_NewError        | NewError    |  9.0903 ns | 0.0772 ns | 0.0722 ns |  9.0704 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| CSharpEssentials_Result_NewError       | NewError    | 19.8204 ns | 0.2721 ns | 0.2545 ns | 19.7411 ns |  2.18 |    0.03 | 0.0033 | 0.0007 |      56 B |        1.40 |
| CSharpEssentials_Any_NewError          | NewError    |  9.2010 ns | 0.1428 ns | 0.1336 ns |  9.2504 ns |  1.01 |    0.02 | 0.0024 | 0.0003 |      40 B |        1.00 |
| OneOf_NewError                         | NewError    |  9.0491 ns | 0.0380 ns | 0.0337 ns |  9.0359 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| Dunet_NewError                         | NewError    | 17.6916 ns | 0.0950 ns | 0.0888 ns | 17.6867 ns |  1.95 |    0.02 | 0.0038 | 0.0009 |      64 B |        1.60 |
| ErrorOr_NewError                       | NewError    | 17.1119 ns | 0.0830 ns | 0.0777 ns | 17.1057 ns |  1.88 |    0.02 | 0.0052 | 0.0017 |      88 B |        2.20 |
| CSharpFunctionalExtensions_NewError    | NewError    |  9.3308 ns | 0.0381 ns | 0.0338 ns |  9.3197 ns |  1.03 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| FluentResults_NewError                 | NewError    | 68.1960 ns | 0.3221 ns | 0.2855 ns | 68.1636 ns |  7.50 |    0.07 | 0.0162 | 0.0161 |     272 B |        6.80 |
|                                        |             |            |           |           |            |       |         |        |        |           |             |
| CSharpEssentials_Typed_Ok              | Ok          |  0.5970 ns | 0.0008 ns | 0.0007 ns |  0.5968 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Result_Ok             | Ok          |  0.5827 ns | 0.0008 ns | 0.0008 ns |  0.5827 ns |  0.98 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Any_Ok                | Ok          |  8.4085 ns | 0.0421 ns | 0.0374 ns |  8.3960 ns | 14.09 |    0.06 | 0.0014 | 0.0001 |      24 B |          NA |
| OneOf_Ok                               | Ok          |  0.7509 ns | 0.0012 ns | 0.0012 ns |  0.7507 ns |  1.26 |    0.00 |      - |      - |         - |          NA |
| Dunet_Ok                               | Ok          | 11.1534 ns | 0.0463 ns | 0.0433 ns | 11.1450 ns | 18.68 |    0.07 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_Ok                             | Ok          |  0.5824 ns | 0.0007 ns | 0.0007 ns |  0.5823 ns |  0.98 |    0.00 |      - |      - |         - |          NA |
| CSharpFunctionalExtensions_Ok          | Ok          |  0.5894 ns | 0.0003 ns | 0.0002 ns |  0.5894 ns |  0.99 |    0.00 |      - |      - |         - |          NA |
| FluentResults_Ok                       | Ok          | 45.4871 ns | 0.1604 ns | 0.1340 ns | 45.5113 ns | 76.20 |    0.23 | 0.0072 | 0.0017 |     120 B |          NA |
