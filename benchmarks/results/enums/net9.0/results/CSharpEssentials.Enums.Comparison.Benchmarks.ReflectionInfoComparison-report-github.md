```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names     | Names      |    40.25 ns | 0.039 ns | 0.036 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_HeldNames | Names      |    24.18 ns | 0.463 ns | 0.720 ns |  0.60 |    0.02 |      - |         - |          NA |
| Bcl_Names                  | Names      |    36.42 ns | 0.442 ns | 0.414 ns |  0.91 |    0.01 | 0.0110 |     184 B |          NA |
| EnumsNet_Names             | Names      |    22.14 ns | 0.104 ns | 0.092 ns |  0.55 |    0.00 |      - |         - |          NA |
|                            |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_Values    | Values     |   125.39 ns | 0.729 ns | 0.646 ns |  1.00 |    0.01 |      - |         - |          NA |
| Bcl_Values                 | Values     | 1,008.29 ns | 1.472 ns | 1.229 ns |  8.04 |    0.04 | 0.0343 |     584 B |          NA |
| EnumsNet_Values            | Values     |    28.56 ns | 0.037 ns | 0.034 ns |  0.23 |    0.00 |      - |         - |          NA |
