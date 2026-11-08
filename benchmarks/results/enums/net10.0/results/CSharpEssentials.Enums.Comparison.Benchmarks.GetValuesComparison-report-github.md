```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 18.99 ns | 0.167 ns | 0.156 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 38.65 ns | 0.286 ns | 0.268 ns |  2.04 |    0.02 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 16.53 ns | 0.104 ns | 0.093 ns |  0.87 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 19.67 ns | 0.120 ns | 0.112 ns |  1.04 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 11.71 ns | 0.019 ns | 0.017 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Values              | GetValues  | 31.33 ns | 0.161 ns | 0.151 ns |  2.68 |    0.01 | 0.0062 |     104 B |          NA |
| EnumsNet_Values         | GetValues  | 14.29 ns | 0.243 ns | 0.227 ns |  1.22 |    0.02 |      - |         - |          NA |
| NetEscapades_Values     | GetValues  | 11.65 ns | 0.017 ns | 0.015 ns |  0.99 |    0.00 |      - |         - |          NA |
