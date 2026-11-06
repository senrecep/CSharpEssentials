```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                  | Categories | Branch | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------- |------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Switch** | **Switch**     | **Int**    | **1.691 ns** | **0.0029 ns** | **0.0025 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Int    | 1.439 ns | 0.0039 ns | 0.0036 ns |  0.85 |    0.00 |         - |          NA |
| Dunet_Switch            | Switch     | Int    | 1.341 ns | 0.0036 ns | 0.0034 ns |  0.79 |    0.00 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_Switch** | **Switch**     | **Text**   | **1.977 ns** | **0.0012 ns** | **0.0010 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Text   | 1.642 ns | 0.0009 ns | 0.0008 ns |  0.83 |    0.00 |         - |          NA |
| Dunet_Switch            | Switch     | Text   | 1.512 ns | 0.0058 ns | 0.0052 ns |  0.76 |    0.00 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Int**    | **1.714 ns** | **0.0101 ns** | **0.0095 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Int    | 1.065 ns | 0.0008 ns | 0.0007 ns |  0.62 |    0.00 |         - |          NA |
| Dunet_ToInt             | ToInt      | Int    | 4.455 ns | 0.0117 ns | 0.0110 ns |  2.60 |    0.02 |         - |          NA |
|                         |            |        |          |           |           |       |         |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Text**   | **1.660 ns** | **0.0008 ns** | **0.0007 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Text   | 1.371 ns | 0.0004 ns | 0.0003 ns |  0.83 |    0.00 |         - |          NA |
| Dunet_ToInt             | ToInt      | Text   | 4.071 ns | 0.0177 ns | 0.0165 ns |  2.45 |    0.01 |         - |          NA |
