```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     |  43.46 ns | 0.117 ns | 0.109 ns |  1.00 |    0.00 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 253.47 ns | 4.292 ns | 4.015 ns |  5.83 |    0.09 | 0.0415 |     696 B |          NA |
| Bcl_Decode              | Decode     |  23.79 ns | 0.053 ns | 0.050 ns |  0.55 |    0.00 |      - |         - |          NA |
|                         |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Encode | Encode     |  19.60 ns | 0.135 ns | 0.126 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| ShortGuid_Encode        | Encode     |  70.02 ns | 0.618 ns | 0.548 ns |  3.57 |    0.04 | 0.0153 |     256 B |        3.56 |
| Bcl_Encode              | Encode     |  18.96 ns | 0.141 ns | 0.132 ns |  0.97 |    0.01 | 0.0043 |      72 B |        1.00 |
