```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 20.57 ns | 0.228 ns | 0.202 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 35.34 ns | 0.213 ns | 0.199 ns |  1.72 |    0.02 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 16.13 ns | 0.112 ns | 0.099 ns |  0.78 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 19.74 ns | 0.087 ns | 0.082 ns |  0.96 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  | 11.95 ns | 0.013 ns | 0.012 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Values              | GetValues  | 32.15 ns | 0.278 ns | 0.247 ns |  2.69 |    0.02 | 0.0062 |     104 B |          NA |
| EnumsNet_Values         | GetValues  | 14.11 ns | 0.184 ns | 0.172 ns |  1.18 |    0.01 |      - |         - |          NA |
| NetEscapades_Values     | GetValues  | 11.42 ns | 0.011 ns | 0.010 ns |  0.96 |    0.00 |      - |         - |          NA |
