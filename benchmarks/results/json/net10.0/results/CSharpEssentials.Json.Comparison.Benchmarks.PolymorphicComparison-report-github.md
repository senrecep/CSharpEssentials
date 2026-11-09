```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 713.0 ns | 2.88 ns | 2.70 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 382.1 ns | 1.06 ns | 0.99 ns |  0.54 | 0.0138 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 402.6 ns | 2.11 ns | 1.76 ns |  1.00 | 0.0281 |     472 B |        1.00 |
| Stj_Serialize                | Serialize   | 276.3 ns | 1.34 ns | 1.25 ns |  0.69 | 0.0315 |     528 B |        1.12 |
