```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 531.3 ns | 2.38 ns | 2.22 ns |  1.00 | 0.0181 |     304 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 341.1 ns | 0.61 ns | 0.51 ns |  1.00 | 0.0162 |     272 B |        1.00 |
| ErrorOr_Serialize            | Serialize   | 321.2 ns | 0.81 ns | 0.72 ns |  0.94 | 0.0162 |     272 B |        1.00 |
