```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     | 102.01 ns | 0.427 ns | 0.379 ns |  1.00 |    0.01 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 257.93 ns | 2.241 ns | 2.096 ns |  2.53 |    0.02 | 0.0415 |     696 B |          NA |
| Bcl_Decode              | Decode     |  24.14 ns | 0.132 ns | 0.124 ns |  0.24 |    0.00 |      - |         - |          NA |
|                         |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Encode | Encode     |  47.68 ns | 0.391 ns | 0.346 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| ShortGuid_Encode        | Encode     |  71.67 ns | 0.773 ns | 0.685 ns |  1.50 |    0.02 | 0.0153 |     256 B |        3.56 |
| Bcl_Encode              | Encode     |  19.52 ns | 0.262 ns | 0.232 ns |  0.41 |    0.01 | 0.0043 |      72 B |        1.00 |
