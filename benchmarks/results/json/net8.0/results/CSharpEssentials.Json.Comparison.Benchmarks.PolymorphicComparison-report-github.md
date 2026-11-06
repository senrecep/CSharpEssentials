```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 827.5 ns | 0.55 ns | 0.51 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 467.6 ns | 0.39 ns | 0.37 ns |  0.57 | 0.0134 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 871.4 ns | 1.20 ns | 1.12 ns |  1.00 | 0.0372 |     624 B |        1.00 |
| Stj_Serialize                | Serialize   | 347.9 ns | 1.82 ns | 1.70 ns |  0.40 | 0.0315 |     528 B |        0.85 |
