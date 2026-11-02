```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 19.22 ns | 0.084 ns | 0.079 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 31.09 ns | 0.373 ns | 0.331 ns |  1.62 |    0.02 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 17.41 ns | 0.131 ns | 0.122 ns |  0.91 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 19.30 ns | 0.222 ns | 0.207 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 15.84 ns | 0.241 ns | 0.225 ns |  1.00 |    0.02 | 0.0062 |     104 B |        1.00 |
| Bcl_Values              | GetValues  | 34.94 ns | 0.318 ns | 0.298 ns |  2.21 |    0.04 | 0.0062 |     104 B |        1.00 |
| EnumsNet_Values         | GetValues  | 15.61 ns | 0.180 ns | 0.169 ns |  0.99 |    0.02 |      - |         - |        0.00 |
| NetEscapades_Values     | GetValues  | 15.61 ns | 0.124 ns | 0.110 ns |  0.99 |    0.02 | 0.0062 |     104 B |        1.00 |
