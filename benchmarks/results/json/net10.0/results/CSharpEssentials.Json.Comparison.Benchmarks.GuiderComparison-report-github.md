```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     |  35.82 ns | 0.095 ns | 0.089 ns |  1.00 |    0.00 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 258.74 ns | 1.923 ns | 1.799 ns |  7.22 |    0.05 | 0.0415 |     696 B |          NA |
| Bcl_Decode              | Decode     |  22.56 ns | 0.047 ns | 0.044 ns |  0.63 |    0.00 |      - |         - |          NA |
|                         |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Encode | Encode     |  21.96 ns | 0.145 ns | 0.136 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| ShortGuid_Encode        | Encode     |  65.01 ns | 0.506 ns | 0.473 ns |  2.96 |    0.03 | 0.0153 |     256 B |        3.56 |
| Bcl_Encode              | Encode     |  19.63 ns | 0.219 ns | 0.205 ns |  0.89 |    0.01 | 0.0043 |      72 B |        1.00 |
