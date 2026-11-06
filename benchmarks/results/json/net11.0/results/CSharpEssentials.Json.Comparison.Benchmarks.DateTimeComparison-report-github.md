```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |----------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Iso      | Iso        |    96.76 ns |  0.058 ns |  0.054 ns |  1.00 |      - |         - |          NA |
| Bcl_Iso                   | Iso        |    83.16 ns |  0.202 ns |  0.189 ns |  0.86 |      - |         - |          NA |
| BclFallback_Iso           | Iso        |    86.84 ns |  0.602 ns |  0.563 ns |  0.90 |      - |         - |          NA |
|                           |            |             |           |           |       |        |           |             |
| CSharpEssentials_LateList | LateList   | 1,306.05 ns | 10.838 ns | 10.138 ns |  1.00 |      - |      48 B |        1.00 |
| BclFallback_LateList      | LateList   |   280.85 ns |  3.298 ns |  3.085 ns |  0.22 | 0.0029 |      48 B |        1.00 |
