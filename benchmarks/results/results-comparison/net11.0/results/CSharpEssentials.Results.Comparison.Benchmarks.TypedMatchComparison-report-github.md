```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |  **1.9143 ns** | **0.0025 ns** | **0.0023 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |  1.6289 ns | 0.0022 ns | 0.0018 ns |  0.85 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |  5.6745 ns | 0.0056 ns | 0.0050 ns |  2.96 |    0.00 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |  1.9177 ns | 0.0048 ns | 0.0045 ns |  1.00 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |  6.4163 ns | 0.0228 ns | 0.0213 ns |  3.35 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |  1.6296 ns | 0.0032 ns | 0.0030 ns |  0.85 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |  2.5986 ns | 0.0059 ns | 0.0055 ns |  1.36 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 79.7124 ns | 0.5840 ns | 0.5462 ns | 41.64 |    0.28 | 0.0143 |     240 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |  **1.6470 ns** | **0.0165 ns** | **0.0154 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |  1.2179 ns | 0.0067 ns | 0.0063 ns |  0.74 |    0.01 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |  5.1038 ns | 0.0126 ns | 0.0118 ns |  3.10 |    0.03 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |  1.6656 ns | 0.0113 ns | 0.0106 ns |  1.01 |    0.01 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |  5.6734 ns | 0.0097 ns | 0.0091 ns |  3.45 |    0.03 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |  1.0914 ns | 0.0078 ns | 0.0069 ns |  0.66 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |  2.2820 ns | 0.0031 ns | 0.0026 ns |  1.39 |    0.01 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    | 18.9622 ns | 0.3119 ns | 0.3338 ns | 11.51 |    0.22 | 0.0057 |      96 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |  **2.2563 ns** | **0.0050 ns** | **0.0046 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |  1.6323 ns | 0.0032 ns | 0.0030 ns |  0.72 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |  5.6510 ns | 0.0065 ns | 0.0058 ns |  2.50 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |  2.2016 ns | 0.0021 ns | 0.0018 ns |  0.98 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |  5.7346 ns | 0.0071 ns | 0.0063 ns |  2.54 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |  1.6321 ns | 0.0078 ns | 0.0073 ns |  0.72 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |  3.0695 ns | 0.0022 ns | 0.0021 ns |  1.36 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 75.7489 ns | 0.3034 ns | 0.2838 ns | 33.57 |    0.14 | 0.0143 |     240 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |  **1.4158 ns** | **0.0075 ns** | **0.0070 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |  0.7410 ns | 0.0018 ns | 0.0017 ns |  0.52 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |  2.0588 ns | 0.0073 ns | 0.0068 ns |  1.45 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |  0.8655 ns | 0.0024 ns | 0.0022 ns |  0.61 |    0.00 |      - |         - |          NA |
| NativeUnion_Switch                 | Switch     | NotFound   |  0.5353 ns | 0.0035 ns | 0.0032 ns |  0.38 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |  **0.8171 ns** | **0.0070 ns** | **0.0058 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |  0.4729 ns | 0.0039 ns | 0.0034 ns |  0.58 |    0.01 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |  5.7597 ns | 0.0467 ns | 0.0437 ns |  7.05 |    0.07 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |  0.5804 ns | 0.0023 ns | 0.0020 ns |  0.71 |    0.01 |      - |         - |          NA |
| NativeUnion_Switch                 | Switch     | Success    |  0.5791 ns | 0.0023 ns | 0.0021 ns |  0.71 |    0.01 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |  **1.3399 ns** | **0.0035 ns** | **0.0033 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |  0.2956 ns | 0.0057 ns | 0.0050 ns |  0.22 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |  3.0717 ns | 0.0033 ns | 0.0029 ns |  2.29 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |  1.7367 ns | 0.0026 ns | 0.0022 ns |  1.30 |    0.00 |      - |         - |          NA |
| NativeUnion_Switch                 | Switch     | Validation |  1.3373 ns | 0.0038 ns | 0.0032 ns |  1.00 |    0.00 |      - |         - |          NA |
