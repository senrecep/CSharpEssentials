```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Categories  | State | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Left**  | **379.2 ns** | **0.49 ns** | **0.43 ns** |  **1.00** | **0.0110** |     **184 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Left  | 287.4 ns | 0.34 ns | 0.29 ns |  0.76 | 0.0038 |      64 B |        0.35 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Right** | **343.6 ns** | **1.58 ns** | **1.48 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Right | 257.6 ns | 1.08 ns | 1.01 ns |  0.75 | 0.0019 |      32 B |        0.21 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Both**  | **380.8 ns** | **0.47 ns** | **0.42 ns** |  **1.00** | **0.0110** |     **184 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Both  | 279.6 ns | 0.77 ns | 0.69 ns |  0.73 | 0.0038 |      64 B |        0.35 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Left**  | **237.6 ns** | **1.07 ns** | **1.00 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Left  | 235.8 ns | 1.24 ns | 1.16 ns |  0.99 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Right** | **219.0 ns** | **1.30 ns** | **1.22 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Right | 241.9 ns | 0.55 ns | 0.46 ns |  1.10 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Both**  | **254.3 ns** | **0.70 ns** | **0.65 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Both  | 247.5 ns | 0.52 ns | 0.48 ns |  0.97 | 0.0100 |     168 B |        1.00 |
