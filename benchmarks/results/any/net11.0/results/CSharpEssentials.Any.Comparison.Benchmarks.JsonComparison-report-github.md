```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | Branch | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Int**    | **444.6 ns** | **2.28 ns** | **2.02 ns** |  **1.00** | **0.0205** |     **344 B** |        **1.00** |
| OneOf_Deserialize            | Deserialize | Int    | 291.6 ns | 1.51 ns | 1.26 ns |  0.66 | 0.0091 |     152 B |        0.44 |
| Dunet_Deserialize            | Deserialize | Int    | 296.0 ns | 0.51 ns | 0.47 ns |  0.67 | 0.0100 |     168 B |        0.49 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Text**   | **464.1 ns** | **0.50 ns** | **0.41 ns** |  **1.00** | **0.0224** |     **376 B** |        **1.00** |
| OneOf_Deserialize            | Deserialize | Text   | 315.6 ns | 0.36 ns | 0.34 ns |  0.68 | 0.0105 |     176 B |        0.47 |
| Dunet_Deserialize            | Deserialize | Text   | 294.1 ns | 5.55 ns | 5.19 ns |  0.63 | 0.0119 |     200 B |        0.53 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Int**    | **214.9 ns** | **0.70 ns** | **0.65 ns** |  **1.00** | **0.0062** |     **104 B** |        **1.00** |
| OneOf_Serialize              | Serialize   | Int    | 145.5 ns | 0.22 ns | 0.17 ns |  0.68 | 0.0052 |      88 B |        0.85 |
| Dunet_Serialize              | Serialize   | Int    | 206.9 ns | 0.85 ns | 0.75 ns |  0.96 | 0.0234 |     392 B |        3.77 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Text**   | **208.9 ns** | **0.29 ns** | **0.27 ns** |  **1.00** | **0.0072** |     **120 B** |        **1.00** |
| OneOf_Serialize              | Serialize   | Text   | 157.7 ns | 0.20 ns | 0.19 ns |  0.75 | 0.0067 |     112 B |        0.93 |
| Dunet_Serialize              | Serialize   | Text   | 221.9 ns | 1.71 ns | 1.51 ns |  1.06 | 0.0243 |     408 B |        3.40 |
