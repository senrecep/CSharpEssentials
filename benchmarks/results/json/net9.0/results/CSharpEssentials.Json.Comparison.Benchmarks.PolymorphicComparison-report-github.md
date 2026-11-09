```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 781.5 ns | 2.18 ns | 2.04 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 447.1 ns | 1.13 ns | 1.05 ns |  0.57 | 0.0138 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 470.1 ns | 1.72 ns | 1.61 ns |  1.00 | 0.0272 |     456 B |        1.00 |
| Stj_Serialize                | Serialize   | 337.4 ns | 1.50 ns | 1.40 ns |  0.72 | 0.0315 |     528 B |        1.16 |
