```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | Branch | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Int**    | **463.5 ns** | **2.20 ns** | **1.83 ns** |  **1.00** | **0.0205** |     **344 B** |        **1.00** |
| OneOf_Deserialize            | Deserialize | Int    | 308.2 ns | 0.70 ns | 0.65 ns |  0.67 | 0.0091 |     152 B |        0.44 |
| Dunet_Deserialize            | Deserialize | Int    | 292.1 ns | 0.44 ns | 0.39 ns |  0.63 | 0.0100 |     168 B |        0.49 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Text**   | **455.0 ns** | **1.11 ns** | **1.04 ns** |  **1.00** | **0.0224** |     **376 B** |        **1.00** |
| OneOf_Deserialize            | Deserialize | Text   | 309.6 ns | 0.37 ns | 0.29 ns |  0.68 | 0.0105 |     176 B |        0.47 |
| Dunet_Deserialize            | Deserialize | Text   | 307.3 ns | 0.51 ns | 0.42 ns |  0.68 | 0.0119 |     200 B |        0.53 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Int**    | **215.4 ns** | **1.35 ns** | **1.20 ns** |  **1.00** | **0.0062** |     **104 B** |        **1.00** |
| OneOf_Serialize              | Serialize   | Int    | 159.4 ns | 0.32 ns | 0.28 ns |  0.74 | 0.0052 |      88 B |        0.85 |
| Dunet_Serialize              | Serialize   | Int    | 225.8 ns | 0.82 ns | 0.73 ns |  1.05 | 0.0234 |     392 B |        3.77 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Text**   | **235.0 ns** | **2.12 ns** | **1.98 ns** |  **1.00** | **0.0072** |     **120 B** |        **1.00** |
| OneOf_Serialize              | Serialize   | Text   | 174.2 ns | 0.24 ns | 0.23 ns |  0.74 | 0.0067 |     112 B |        0.93 |
| Dunet_Serialize              | Serialize   | Text   | 240.6 ns | 0.71 ns | 0.63 ns |  1.02 | 0.0243 |     408 B |        3.40 |
