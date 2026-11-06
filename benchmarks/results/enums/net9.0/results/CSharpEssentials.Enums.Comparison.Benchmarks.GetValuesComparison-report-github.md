```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 20.36 ns | 0.261 ns | 0.244 ns |  1.00 |    0.02 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 32.50 ns | 0.508 ns | 0.475 ns |  1.60 |    0.03 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 17.26 ns | 0.196 ns | 0.174 ns |  0.85 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 20.34 ns | 0.371 ns | 0.347 ns |  1.00 |    0.02 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 16.46 ns | 0.198 ns | 0.185 ns |  1.00 |    0.02 | 0.0062 |     104 B |        1.00 |
| Bcl_Values              | GetValues  | 36.46 ns | 0.198 ns | 0.175 ns |  2.22 |    0.03 | 0.0062 |     104 B |        1.00 |
| EnumsNet_Values         | GetValues  | 15.55 ns | 0.131 ns | 0.122 ns |  0.94 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Values     | GetValues  | 15.77 ns | 0.246 ns | 0.206 ns |  0.96 |    0.02 | 0.0062 |     104 B |        1.00 |
