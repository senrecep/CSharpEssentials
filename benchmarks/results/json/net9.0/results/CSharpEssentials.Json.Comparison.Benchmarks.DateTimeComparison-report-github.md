```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |----------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Iso      | Iso        |   121.1 ns |  0.04 ns |  0.04 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Iso                   | Iso        |   100.7 ns |  0.06 ns |  0.05 ns |  0.83 |    0.00 |      - |         - |          NA |
| BclFallback_Iso           | Iso        |   103.8 ns |  0.03 ns |  0.03 ns |  0.86 |    0.00 |      - |         - |          NA |
|                           |            |            |          |          |       |         |        |           |             |
| CSharpEssentials_LateList | LateList   | 1,263.0 ns | 15.62 ns | 14.61 ns |  1.00 |    0.02 | 0.0019 |      48 B |        1.00 |
| BclFallback_LateList      | LateList   |   319.0 ns |  2.77 ns |  2.59 ns |  0.25 |    0.00 | 0.0029 |      48 B |        1.00 |
