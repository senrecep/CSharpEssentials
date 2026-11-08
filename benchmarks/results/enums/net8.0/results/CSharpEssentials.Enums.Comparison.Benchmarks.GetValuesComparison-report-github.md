```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 20.45 ns | 0.130 ns | 0.122 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 29.68 ns | 0.222 ns | 0.208 ns |  1.45 |    0.01 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 17.39 ns | 0.140 ns | 0.131 ns |  0.85 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 20.37 ns | 0.094 ns | 0.083 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 15.56 ns | 0.137 ns | 0.129 ns |  1.00 |    0.01 | 0.0062 |     104 B |        1.00 |
| Bcl_Values              | GetValues  | 44.47 ns | 0.166 ns | 0.139 ns |  2.86 |    0.02 | 0.0062 |     104 B |        1.00 |
| EnumsNet_Values         | GetValues  | 15.62 ns | 0.008 ns | 0.007 ns |  1.00 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Values     | GetValues  | 15.54 ns | 0.162 ns | 0.151 ns |  1.00 |    0.01 | 0.0062 |     104 B |        1.00 |
