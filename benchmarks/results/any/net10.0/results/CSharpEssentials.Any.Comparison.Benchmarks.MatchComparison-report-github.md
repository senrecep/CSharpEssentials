```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Categories | Branch | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------- |------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Switch** | **Switch**     | **Int**    | **1.708 ns** | **0.0105 ns** | **0.0093 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Int    | 1.521 ns | 0.0065 ns | 0.0058 ns |  0.89 |    0.01 |         - |          NA |
| Dunet_Switch            | Switch     | Int    | 1.406 ns | 0.0420 ns | 0.0392 ns |  0.82 |    0.02 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_Switch** | **Switch**     | **Text**   | **1.649 ns** | **0.0013 ns** | **0.0013 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Text   | 1.958 ns | 0.0235 ns | 0.0220 ns |  1.19 |    0.01 |         - |          NA |
| Dunet_Switch            | Switch     | Text   | 1.552 ns | 0.0174 ns | 0.0163 ns |  0.94 |    0.01 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Int**    | **1.551 ns** | **0.0183 ns** | **0.0172 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Int    | 1.124 ns | 0.0196 ns | 0.0183 ns |  0.72 |    0.01 |         - |          NA |
| Dunet_ToInt             | ToInt      | Int    | 5.093 ns | 0.0098 ns | 0.0087 ns |  3.28 |    0.04 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Text**   | **1.370 ns** | **0.0079 ns** | **0.0070 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Text   | 1.239 ns | 0.0039 ns | 0.0037 ns |  0.90 |    0.01 |         - |          NA |
| Dunet_ToInt             | ToInt      | Text   | 5.175 ns | 0.0008 ns | 0.0007 ns |  3.78 |    0.02 |         - |          NA |
