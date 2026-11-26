```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Typed_CachedError     | CachedError |  2.6047 ns | 0.0018 ns | 0.0017 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Result_CachedError    | CachedError | 14.7804 ns | 0.0531 ns | 0.0497 ns |  5.67 |    0.02 | 0.0033 | 0.0007 |      56 B |          NA |
| CSharpEssentials_Any_CachedError       | CachedError |  2.6037 ns | 0.0015 ns | 0.0013 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| OneOf_CachedError                      | CachedError |  2.6077 ns | 0.0035 ns | 0.0031 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| Dunet_CachedError                      | CachedError | 11.6612 ns | 0.0474 ns | 0.0443 ns |  4.48 |    0.02 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_CachedError                    | CachedError | 21.5302 ns | 0.0688 ns | 0.0644 ns |  8.27 |    0.02 | 0.0052 | 0.0017 |      88 B |          NA |
| CSharpFunctionalExtensions_CachedError | CachedError |  2.6040 ns | 0.0015 ns | 0.0013 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| FluentResults_CachedError              | CachedError | 30.2794 ns | 0.1418 ns | 0.1326 ns | 11.62 |    0.05 | 0.0072 | 0.0023 |     120 B |          NA |
|                                        |             |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_Typed_NewError        | NewError    |  7.8162 ns | 0.0336 ns | 0.0315 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| CSharpEssentials_Result_NewError       | NewError    |  8.9502 ns | 0.0403 ns | 0.0357 ns |  1.15 |    0.01 | 0.0033 | 0.0007 |      56 B |        1.40 |
| CSharpEssentials_Any_NewError          | NewError    |  7.8310 ns | 0.0602 ns | 0.0564 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| OneOf_NewError                         | NewError    |  8.0987 ns | 0.0398 ns | 0.0372 ns |  1.04 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| Dunet_NewError                         | NewError    | 16.1199 ns | 0.0472 ns | 0.0394 ns |  2.06 |    0.01 | 0.0038 | 0.0009 |      64 B |        1.60 |
| ErrorOr_NewError                       | NewError    | 14.8849 ns | 0.0820 ns | 0.0727 ns |  1.90 |    0.01 | 0.0053 | 0.0017 |      88 B |        2.20 |
| CSharpFunctionalExtensions_NewError    | NewError    |  8.0833 ns | 0.0761 ns | 0.0712 ns |  1.03 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| FluentResults_NewError                 | NewError    | 53.6179 ns | 0.2051 ns | 0.1918 ns |  6.86 |    0.04 | 0.0162 | 0.0162 |     272 B |        6.80 |
|                                        |             |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_Typed_Ok              | Ok          |  0.5647 ns | 0.0006 ns | 0.0005 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Result_Ok             | Ok          |  0.3003 ns | 0.0012 ns | 0.0012 ns |  0.53 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Any_Ok                | Ok          |  6.9744 ns | 0.0330 ns | 0.0309 ns | 12.35 |    0.05 | 0.0014 | 0.0001 |      24 B |          NA |
| OneOf_Ok                               | Ok          |  0.7431 ns | 0.0004 ns | 0.0003 ns |  1.32 |    0.00 |      - |      - |         - |          NA |
| Dunet_Ok                               | Ok          | 10.0228 ns | 0.0386 ns | 0.0362 ns | 17.75 |    0.06 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_Ok                             | Ok          |  0.3051 ns | 0.0003 ns | 0.0002 ns |  0.54 |    0.00 |      - |      - |         - |          NA |
| CSharpFunctionalExtensions_Ok          | Ok          |  0.6250 ns | 0.0027 ns | 0.0026 ns |  1.11 |    0.00 |      - |      - |         - |          NA |
| FluentResults_Ok                       | Ok          | 26.3494 ns | 0.0829 ns | 0.0735 ns | 46.66 |    0.13 | 0.0067 | 0.0016 |     112 B |          NA |
