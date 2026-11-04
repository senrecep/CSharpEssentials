```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 19.09 ns | 0.122 ns | 0.114 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 33.94 ns | 0.178 ns | 0.167 ns |  1.78 |    0.01 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 16.30 ns | 0.193 ns | 0.172 ns |  0.85 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 18.77 ns | 0.216 ns | 0.202 ns |  0.98 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 11.98 ns | 0.017 ns | 0.016 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Values              | GetValues  | 31.23 ns | 0.270 ns | 0.253 ns |  2.61 |    0.02 | 0.0062 |     104 B |          NA |
| EnumsNet_Values         | GetValues  | 14.02 ns | 0.133 ns | 0.125 ns |  1.17 |    0.01 |      - |         - |          NA |
| NetEscapades_Values     | GetValues  | 11.46 ns | 0.022 ns | 0.021 ns |  0.96 |    0.00 |      - |         - |          NA |
