```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |  **2.2075 ns** | **0.0079 ns** | **0.0074 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |  1.6273 ns | 0.0032 ns | 0.0028 ns |  0.74 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |  5.3799 ns | 0.0049 ns | 0.0044 ns |  2.44 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |  1.9135 ns | 0.0017 ns | 0.0014 ns |  0.87 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |  5.8492 ns | 0.0103 ns | 0.0091 ns |  2.65 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |  1.6300 ns | 0.0036 ns | 0.0034 ns |  0.74 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |  2.4964 ns | 0.0045 ns | 0.0042 ns |  1.13 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 76.8401 ns | 0.3645 ns | 0.3231 ns | 34.81 |    0.18 | 0.0143 |     240 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |  **1.6705 ns** | **0.0142 ns** | **0.0133 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |  1.2330 ns | 0.0062 ns | 0.0058 ns |  0.74 |    0.01 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |  5.3604 ns | 0.0037 ns | 0.0033 ns |  3.21 |    0.02 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |  1.6736 ns | 0.0110 ns | 0.0103 ns |  1.00 |    0.01 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |  5.7838 ns | 0.0268 ns | 0.0251 ns |  3.46 |    0.03 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |  1.0912 ns | 0.0057 ns | 0.0050 ns |  0.65 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |  2.3267 ns | 0.0051 ns | 0.0039 ns |  1.39 |    0.01 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    | 21.2345 ns | 0.1419 ns | 0.1328 ns | 12.71 |    0.12 | 0.0057 |      96 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |  **1.9183 ns** | **0.0041 ns** | **0.0034 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |  1.6367 ns | 0.0094 ns | 0.0088 ns |  0.85 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |  5.3857 ns | 0.0054 ns | 0.0045 ns |  2.81 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |  2.2038 ns | 0.0034 ns | 0.0030 ns |  1.15 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |  5.8558 ns | 0.0044 ns | 0.0041 ns |  3.05 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |  1.6944 ns | 0.0037 ns | 0.0033 ns |  0.88 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |  3.0724 ns | 0.0031 ns | 0.0026 ns |  1.60 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 82.5593 ns | 0.2373 ns | 0.2219 ns | 43.04 |    0.13 | 0.0143 |     240 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |  **1.4182 ns** | **0.0023 ns** | **0.0021 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |  0.7441 ns | 0.0031 ns | 0.0029 ns |  0.52 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |  1.9202 ns | 0.0027 ns | 0.0024 ns |  1.35 |    0.00 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |  1.1594 ns | 0.0050 ns | 0.0047 ns |  0.82 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |  **1.0406 ns** | **0.0060 ns** | **0.0056 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |  0.4716 ns | 0.0018 ns | 0.0016 ns |  0.45 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |  5.8368 ns | 0.0328 ns | 0.0307 ns |  5.61 |    0.04 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |  0.2998 ns | 0.0035 ns | 0.0027 ns |  0.29 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |  **1.3382 ns** | **0.0033 ns** | **0.0029 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |  0.2554 ns | 0.0046 ns | 0.0043 ns |  0.19 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |  3.0737 ns | 0.0046 ns | 0.0036 ns |  2.30 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |  0.8724 ns | 0.0043 ns | 0.0038 ns |  0.65 |    0.00 |      - |         - |          NA |
