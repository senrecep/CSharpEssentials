```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 537.9 ns | 0.61 ns | 0.54 ns |  1.00 | 0.0181 |     304 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 289.8 ns | 0.84 ns | 0.78 ns |  1.00 | 0.0162 |     272 B |        1.00 |
| ErrorOr_Serialize            | Serialize   | 333.1 ns | 2.04 ns | 1.91 ns |  1.15 | 0.0162 |     272 B |        1.00 |
