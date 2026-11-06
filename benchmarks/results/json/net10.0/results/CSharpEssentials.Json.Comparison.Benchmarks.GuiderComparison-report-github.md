```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     |  80.81 ns | 0.673 ns | 0.596 ns |  1.00 |    0.01 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 253.70 ns | 5.029 ns | 5.589 ns |  3.14 |    0.07 | 0.0415 |     696 B |          NA |
| Bcl_Decode              | Decode     |  22.12 ns | 0.027 ns | 0.025 ns |  0.27 |    0.00 |      - |         - |          NA |
|                         |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Encode | Encode     |  46.85 ns | 0.260 ns | 0.243 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| ShortGuid_Encode        | Encode     |  68.64 ns | 0.362 ns | 0.339 ns |  1.47 |    0.01 | 0.0153 |     256 B |        3.56 |
| Bcl_Encode              | Encode     |  22.26 ns | 0.125 ns | 0.117 ns |  0.48 |    0.00 | 0.0043 |      72 B |        1.00 |
