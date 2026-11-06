```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 668.7 ns | 0.53 ns | 0.41 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 378.5 ns | 0.56 ns | 0.46 ns |  0.57 | 0.0138 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 763.5 ns | 2.29 ns | 2.03 ns |  1.00 | 0.0381 |     640 B |        1.00 |
| Stj_Serialize                | Serialize   | 296.2 ns | 0.99 ns | 0.88 ns |  0.39 | 0.0315 |     528 B |        0.82 |
