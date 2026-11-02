```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 20.37 ns | 0.153 ns | 0.143 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 31.41 ns | 0.466 ns | 0.436 ns |  1.54 |    0.02 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 17.45 ns | 0.141 ns | 0.132 ns |  0.86 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 19.78 ns | 0.196 ns | 0.174 ns |  0.97 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 15.79 ns | 0.189 ns | 0.177 ns |  1.00 |    0.02 | 0.0062 |     104 B |        1.00 |
| Bcl_Values              | GetValues  | 44.19 ns | 0.273 ns | 0.255 ns |  2.80 |    0.03 | 0.0062 |     104 B |        1.00 |
| EnumsNet_Values         | GetValues  | 15.61 ns | 0.004 ns | 0.003 ns |  0.99 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Values     | GetValues  | 15.62 ns | 0.181 ns | 0.169 ns |  0.99 |    0.02 | 0.0062 |     104 B |        1.00 |
