```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Categories  | State | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Left**  | **381.2 ns** | **0.45 ns** | **0.42 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Left  | 281.5 ns | 0.13 ns | 0.11 ns |  0.74 | 0.0038 |      64 B |        0.42 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Right** | **343.0 ns** | **0.32 ns** | **0.28 ns** |  **1.00** | **0.0072** |     **120 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Right | 260.8 ns | 0.13 ns | 0.11 ns |  0.76 | 0.0019 |      32 B |        0.27 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Both**  | **379.4 ns** | **0.29 ns** | **0.25 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Both  | 282.7 ns | 0.29 ns | 0.25 ns |  0.75 | 0.0038 |      64 B |        0.42 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Left**  | **230.0 ns** | **1.16 ns** | **1.09 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Left  | 226.8 ns | 0.25 ns | 0.21 ns |  0.99 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Right** | **210.3 ns** | **1.89 ns** | **1.77 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Right | 193.7 ns | 0.41 ns | 0.32 ns |  0.92 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Both**  | **228.7 ns** | **1.37 ns** | **1.28 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Both  | 221.8 ns | 0.41 ns | 0.39 ns |  0.97 | 0.0100 |     168 B |        1.00 |
