```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 644.2 ns | 2.66 ns | 2.49 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 389.6 ns | 1.56 ns | 1.30 ns |  0.60 | 0.0134 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 370.0 ns | 0.84 ns | 0.70 ns |  1.00 | 0.0281 |     472 B |        1.00 |
| Stj_Serialize                | Serialize   | 263.1 ns | 1.76 ns | 1.65 ns |  0.71 | 0.0315 |     528 B |        1.12 |
