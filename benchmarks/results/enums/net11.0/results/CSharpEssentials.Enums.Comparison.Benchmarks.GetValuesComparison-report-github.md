```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Names  | GetNames   | 18.331 ns | 0.0800 ns | 0.0709 ns |  1.00 |    0.01 | 0.0110 |      - |     184 B |        1.00 |
| Bcl_Names               | GetNames   | 24.735 ns | 0.2782 ns | 0.2466 ns |  1.35 |    0.01 | 0.0110 | 0.0000 |     184 B |        1.00 |
| EnumsNet_Names          | GetNames   | 16.030 ns | 0.1011 ns | 0.0946 ns |  0.87 |    0.01 |      - |      - |         - |        0.00 |
| NetEscapades_Names      | GetNames   | 18.312 ns | 0.0800 ns | 0.0749 ns |  1.00 |    0.01 | 0.0110 |      - |     184 B |        1.00 |
|                         |            |           |           |           |       |         |        |        |           |             |
| CSharpEssentials_Values | GetValues  |  9.123 ns | 0.0069 ns | 0.0061 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| Bcl_Values              | GetValues  | 29.474 ns | 0.2939 ns | 0.2605 ns |  3.23 |    0.03 | 0.0062 |      - |     104 B |          NA |
| EnumsNet_Values         | GetValues  | 15.616 ns | 0.1086 ns | 0.1016 ns |  1.71 |    0.01 |      - |      - |         - |          NA |
| NetEscapades_Values     | GetValues  |  8.963 ns | 0.0032 ns | 0.0030 ns |  0.98 |    0.00 |      - |      - |         - |          NA |
