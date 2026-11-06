```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_FromInt  | FromInt    |  8.2686 ns | 0.0662 ns | 0.0619 ns |  8.2647 ns |  1.00 |    0.01 | 0.0014 | 0.0001 |      24 B |        1.00 |
| OneOf_FromInt             | FromInt    |  0.5900 ns | 0.0025 ns | 0.0022 ns |  0.5893 ns |  0.07 |    0.00 |      - |      - |         - |        0.00 |
| Dunet_FromInt             | FromInt    | 10.5166 ns | 0.0511 ns | 0.0478 ns | 10.5154 ns |  1.27 |    0.01 | 0.0014 | 0.0001 |      24 B |        1.00 |
|                           |            |            |           |           |            |       |         |        |        |           |             |
| CSharpEssentials_FromText | FromText   |  2.8651 ns | 0.0562 ns | 0.0750 ns |  2.8909 ns |  1.00 |    0.04 |      - |      - |         - |          NA |
| OneOf_FromText            | FromText   |  2.6899 ns | 0.0538 ns | 0.1215 ns |  2.6132 ns |  0.94 |    0.05 |      - |      - |         - |          NA |
| Dunet_FromText            | FromText   | 12.2663 ns | 0.0327 ns | 0.0289 ns | 12.2625 ns |  4.28 |    0.12 | 0.0014 | 0.0001 |      24 B |          NA |
