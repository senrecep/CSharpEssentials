```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Names     | Names      |  45.72 ns | 0.362 ns | 0.339 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HeldNames | Names      |  24.27 ns | 0.139 ns | 0.130 ns |  0.53 |      - |         - |          NA |
| Bcl_Names                  | Names      |  36.17 ns | 0.537 ns | 0.502 ns |  0.79 | 0.0110 |     184 B |          NA |
| EnumsNet_Names             | Names      |  22.09 ns | 0.207 ns | 0.194 ns |  0.48 |      - |         - |          NA |
|                            |            |           |          |          |       |        |           |             |
| CSharpEssentials_Values    | Values     | 137.76 ns | 0.125 ns | 0.105 ns |  1.00 |      - |         - |          NA |
| Bcl_Values                 | Values     | 932.84 ns | 0.846 ns | 0.750 ns |  6.77 | 0.0343 |     584 B |          NA |
| EnumsNet_Values            | Values     |  30.52 ns | 0.152 ns | 0.142 ns |  0.22 |      - |         - |          NA |
