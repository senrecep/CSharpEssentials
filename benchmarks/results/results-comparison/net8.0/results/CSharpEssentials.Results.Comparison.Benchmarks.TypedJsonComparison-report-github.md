```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Categories  | Case     | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |------------ |--------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Deserialize** | **Deserialize** | **NotFound** | **1,250.8 ns** | **3.69 ns** | **3.45 ns** |  **1.00** | **0.0191** |     **320 B** |        **1.00** |
| OneOf_Deserialize                  | Deserialize | NotFound | 1,075.8 ns | 2.61 ns | 2.44 ns |  0.86 | 0.0286 |     496 B |        1.55 |
| Dunet_Deserialize                  | Deserialize | NotFound |   904.2 ns | 1.91 ns | 1.70 ns |  0.72 | 0.0544 |     912 B |        2.85 |
|                                    |             |          |            |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Deserialize** | **Deserialize** | **Success**  |   **394.5 ns** | **0.41 ns** | **0.37 ns** |  **1.00** | **0.0024** |      **40 B** |        **1.00** |
| OneOf_Deserialize                  | Deserialize | Success  |   387.2 ns | 0.53 ns | 0.47 ns |  0.98 | 0.0138 |     232 B |        5.80 |
| Dunet_Deserialize                  | Deserialize | Success  |   299.7 ns | 0.76 ns | 0.67 ns |  0.76 | 0.0095 |     160 B |        4.00 |
|                                    |             |          |            |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Serialize**   | **Serialize**   | **NotFound** |   **485.8 ns** | **1.47 ns** | **1.23 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| OneOf_Serialize                    | Serialize   | NotFound |   553.0 ns | 0.68 ns | 0.57 ns |  1.14 | 0.0334 |     568 B |        1.87 |
| Dunet_Serialize                    | Serialize   | NotFound |   524.9 ns | 1.60 ns | 1.49 ns |  1.08 | 0.0353 |     600 B |        1.97 |
|                                    |             |          |            |         |         |       |        |           |             |
| **CSharpEssentials_Typed_Serialize**   | **Serialize**   | **Success**  |   **247.1 ns** | **0.72 ns** | **0.64 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| OneOf_Serialize                    | Serialize   | Success  |   340.1 ns | 0.75 ns | 0.66 ns |  1.38 | 0.0205 |     344 B |        4.30 |
| Dunet_Serialize                    | Serialize   | Success  |   256.1 ns | 0.87 ns | 0.77 ns |  1.04 | 0.0229 |     384 B |        4.80 |
