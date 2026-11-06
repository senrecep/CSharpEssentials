```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 21.46 ns | 0.104 ns | 0.092 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 32.85 ns | 0.322 ns | 0.302 ns |  1.53 |    0.02 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 17.46 ns | 0.106 ns | 0.088 ns |  0.81 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 21.92 ns | 0.195 ns | 0.183 ns |  1.02 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 16.69 ns | 0.170 ns | 0.151 ns |  1.00 |    0.01 | 0.0062 |     104 B |        1.00 |
| Bcl_Values              | GetValues  | 46.04 ns | 0.160 ns | 0.133 ns |  2.76 |    0.03 | 0.0062 |     104 B |        1.00 |
| EnumsNet_Values         | GetValues  | 15.62 ns | 0.005 ns | 0.005 ns |  0.94 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Values     | GetValues  | 16.89 ns | 0.147 ns | 0.130 ns |  1.01 |    0.01 | 0.0062 |     104 B |        1.00 |
