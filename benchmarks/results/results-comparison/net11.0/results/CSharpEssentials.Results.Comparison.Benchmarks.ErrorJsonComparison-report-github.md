```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 442.8 ns | 0.43 ns | 0.38 ns |  1.00 | 0.0153 |     256 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 243.8 ns | 0.70 ns | 0.66 ns |  1.00 | 0.0162 |     272 B |        1.00 |
| ErrorOr_Serialize            | Serialize   | 249.4 ns | 1.24 ns | 1.16 ns |  1.02 | 0.0162 |     272 B |        1.00 |
