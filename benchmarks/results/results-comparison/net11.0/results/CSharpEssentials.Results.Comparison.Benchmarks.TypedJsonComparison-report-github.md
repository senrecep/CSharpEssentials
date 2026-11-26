```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                             | Categories  | Case     | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |------------ |--------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Deserialize** | **Deserialize** | **NotFound** | **997.2 ns** | **3.50 ns** | **3.28 ns** |  **1.00** | **0.0191** |     **320 B** |        **1.00** |
| OneOf_Deserialize                  | Deserialize | NotFound | 849.1 ns | 2.61 ns | 2.18 ns |  0.85 | 0.0229 |     432 B |        1.35 |
| Dunet_Deserialize                  | Deserialize | NotFound | 749.2 ns | 2.97 ns | 2.63 ns |  0.75 | 0.0544 |     912 B |        2.85 |
|                                    |             |          |          |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Deserialize** | **Deserialize** | **Success**  | **279.4 ns** | **0.66 ns** | **0.62 ns** |  **1.00** | **0.0024** |      **40 B** |        **1.00** |
| OneOf_Deserialize                  | Deserialize | Success  | 310.2 ns | 0.42 ns | 0.37 ns |  1.11 | 0.0100 |     168 B |        4.20 |
| Dunet_Deserialize                  | Deserialize | Success  | 296.6 ns | 0.63 ns | 0.56 ns |  1.06 | 0.0095 |     160 B |        4.00 |
|                                    |             |          |          |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Serialize**   | **Serialize**   | **NotFound** | **378.9 ns** | **1.28 ns** | **1.13 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| OneOf_Serialize                    | Serialize   | NotFound | 356.4 ns | 0.99 ns | 0.93 ns |  0.94 | 0.0186 |     312 B |        1.03 |
| Dunet_Serialize                    | Serialize   | NotFound | 402.2 ns | 1.21 ns | 1.07 ns |  1.06 | 0.0358 |     600 B |        1.97 |
|                                    |             |          |          |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Serialize**   | **Serialize**   | **Success**  | **164.8 ns** | **0.41 ns** | **0.38 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| OneOf_Serialize                    | Serialize   | Success  | 159.1 ns | 0.56 ns | 0.53 ns |  0.96 | 0.0052 |      88 B |        1.10 |
| Dunet_Serialize                    | Serialize   | Success  | 211.7 ns | 0.74 ns | 0.69 ns |  1.28 | 0.0229 |     384 B |        4.80 |
