```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 781.2 ns | 0.99 ns | 0.88 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 438.8 ns | 0.33 ns | 0.28 ns |  0.56 | 0.0138 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 875.9 ns | 1.89 ns | 1.77 ns |  1.00 | 0.0372 |     624 B |        1.00 |
| Stj_Serialize                | Serialize   | 352.8 ns | 0.88 ns | 0.78 ns |  0.40 | 0.0315 |     528 B |        0.85 |
