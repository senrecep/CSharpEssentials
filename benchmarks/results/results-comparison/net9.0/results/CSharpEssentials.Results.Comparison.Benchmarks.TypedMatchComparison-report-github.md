```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |  **3.9563 ns** | **0.0048 ns** | **0.0045 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |  1.6458 ns | 0.0040 ns | 0.0033 ns |  0.42 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |  9.2109 ns | 0.0480 ns | 0.0449 ns |  2.33 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |  1.9324 ns | 0.0024 ns | 0.0020 ns |  0.49 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |  4.7559 ns | 0.0074 ns | 0.0070 ns |  1.20 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |  2.1842 ns | 0.0048 ns | 0.0042 ns |  0.55 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |  3.0639 ns | 0.0058 ns | 0.0054 ns |  0.77 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 96.8133 ns | 0.4379 ns | 0.3882 ns | 24.47 |    0.10 | 0.0157 |     264 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |  **3.3753 ns** | **0.0016 ns** | **0.0013 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |  1.2193 ns | 0.0074 ns | 0.0069 ns |  0.36 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |  9.2034 ns | 0.0315 ns | 0.0295 ns |  2.73 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |  1.8985 ns | 0.0042 ns | 0.0037 ns |  0.56 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |  4.5388 ns | 0.0059 ns | 0.0055 ns |  1.34 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |  1.0838 ns | 0.0035 ns | 0.0033 ns |  0.32 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |  2.2854 ns | 0.0042 ns | 0.0038 ns |  0.68 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    | 40.2779 ns | 0.1982 ns | 0.1854 ns | 11.93 |    0.05 | 0.0057 |      96 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |  **3.9627 ns** | **0.0092 ns** | **0.0086 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |  1.4235 ns | 0.0023 ns | 0.0019 ns |  0.36 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |  9.1949 ns | 0.0145 ns | 0.0135 ns |  2.32 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |  2.2150 ns | 0.0022 ns | 0.0017 ns |  0.56 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |  4.5340 ns | 0.0041 ns | 0.0034 ns |  1.14 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |  1.6445 ns | 0.0019 ns | 0.0018 ns |  0.42 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |  6.5556 ns | 0.0056 ns | 0.0047 ns |  1.65 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 97.3899 ns | 0.5222 ns | 0.4884 ns | 24.58 |    0.13 | 0.0157 |     264 B |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |  **1.4224 ns** | **0.0078 ns** | **0.0073 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |  0.7396 ns | 0.0013 ns | 0.0011 ns |  0.52 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |  2.0804 ns | 0.0089 ns | 0.0083 ns |  1.46 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |  0.4864 ns | 0.0018 ns | 0.0017 ns |  0.34 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |  **1.0403 ns** | **0.0064 ns** | **0.0057 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |  0.4861 ns | 0.0010 ns | 0.0008 ns |  0.47 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |  6.3622 ns | 0.0207 ns | 0.0173 ns |  6.12 |    0.04 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |  0.0584 ns | 0.0053 ns | 0.0047 ns |  0.06 |    0.00 |      - |         - |          NA |
|                                    |            |            |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |  **1.3580 ns** | **0.0055 ns** | **0.0052 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |  0.7396 ns | 0.0013 ns | 0.0011 ns |  0.54 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |  2.8177 ns | 0.0039 ns | 0.0034 ns |  2.08 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |  0.6004 ns | 0.0041 ns | 0.0038 ns |  0.44 |    0.00 |      - |         - |          NA |
