```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 19.966 ns | 0.4299 ns | 0.4600 ns |  1.00 |    0.03 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 30.191 ns | 0.4268 ns | 0.3784 ns |  1.51 |    0.04 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 15.949 ns | 0.1863 ns | 0.1742 ns |  0.80 |    0.02 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 20.270 ns | 0.4263 ns | 0.4738 ns |  1.02 |    0.03 | 0.0110 |     184 B |        1.00 |
|                         |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  |  9.415 ns | 0.0139 ns | 0.0130 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Values              | GetValues  | 32.363 ns | 0.2139 ns | 0.1896 ns |  3.44 |    0.02 | 0.0062 |     104 B |          NA |
| EnumsNet_Values         | GetValues  | 13.882 ns | 0.2063 ns | 0.1930 ns |  1.47 |    0.02 |      - |         - |          NA |
| NetEscapades_Values     | GetValues  |  9.331 ns | 0.0124 ns | 0.0110 ns |  0.99 |    0.00 |      - |         - |          NA |
