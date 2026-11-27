```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |  **2.2372 ns** | **0.0199 ns** | **0.0186 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |  1.6452 ns | 0.0034 ns | 0.0028 ns |  0.74 |    0.01 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |  9.2137 ns | 0.0307 ns | 0.0272 ns |  4.12 |    0.04 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |  1.9370 ns | 0.0081 ns | 0.0075 ns |  0.87 |    0.01 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |  4.5544 ns | 0.0061 ns | 0.0051 ns |  2.04 |    0.02 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |  2.1840 ns | 0.0013 ns | 0.0012 ns |  0.98 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |  3.0712 ns | 0.0159 ns | 0.0149 ns |  1.37 |    0.01 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 98.3270 ns | 1.0001 ns | 0.9355 ns | 43.95 |    0.54 | 0.0157 |     264 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |  **2.2287 ns** | **0.0142 ns** | **0.0126 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |  1.2227 ns | 0.0090 ns | 0.0084 ns |  0.55 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |  9.2109 ns | 0.0113 ns | 0.0100 ns |  4.13 |    0.02 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |  1.8974 ns | 0.0044 ns | 0.0039 ns |  0.85 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |  4.5354 ns | 0.0072 ns | 0.0060 ns |  2.04 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |  1.0934 ns | 0.0110 ns | 0.0092 ns |  0.49 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |  2.3214 ns | 0.0079 ns | 0.0074 ns |  1.04 |    0.01 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    | 41.0749 ns | 0.3324 ns | 0.3109 ns | 18.43 |    0.17 | 0.0057 |      96 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |  **2.2188 ns** | **0.0011 ns** | **0.0010 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |  1.6421 ns | 0.0032 ns | 0.0028 ns |  0.74 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |  9.1981 ns | 0.0718 ns | 0.0636 ns |  4.15 |    0.03 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |  2.2252 ns | 0.0080 ns | 0.0071 ns |  1.00 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |  4.6307 ns | 0.0102 ns | 0.0090 ns |  2.09 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |  1.7653 ns | 0.0062 ns | 0.0058 ns |  0.80 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |  6.5785 ns | 0.0131 ns | 0.0116 ns |  2.96 |    0.01 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 97.4342 ns | 0.3218 ns | 0.2853 ns | 43.91 |    0.13 | 0.0157 |     264 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |  **1.5153 ns** | **0.0042 ns** | **0.0039 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |  0.7451 ns | 0.0051 ns | 0.0048 ns |  0.49 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |  1.9250 ns | 0.0067 ns | 0.0063 ns |  1.27 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |  0.5953 ns | 0.0018 ns | 0.0017 ns |  0.39 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |  **0.9035 ns** | **0.0025 ns** | **0.0023 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |  0.4893 ns | 0.0013 ns | 0.0012 ns |  0.54 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |  6.3248 ns | 0.0228 ns | 0.0191 ns |  7.00 |    0.03 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |  0.0512 ns | 0.0080 ns | 0.0071 ns |  0.06 |    0.01 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |  **1.3570 ns** | **0.0070 ns** | **0.0065 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |  0.7426 ns | 0.0031 ns | 0.0028 ns |  0.55 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |  2.8142 ns | 0.0056 ns | 0.0047 ns |  2.07 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |  0.5994 ns | 0.0043 ns | 0.0040 ns |  0.44 |    0.00 |      - |         - |          NA |
