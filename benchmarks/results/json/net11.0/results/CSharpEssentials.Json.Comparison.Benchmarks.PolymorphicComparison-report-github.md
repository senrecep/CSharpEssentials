```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 659.0 ns | 0.57 ns | 0.47 ns |  1.00 | 0.0134 |     232 B |        1.00 |
| Stj_Deserialize              | Deserialize | 392.9 ns | 0.87 ns | 0.82 ns |  0.60 | 0.0134 |     232 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 673.1 ns | 2.85 ns | 2.52 ns |  1.00 | 0.0381 |     640 B |        1.00 |
| Stj_Serialize                | Serialize   | 286.3 ns | 0.80 ns | 0.75 ns |  0.43 | 0.0315 |     528 B |        0.82 |
