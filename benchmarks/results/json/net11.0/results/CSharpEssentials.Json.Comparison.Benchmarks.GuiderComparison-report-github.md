```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     |  21.76 ns | 0.017 ns | 0.016 ns |  1.00 |    0.00 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 188.53 ns | 0.897 ns | 0.839 ns |  8.66 |    0.04 | 0.0415 |     696 B |          NA |
| Bcl_Decode              | Decode     |  20.23 ns | 0.035 ns | 0.033 ns |  0.93 |    0.00 |      - |         - |          NA |
|                         |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Encode | Encode     |  14.33 ns | 0.101 ns | 0.094 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| ShortGuid_Encode        | Encode     |  53.79 ns | 0.231 ns | 0.216 ns |  3.75 |    0.03 | 0.0153 |     256 B |        3.56 |
| Bcl_Encode              | Encode     |  13.94 ns | 0.082 ns | 0.068 ns |  0.97 |    0.01 | 0.0043 |      72 B |        1.00 |
