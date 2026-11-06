```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Deserialize | Deserialize | 492.1 ns | 1.06 ns | 0.94 ns |  1.00 | 0.0181 |     304 B |        1.00 |
|                              |             |          |         |         |       |        |           |             |
| CSharpEssentials_Serialize   | Serialize   | 280.2 ns | 0.90 ns | 0.84 ns |  1.00 | 0.0162 |     272 B |        1.00 |
| ErrorOr_Serialize            | Serialize   | 280.3 ns | 1.22 ns | 1.14 ns |  1.00 | 0.0162 |     272 B |        1.00 |
