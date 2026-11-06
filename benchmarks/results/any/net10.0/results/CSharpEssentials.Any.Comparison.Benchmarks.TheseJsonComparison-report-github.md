```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | State | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Deserialize** | **Deserialize** | **Left**  | **342.3 ns** | **1.11 ns** | **0.98 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Left  | 259.8 ns | 0.60 ns | 0.47 ns |  0.76 | 0.0038 |      64 B |        0.42 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Right** | **325.0 ns** | **0.86 ns** | **0.80 ns** |  **1.00** | **0.0072** |     **120 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Right | 240.1 ns | 1.01 ns | 0.94 ns |  0.74 | 0.0019 |      32 B |        0.27 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Deserialize** | **Deserialize** | **Both**  | **340.8 ns** | **0.36 ns** | **0.32 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten_Deserialize      | Deserialize | Both  | 258.8 ns | 0.35 ns | 0.31 ns |  0.76 | 0.0038 |      64 B |        0.42 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Left**  | **231.4 ns** | **1.12 ns** | **1.04 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Left  | 221.1 ns | 1.03 ns | 0.96 ns |  0.96 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Right** | **206.5 ns** | **1.74 ns** | **1.62 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Right | 183.1 ns | 1.36 ns | 1.28 ns |  0.89 | 0.0100 |     168 B |        1.00 |
|                              |             |       |          |         |         |       |        |           |             |
| **CSharpEssentials_Serialize**   | **Serialize**   | **Both**  | **223.9 ns** | **1.48 ns** | **1.39 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| HandWritten_Serialize        | Serialize   | Both  | 223.5 ns | 0.35 ns | 0.31 ns |  1.00 | 0.0100 |     168 B |        1.00 |
