```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_FromInt  | FromInt    |  8.6522 ns | 0.0331 ns | 0.0310 ns |  1.00 | 0.0014 | 0.0001 |      24 B |        1.00 |
| OneOf_FromInt             | FromInt    |  0.5826 ns | 0.0003 ns | 0.0003 ns |  0.07 |      - |      - |         - |        0.00 |
| Dunet_FromInt             | FromInt    | 12.5759 ns | 0.0534 ns | 0.0500 ns |  1.45 | 0.0014 | 0.0001 |      24 B |        1.00 |
|                           |            |            |           |           |       |        |        |           |             |
| CSharpEssentials_FromText | FromText   |  2.6126 ns | 0.0014 ns | 0.0011 ns |  1.00 |      - |      - |         - |          NA |
| OneOf_FromText            | FromText   |  2.6213 ns | 0.0011 ns | 0.0009 ns |  1.00 |      - |      - |         - |          NA |
| Dunet_FromText            | FromText   | 12.4445 ns | 0.0380 ns | 0.0337 ns |  4.76 | 0.0014 | 0.0001 |      24 B |          NA |
