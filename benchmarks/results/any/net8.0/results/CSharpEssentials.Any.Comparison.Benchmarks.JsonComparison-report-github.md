```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Categories  | Branch | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Int**    | **512.4 ns** | **1.20 ns** | **1.12 ns** |  **1.00** | **0.0219** |     **376 B** |        **1.00** |
| OneOf_Deserialize            | Deserialize | Int    | 394.7 ns | 1.07 ns | 0.94 ns |  0.77 | 0.0129 |     216 B |        0.57 |
| Dunet_Deserialize            | Deserialize | Int    | 316.2 ns | 0.45 ns | 0.40 ns |  0.62 | 0.0100 |     168 B |        0.45 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Text**   | **517.6 ns** | **1.01 ns** | **0.84 ns** |  **1.00** | **0.0238** |     **408 B** |        **1.00** |
| OneOf_Deserialize            | Deserialize | Text   | 406.6 ns | 0.90 ns | 0.75 ns |  0.79 | 0.0143 |     240 B |        0.59 |
| Dunet_Deserialize            | Deserialize | Text   | 338.2 ns | 0.68 ns | 0.60 ns |  0.65 | 0.0119 |     200 B |        0.49 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Int**    | **253.6 ns** | **0.44 ns** | **0.39 ns** |  **1.00** | **0.0062** |     **104 B** |        **1.00** |
| OneOf_Serialize              | Serialize   | Int    | 245.7 ns | 0.29 ns | 0.27 ns |  0.97 | 0.0129 |     216 B |        2.08 |
| Dunet_Serialize              | Serialize   | Int    | 265.2 ns | 1.03 ns | 0.92 ns |  1.05 | 0.0234 |     392 B |        3.77 |
|                              |             |        |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Text**   | **269.3 ns** | **0.23 ns** | **0.20 ns** |  **1.00** | **0.0072** |     **120 B** |        **1.00** |
| OneOf_Serialize              | Serialize   | Text   | 283.7 ns | 0.50 ns | 0.46 ns |  1.05 | 0.0143 |     240 B |        2.00 |
| Dunet_Serialize              | Serialize   | Text   | 282.0 ns | 1.02 ns | 0.95 ns |  1.05 | 0.0243 |     408 B |        3.40 |
