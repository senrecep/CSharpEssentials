```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_FromInt  | FromInt    |  7.6038 ns | 0.0309 ns | 0.0289 ns |  1.00 |    0.01 | 0.0014 | 0.0001 |      24 B |        1.00 |
| OneOf_FromInt             | FromInt    |  0.4394 ns | 0.0002 ns | 0.0002 ns |  0.06 |    0.00 |      - |      - |         - |        0.00 |
| Dunet_FromInt             | FromInt    |  9.2265 ns | 0.0612 ns | 0.0511 ns |  1.21 |    0.01 | 0.0014 | 0.0001 |      24 B |        1.00 |
|                           |            |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_FromText | FromText   |  2.0299 ns | 0.0006 ns | 0.0005 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| OneOf_FromText            | FromText   |  2.0246 ns | 0.0008 ns | 0.0007 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| Dunet_FromText            | FromText   | 10.7627 ns | 0.0809 ns | 0.0757 ns |  5.30 |    0.04 | 0.0014 | 0.0001 |      24 B |          NA |
