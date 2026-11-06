```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                    | Categories | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |----------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Iso      | Iso        |    94.65 ns | 0.060 ns | 0.050 ns |  1.00 |      - |         - |          NA |
| Bcl_Iso                   | Iso        |    82.41 ns | 0.049 ns | 0.044 ns |  0.87 |      - |         - |          NA |
| BclFallback_Iso           | Iso        |    84.04 ns | 0.059 ns | 0.055 ns |  0.89 |      - |         - |          NA |
|                           |            |             |          |          |       |        |           |             |
| CSharpEssentials_LateList | LateList   | 1,249.50 ns | 5.682 ns | 5.315 ns |  1.00 | 0.0019 |      48 B |        1.00 |
| BclFallback_LateList      | LateList   |   288.28 ns | 0.106 ns | 0.083 ns |  0.23 | 0.0029 |      48 B |        1.00 |
