```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Typed_CachedError     | CachedError |  2.6078 ns | 0.0041 ns | 0.0038 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Result_CachedError    | CachedError | 13.4252 ns | 0.0747 ns | 0.0662 ns |  5.15 |    0.03 | 0.0033 | 0.0007 |      56 B |          NA |
| CSharpEssentials_Any_CachedError       | CachedError |  2.6045 ns | 0.0012 ns | 0.0011 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| OneOf_CachedError                      | CachedError |  2.8842 ns | 0.0056 ns | 0.0049 ns |  1.11 |    0.00 |      - |      - |         - |          NA |
| Dunet_CachedError                      | CachedError | 10.3519 ns | 0.0587 ns | 0.0521 ns |  3.97 |    0.02 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_CachedError                    | CachedError | 18.8802 ns | 0.0678 ns | 0.0635 ns |  7.24 |    0.03 | 0.0052 | 0.0017 |      88 B |          NA |
| CSharpFunctionalExtensions_CachedError | CachedError |  2.6043 ns | 0.0011 ns | 0.0010 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| FluentResults_CachedError              | CachedError | 26.8299 ns | 0.2186 ns | 0.2045 ns | 10.29 |    0.08 | 0.0072 | 0.0024 |     120 B |          NA |
| NativeUnion_CachedError                | CachedError |  2.6054 ns | 0.0023 ns | 0.0020 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
|                                        |             |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_Typed_NewError        | NewError    |  7.4552 ns | 0.0504 ns | 0.0472 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| CSharpEssentials_Result_NewError       | NewError    |  8.2217 ns | 0.0626 ns | 0.0586 ns |  1.10 |    0.01 | 0.0033 | 0.0007 |      56 B |        1.40 |
| CSharpEssentials_Any_NewError          | NewError    |  7.4207 ns | 0.0482 ns | 0.0451 ns |  1.00 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| OneOf_NewError                         | NewError    |  7.6696 ns | 0.0613 ns | 0.0574 ns |  1.03 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| Dunet_NewError                         | NewError    | 17.4504 ns | 0.1081 ns | 0.1011 ns |  2.34 |    0.02 | 0.0038 | 0.0009 |      64 B |        1.60 |
| ErrorOr_NewError                       | NewError    | 14.1636 ns | 0.0948 ns | 0.0887 ns |  1.90 |    0.02 | 0.0053 | 0.0017 |      88 B |        2.20 |
| CSharpFunctionalExtensions_NewError    | NewError    |  7.6867 ns | 0.0288 ns | 0.0255 ns |  1.03 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
| FluentResults_NewError                 | NewError    | 50.4015 ns | 0.3631 ns | 0.3396 ns |  6.76 |    0.06 | 0.0162 | 0.0162 |     272 B |        6.80 |
| NativeUnion_NewError                   | NewError    |  7.3795 ns | 0.0491 ns | 0.0459 ns |  0.99 |    0.01 | 0.0024 | 0.0003 |      40 B |        1.00 |
|                                        |             |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_Typed_Ok              | Ok          |  0.5732 ns | 0.0033 ns | 0.0031 ns |  1.00 |    0.01 |      - |      - |         - |          NA |
| CSharpEssentials_Result_Ok             | Ok          |  0.2997 ns | 0.0003 ns | 0.0003 ns |  0.52 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_Any_Ok                | Ok          |  6.6517 ns | 0.0321 ns | 0.0301 ns | 11.60 |    0.08 | 0.0014 | 0.0001 |      24 B |          NA |
| OneOf_Ok                               | Ok          |  0.7397 ns | 0.0003 ns | 0.0003 ns |  1.29 |    0.01 |      - |      - |         - |          NA |
| Dunet_Ok                               | Ok          |  9.5440 ns | 0.0472 ns | 0.0394 ns | 16.65 |    0.11 | 0.0014 | 0.0001 |      24 B |          NA |
| ErrorOr_Ok                             | Ok          |  0.3003 ns | 0.0006 ns | 0.0006 ns |  0.52 |    0.00 |      - |      - |         - |          NA |
| CSharpFunctionalExtensions_Ok          | Ok          |  0.5610 ns | 0.0010 ns | 0.0009 ns |  0.98 |    0.01 |      - |      - |         - |          NA |
| FluentResults_Ok                       | Ok          | 22.8245 ns | 0.0945 ns | 0.0884 ns | 39.82 |    0.26 | 0.0067 | 0.0016 |     112 B |          NA |
| NativeUnion_Ok                         | Ok          |  6.6133 ns | 0.0230 ns | 0.0215 ns | 11.54 |    0.07 | 0.0014 | 0.0001 |      24 B |          NA |
