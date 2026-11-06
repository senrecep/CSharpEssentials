```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names     | Names      |  38.36 ns | 0.317 ns | 0.281 ns |  1.00 |    0.01 |      - |         - |          NA |
| CSharpEssentials_HeldNames | Names      |  23.57 ns | 0.181 ns | 0.169 ns |  0.61 |    0.01 |      - |         - |          NA |
| Bcl_Names                  | Names      |  32.81 ns | 0.323 ns | 0.286 ns |  0.86 |    0.01 | 0.0110 |     184 B |          NA |
| EnumsNet_Names             | Names      |  21.55 ns | 0.110 ns | 0.098 ns |  0.56 |    0.00 |      - |         - |          NA |
|                            |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Values    | Values     |  32.70 ns | 0.475 ns | 0.445 ns |  1.00 |    0.02 |      - |         - |          NA |
| Bcl_Values                 | Values     | 206.56 ns | 0.610 ns | 0.570 ns |  6.32 |    0.08 | 0.0348 |     584 B |          NA |
| EnumsNet_Values            | Values     |  29.72 ns | 0.540 ns | 0.663 ns |  0.91 |    0.02 |      - |         - |          NA |
