```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.3181 ns** | **0.0032 ns** | **0.0030 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |  1.3223 ns | 0.0028 ns | 0.0026 ns |   1.00 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3194 ns | 0.0076 ns | 0.0071 ns |   1.76 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9852 ns | 0.0055 ns | 0.0051 ns |   0.75 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 32.4655 ns | 0.1323 ns | 0.1173 ns |  24.63 |    0.10 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.2199 ns** | **0.0046 ns** | **0.0043 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |  1.2241 ns | 0.0063 ns | 0.0056 ns |   1.00 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.2916 ns | 0.0062 ns | 0.0055 ns |   1.88 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0928 ns | 0.0052 ns | 0.0049 ns |   0.90 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 40.5911 ns | 0.2580 ns | 0.2414 ns |  33.27 |    0.22 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **0.8828 ns** | **0.0029 ns** | **0.0024 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |  0.9471 ns | 0.0032 ns | 0.0030 ns |   1.07 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.3185 ns | 0.0034 ns | 0.0032 ns |   1.49 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  0.8837 ns | 0.0053 ns | 0.0047 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 99.1352 ns | 0.4911 ns | 0.4594 ns | 112.30 |    0.58 | 0.0157 |     264 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **1.7414 ns** | **0.0090 ns** | **0.0084 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |  1.7337 ns | 0.0111 ns | 0.0104 ns |   1.00 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.3154 ns | 0.0062 ns | 0.0052 ns |   1.33 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  1.4462 ns | 0.0049 ns | 0.0043 ns |   0.83 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 42.1963 ns | 0.2499 ns | 0.2338 ns |  24.23 |    0.17 | 0.0057 |      96 B |          NA |
