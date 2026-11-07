```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 19.42 ns | 0.189 ns | 0.168 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 30.92 ns | 0.291 ns | 0.272 ns |  1.59 |    0.02 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 17.18 ns | 0.179 ns | 0.167 ns |  0.88 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 19.58 ns | 0.201 ns | 0.188 ns |  1.01 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 15.22 ns | 0.148 ns | 0.138 ns |  1.00 |    0.01 | 0.0062 |     104 B |        1.00 |
| Bcl_Values              | GetValues  | 35.03 ns | 0.193 ns | 0.180 ns |  2.30 |    0.02 | 0.0062 |     104 B |        1.00 |
| EnumsNet_Values         | GetValues  | 15.91 ns | 0.139 ns | 0.130 ns |  1.05 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Values     | GetValues  | 15.26 ns | 0.127 ns | 0.113 ns |  1.00 |    0.01 | 0.0062 |     104 B |        1.00 |
