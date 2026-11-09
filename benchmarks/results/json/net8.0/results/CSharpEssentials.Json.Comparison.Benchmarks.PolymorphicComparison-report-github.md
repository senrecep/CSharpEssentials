```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 809.7 ns | 2.00 ns | 1.67 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 440.7 ns | 1.67 ns | 1.48 ns |  0.54 | 0.0138 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 506.5 ns | 4.19 ns | 3.92 ns |  1.00 | 0.0267 |     456 B |        1.00 |
| Stj_Serialize                | Serialize   | 340.6 ns | 2.15 ns | 1.79 ns |  0.67 | 0.0315 |     528 B |        1.16 |
