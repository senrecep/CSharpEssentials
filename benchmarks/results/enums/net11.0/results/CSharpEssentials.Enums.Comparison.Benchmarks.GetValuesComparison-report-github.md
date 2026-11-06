```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 18.877 ns | 0.2047 ns | 0.1814 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 29.011 ns | 0.4780 ns | 0.4471 ns |  1.54 |    0.03 | 0.0110 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 16.319 ns | 0.0859 ns | 0.0804 ns |  0.86 |    0.01 |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 18.306 ns | 0.1208 ns | 0.1071 ns |  0.97 |    0.01 | 0.0110 |     184 B |        1.00 |
|                         |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_Values | GetValues  |  9.408 ns | 0.0058 ns | 0.0054 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Values              | GetValues  | 30.426 ns | 0.2071 ns | 0.1937 ns |  3.23 |    0.02 | 0.0062 |     104 B |          NA |
| EnumsNet_Values         | GetValues  | 13.908 ns | 0.1123 ns | 0.0996 ns |  1.48 |    0.01 |      - |         - |          NA |
| NetEscapades_Values     | GetValues  |  9.408 ns | 0.0055 ns | 0.0048 ns |  1.00 |    0.00 |      - |         - |          NA |
