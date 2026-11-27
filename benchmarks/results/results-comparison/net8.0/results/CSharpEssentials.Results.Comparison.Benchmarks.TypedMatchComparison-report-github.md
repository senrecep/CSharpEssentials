```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Categories | Case       | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |----------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Match**       | **Match**      | **NotFound**   |   **2.4911 ns** | **0.0012 ns** | **0.0010 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | NotFound   |   1.6253 ns | 0.0014 ns | 0.0011 ns |  0.65 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | NotFound   |   9.4724 ns | 0.0035 ns | 0.0029 ns |  3.80 |    0.00 |      - |         - |          NA |
| OneOf_Match                        | Match      | NotFound   |   2.4732 ns | 0.0022 ns | 0.0019 ns |  0.99 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | NotFound   |   4.9915 ns | 0.0035 ns | 0.0031 ns |  2.00 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | NotFound   |   1.6611 ns | 0.0113 ns | 0.0095 ns |  0.67 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | NotFound   |   3.1082 ns | 0.0055 ns | 0.0049 ns |  1.25 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | NotFound   | 130.3059 ns | 1.4893 ns | 1.3931 ns | 52.31 |    0.54 | 0.0181 |     304 B |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Success**    |   **1.9175 ns** | **0.0031 ns** | **0.0029 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Success    |   1.0463 ns | 0.0016 ns | 0.0014 ns |  0.55 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Success    |   9.5800 ns | 0.0173 ns | 0.0135 ns |  5.00 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Success    |   1.6252 ns | 0.0018 ns | 0.0016 ns |  0.85 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Success    |   5.2500 ns | 0.0069 ns | 0.0061 ns |  2.74 |    0.01 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Success    |   1.0466 ns | 0.0035 ns | 0.0031 ns |  0.55 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Success    |   2.2892 ns | 0.0058 ns | 0.0054 ns |  1.19 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Success    |  55.8067 ns | 0.1525 ns | 0.1352 ns | 29.10 |    0.08 | 0.0067 |     112 B |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Match**       | **Match**      | **Validation** |   **2.4928 ns** | **0.0036 ns** | **0.0034 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result_Match      | Match      | Validation |   1.6230 ns | 0.0010 ns | 0.0009 ns |  0.65 |    0.00 |      - |         - |          NA |
| CSharpEssentials_Any_Match         | Match      | Validation |   9.5221 ns | 0.0210 ns | 0.0197 ns |  3.82 |    0.01 |      - |         - |          NA |
| OneOf_Match                        | Match      | Validation |   2.7627 ns | 0.0033 ns | 0.0029 ns |  1.11 |    0.00 |      - |         - |          NA |
| Dunet_Match                        | Match      | Validation |   4.9057 ns | 0.0087 ns | 0.0082 ns |  1.97 |    0.00 |      - |         - |          NA |
| ErrorOr_Match                      | Match      | Validation |   1.6589 ns | 0.0052 ns | 0.0043 ns |  0.67 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_Match   | Match      | Validation |   5.7004 ns | 0.0052 ns | 0.0043 ns |  2.29 |    0.00 |      - |         - |          NA |
| FluentResults_Match                | Match      | Validation | 127.9084 ns | 0.5982 ns | 0.5596 ns | 51.31 |    0.23 | 0.0181 |     304 B |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **NotFound**   |   **1.2744 ns** | **0.0049 ns** | **0.0043 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | NotFound   |   0.7624 ns | 0.0035 ns | 0.0028 ns |  0.60 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | NotFound   |   2.9029 ns | 0.0083 ns | 0.0078 ns |  2.28 |    0.01 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | NotFound   |   0.4711 ns | 0.0014 ns | 0.0011 ns |  0.37 |    0.00 |      - |         - |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Success**    |   **1.0622 ns** | **0.0081 ns** | **0.0076 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Success    |   0.4722 ns | 0.0037 ns | 0.0035 ns |  0.44 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Success    |   6.7701 ns | 0.0427 ns | 0.0399 ns |  6.37 |    0.06 | 0.0014 |      24 B |          NA |
| Dunet_Switch                       | Switch     | Success    |   0.5210 ns | 0.0039 ns | 0.0036 ns |  0.49 |    0.00 |      - |         - |          NA |
|                                    |            |            |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Switch**      | **Switch**     | **Validation** |   **1.3389 ns** | **0.0067 ns** | **0.0052 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_TryGetValue | Switch     | Validation |   0.2896 ns | 0.0013 ns | 0.0011 ns |  0.22 |    0.00 |      - |         - |          NA |
| OneOf_Switch                       | Switch     | Validation |   7.7548 ns | 0.1700 ns | 0.1590 ns |  5.79 |    0.12 |      - |         - |          NA |
| Dunet_Switch                       | Switch     | Validation |   0.9028 ns | 0.0164 ns | 0.0154 ns |  0.67 |    0.01 |      - |         - |          NA |
