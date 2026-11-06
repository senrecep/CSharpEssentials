```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_FromInt  | FromInt    |  7.2450 ns | 0.0486 ns | 0.0455 ns |  1.00 |    0.01 | 0.0014 | 0.0001 |      24 B |        1.00 |
| OneOf_FromInt             | FromInt    |  0.4401 ns | 0.0006 ns | 0.0005 ns |  0.06 |    0.00 |      - |      - |         - |        0.00 |
| Dunet_FromInt             | FromInt    |  9.5177 ns | 0.0343 ns | 0.0304 ns |  1.31 |    0.01 | 0.0014 | 0.0001 |      24 B |        1.00 |
|                           |            |            |           |           |       |         |        |        |           |             |
| CSharpEssentials_FromText | FromText   |  2.3129 ns | 0.0004 ns | 0.0003 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| OneOf_FromText            | FromText   |  2.0255 ns | 0.0006 ns | 0.0005 ns |  0.88 |    0.00 |      - |      - |         - |          NA |
| Dunet_FromText            | FromText   | 11.2354 ns | 0.1074 ns | 0.1005 ns |  4.86 |    0.04 | 0.0014 | 0.0001 |      24 B |          NA |
