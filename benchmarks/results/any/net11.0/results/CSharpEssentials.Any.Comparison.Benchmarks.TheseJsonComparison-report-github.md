```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | State | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Left**  | **342.6 ns** | **1.63 ns** | **1.44 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Left  | 239.3 ns | 1.54 ns | 1.44 ns |  0.70 | 0.0038 |      64 B |        0.42 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Right** | **318.1 ns** | **2.23 ns** | **1.98 ns** |  **1.00** | **0.0072** |     **120 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Right | 216.8 ns | 0.23 ns | 0.18 ns |  0.68 | 0.0019 |      32 B |        0.27 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Both**  | **340.1 ns** | **0.88 ns** | **0.82 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Both  | 261.6 ns | 0.36 ns | 0.28 ns |  0.77 | 0.0038 |      64 B |        0.42 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Left**  | **222.0 ns** | **1.50 ns** | **1.40 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Left  | 207.0 ns | 1.53 ns | 1.43 ns |  0.93 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Right** | **203.6 ns** | **0.41 ns** | **0.38 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Right | 183.9 ns | 0.26 ns | 0.23 ns |  0.90 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Both**  | **224.8 ns** | **1.58 ns** | **1.48 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Both  | 201.0 ns | 1.99 ns | 1.87 ns |  0.89 | 0.0100 |     168 B |        1.00 |
