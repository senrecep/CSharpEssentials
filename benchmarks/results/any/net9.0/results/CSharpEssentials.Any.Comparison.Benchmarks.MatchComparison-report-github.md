```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                  | Categories | Branch | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------- |------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Switch** | **Switch**     | **Int**    | **2.070 ns** | **0.0030 ns** | **0.0027 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Int    | 1.367 ns | 0.0065 ns | 0.0058 ns |  0.66 |    0.00 |         - |          NA |
| Dunet_Switch            | Switch     | Int    | 1.355 ns | 0.0094 ns | 0.0088 ns |  0.65 |    0.00 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_Switch** | **Switch**     | **Text**   | **1.938 ns** | **0.0013 ns** | **0.0010 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Text   | 1.490 ns | 0.0045 ns | 0.0040 ns |  0.77 |    0.00 |         - |          NA |
| Dunet_Switch            | Switch     | Text   | 1.504 ns | 0.0045 ns | 0.0037 ns |  0.78 |    0.00 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Int**    | **1.932 ns** | **0.0010 ns** | **0.0010 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Int    | 1.332 ns | 0.0467 ns | 0.0437 ns |  0.69 |    0.02 |         - |          NA |
| Dunet_ToInt             | ToInt      | Int    | 3.851 ns | 0.0091 ns | 0.0071 ns |  1.99 |    0.00 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Text**   | **3.330 ns** | **0.0138 ns** | **0.0122 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Text   | 1.246 ns | 0.0085 ns | 0.0071 ns |  0.37 |    0.00 |         - |          NA |
| Dunet_ToInt             | ToInt      | Text   | 4.409 ns | 0.0045 ns | 0.0040 ns |  1.32 |    0.00 |         - |          NA |
