```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |   **4.2091 ns** | **0.0048 ns** | **0.0043 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |   1.6321 ns | 0.0095 ns | 0.0089 ns |  0.39 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |   9.5105 ns | 0.0162 ns | 0.0151 ns |  2.26 |    0.00 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |   2.4740 ns | 0.0032 ns | 0.0025 ns |  0.59 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |   5.2476 ns | 0.0093 ns | 0.0078 ns |  1.25 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |   1.6584 ns | 0.0089 ns | 0.0079 ns |  0.39 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |   3.1136 ns | 0.0081 ns | 0.0076 ns |  0.74 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 130.1149 ns | 0.4994 ns | 0.4427 ns | 30.91 |    0.11 | 0.0181 |     304 B |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |   **3.3576 ns** | **0.0070 ns** | **0.0062 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |   1.0501 ns | 0.0043 ns | 0.0040 ns |  0.31 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |   9.6072 ns | 0.0171 ns | 0.0160 ns |  2.86 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |   1.6266 ns | 0.0033 ns | 0.0029 ns |  0.48 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |   5.4336 ns | 0.0120 ns | 0.0112 ns |  1.62 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |   1.0520 ns | 0.0044 ns | 0.0041 ns |  0.31 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |   2.3232 ns | 0.0084 ns | 0.0075 ns |  0.69 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    |  55.5031 ns | 0.2147 ns | 0.2008 ns | 16.53 |    0.07 | 0.0067 |     112 B |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |   **4.2075 ns** | **0.0063 ns** | **0.0056 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |   1.7714 ns | 0.0053 ns | 0.0050 ns |  0.42 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |   9.5190 ns | 0.0181 ns | 0.0169 ns |  2.26 |    0.00 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |   2.7615 ns | 0.0031 ns | 0.0026 ns |  0.66 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |   4.9016 ns | 0.0072 ns | 0.0068 ns |  1.16 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |   1.9032 ns | 0.0098 ns | 0.0092 ns |  0.45 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |   5.7127 ns | 0.0083 ns | 0.0077 ns |  1.36 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 127.1941 ns | 0.2992 ns | 0.2798 ns | 30.23 |    0.08 | 0.0181 |     304 B |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |   **1.2908 ns** | **0.0075 ns** | **0.0067 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |   0.7583 ns | 0.0013 ns | 0.0011 ns |  0.59 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |   2.9135 ns | 0.0107 ns | 0.0100 ns |  2.26 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |   0.4699 ns | 0.0012 ns | 0.0012 ns |  0.36 |    0.00 |      - |         - |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |   **1.0489 ns** | **0.0025 ns** | **0.0022 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |   0.4708 ns | 0.0016 ns | 0.0014 ns |  0.45 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |   6.6654 ns | 0.0365 ns | 0.0341 ns |  6.35 |    0.03 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |   0.3247 ns | 0.0021 ns | 0.0020 ns |  0.31 |    0.00 |      - |         - |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |   **1.3382 ns** | **0.0038 ns** | **0.0033 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |   0.2911 ns | 0.0032 ns | 0.0030 ns |  0.22 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |   7.7746 ns | 0.1676 ns | 0.1568 ns |  5.81 |    0.11 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |   0.8701 ns | 0.0063 ns | 0.0052 ns |  0.65 |    0.00 |      - |         - |          NA |
