```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                             | Categories  | Case     | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |------------ |--------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Deserialize** | **Deserialize** | **NotFound** | **1,068.5 ns** | **3.73 ns** | **3.31 ns** |  **1.00** | **0.0191** |     **320 B** |        **1.00** |
| OneOf_Deserialize                  | Deserialize | NotFound |   898.9 ns | 3.12 ns | 2.92 ns |  0.84 | 0.0257 |     432 B |        1.35 |
| Dunet_Deserialize                  | Deserialize | NotFound |   789.4 ns | 3.09 ns | 2.89 ns |  0.74 | 0.0544 |     912 B |        2.85 |
|                                    |             |          |            |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Deserialize** | **Deserialize** | **Success**  |   **293.4 ns** | **0.45 ns** | **0.40 ns** |  **1.00** | **0.0024** |      **40 B** |        **1.00** |
| OneOf_Deserialize                  | Deserialize | Success  |   314.0 ns | 1.30 ns | 1.21 ns |  1.07 | 0.0100 |     168 B |        4.20 |
| Dunet_Deserialize                  | Deserialize | Success  |   276.1 ns | 0.54 ns | 0.48 ns |  0.94 | 0.0095 |     160 B |        4.00 |
|                                    |             |          |            |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Serialize**   | **Serialize**   | **NotFound** |   **419.5 ns** | **3.31 ns** | **2.94 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| OneOf_Serialize                    | Serialize   | NotFound |   370.9 ns | 0.84 ns | 0.78 ns |  0.88 | 0.0186 |     312 B |        1.03 |
| Dunet_Serialize                    | Serialize   | NotFound |   464.2 ns | 1.67 ns | 1.40 ns |  1.11 | 0.0358 |     600 B |        1.97 |
|                                    |             |          |            |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Serialize**   | **Serialize**   | **Success**  |   **195.8 ns** | **0.44 ns** | **0.39 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| OneOf_Serialize                    | Serialize   | Success  |   160.0 ns | 0.48 ns | 0.45 ns |  0.82 | 0.0052 |      88 B |        1.10 |
| Dunet_Serialize                    | Serialize   | Success  |   229.2 ns | 0.80 ns | 0.62 ns |  1.17 | 0.0229 |     384 B |        4.80 |
