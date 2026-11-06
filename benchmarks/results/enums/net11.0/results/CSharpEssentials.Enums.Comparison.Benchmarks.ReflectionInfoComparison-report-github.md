```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names     | Names      |  40.33 ns | 0.027 ns | 0.024 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_HeldNames | Names      |  23.28 ns | 0.113 ns | 0.094 ns |  0.58 |    0.00 |      - |         - |          NA |
| Bcl_Names                  | Names      |  35.59 ns | 0.709 ns | 0.729 ns |  0.88 |    0.02 | 0.0110 |     184 B |          NA |
| EnumsNet_Names             | Names      |  20.94 ns | 0.069 ns | 0.061 ns |  0.52 |    0.00 |      - |         - |          NA |
|                            |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Values    | Values     |  33.80 ns | 0.055 ns | 0.049 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Values                 | Values     | 191.69 ns | 1.254 ns | 1.173 ns |  5.67 |    0.03 | 0.0348 |     584 B |          NA |
| EnumsNet_Values            | Values     |  28.84 ns | 0.062 ns | 0.058 ns |  0.85 |    0.00 |      - |         - |          NA |
