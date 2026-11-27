```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |  **2.2213 ns** | **0.0027 ns** | **0.0022 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |  1.8612 ns | 0.0072 ns | 0.0061 ns |  0.84 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |  9.1764 ns | 0.0320 ns | 0.0284 ns |  4.13 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |  1.9318 ns | 0.0020 ns | 0.0019 ns |  0.87 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |  4.7606 ns | 0.0059 ns | 0.0052 ns |  2.14 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |  2.1850 ns | 0.0025 ns | 0.0024 ns |  0.98 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |  3.0567 ns | 0.0039 ns | 0.0036 ns |  1.38 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 99.4548 ns | 0.6477 ns | 0.6058 ns | 44.77 |    0.27 | 0.0157 |     264 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |  **2.2230 ns** | **0.0054 ns** | **0.0051 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |  1.2114 ns | 0.0052 ns | 0.0048 ns |  0.54 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |  9.2050 ns | 0.0205 ns | 0.0192 ns |  4.14 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |  1.8925 ns | 0.0016 ns | 0.0015 ns |  0.85 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |  4.5668 ns | 0.0302 ns | 0.0283 ns |  2.05 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |  1.1758 ns | 0.0064 ns | 0.0060 ns |  0.53 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |  2.3219 ns | 0.0094 ns | 0.0088 ns |  1.04 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    | 41.3369 ns | 0.3890 ns | 0.3448 ns | 18.59 |    0.16 | 0.0057 |      96 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |  **3.4858 ns** | **0.0111 ns** | **0.0098 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |  1.6452 ns | 0.0032 ns | 0.0030 ns |  0.47 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |  9.1843 ns | 0.0413 ns | 0.0387 ns |  2.63 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |  2.2232 ns | 0.0045 ns | 0.0042 ns |  0.64 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |  4.5535 ns | 0.0063 ns | 0.0059 ns |  1.31 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |  1.6443 ns | 0.0037 ns | 0.0034 ns |  0.47 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |  6.5671 ns | 0.0141 ns | 0.0132 ns |  1.88 |    0.01 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 98.9613 ns | 0.2269 ns | 0.2122 ns | 28.39 |    0.10 | 0.0157 |     264 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |  **1.5082 ns** | **0.0025 ns** | **0.0021 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |  0.7399 ns | 0.0012 ns | 0.0012 ns |  0.49 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |  2.0807 ns | 0.0037 ns | 0.0031 ns |  1.38 |    0.00 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |  0.4872 ns | 0.0027 ns | 0.0024 ns |  0.32 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |  **1.0048 ns** | **0.0054 ns** | **0.0051 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |  0.4902 ns | 0.0044 ns | 0.0041 ns |  0.49 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |  6.4137 ns | 0.0605 ns | 0.0566 ns |  6.38 |    0.06 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |  0.0654 ns | 0.0062 ns | 0.0055 ns |  0.07 |    0.01 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |  **1.3571 ns** | **0.0054 ns** | **0.0051 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |  0.7405 ns | 0.0016 ns | 0.0015 ns |  0.55 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |  2.8208 ns | 0.0099 ns | 0.0088 ns |  2.08 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |  0.5955 ns | 0.0021 ns | 0.0018 ns |  0.44 |    0.00 |      - |         - |          NA |
