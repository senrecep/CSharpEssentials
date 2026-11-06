```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |----------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Iso      | Iso        |   108.01 ns | 0.039 ns | 0.032 ns |  1.00 |      - |         - |          NA |
| Bcl_Iso                   | Iso        |    89.51 ns | 0.019 ns | 0.016 ns |  0.83 |      - |         - |          NA |
| BclFallback_Iso           | Iso        |   107.70 ns | 1.660 ns | 1.386 ns |  1.00 |      - |         - |          NA |
|                           |            |             |          |          |       |        |           |             |
| CSharpEssentials_LateList | LateList   | 1,251.91 ns | 2.637 ns | 2.202 ns |  1.00 | 0.0019 |      48 B |        1.00 |
| BclFallback_LateList      | LateList   |   323.40 ns | 0.369 ns | 0.327 ns |  0.26 | 0.0029 |      48 B |        1.00 |
