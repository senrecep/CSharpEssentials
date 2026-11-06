```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Categories | Branch | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------ |----------- |------- |---------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Switch** | **Switch**     | **Int**    | **1.696 ns** | **0.0040 ns** | **0.0035 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Int    | 1.437 ns | 0.0068 ns | 0.0064 ns |  0.85 |         - |          NA |
| Dunet_Switch            | Switch     | Int    | 1.397 ns | 0.0191 ns | 0.0179 ns |  0.82 |         - |          NA |
|                         |            |        |          |           |           |       |           |             |
| **CSharpEssentials_Switch** | **Switch**     | **Text**   | **1.648 ns** | **0.0015 ns** | **0.0012 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_Switch            | Switch     | Text   | 1.445 ns | 0.0174 ns | 0.0154 ns |  0.88 |         - |          NA |
| Dunet_Switch            | Switch     | Text   | 1.493 ns | 0.0098 ns | 0.0087 ns |  0.91 |         - |          NA |
|                         |            |        |          |           |           |       |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Int**    | **1.402 ns** | **0.0062 ns** | **0.0058 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Int    | 1.172 ns | 0.0090 ns | 0.0080 ns |  0.84 |         - |          NA |
| Dunet_ToInt             | ToInt      | Int    | 4.837 ns | 0.0020 ns | 0.0016 ns |  3.45 |         - |          NA |
|                         |            |        |          |           |           |       |           |             |
| **CSharpEssentials_ToInt**  | **ToInt**      | **Text**   | **1.437 ns** | **0.0043 ns** | **0.0038 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ToInt             | ToInt      | Text   | 1.363 ns | 0.0088 ns | 0.0074 ns |  0.95 |         - |          NA |
| Dunet_ToInt             | ToInt      | Text   | 5.077 ns | 0.0010 ns | 0.0008 ns |  3.53 |         - |          NA |
