```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     |  56.49 ns | 0.449 ns | 0.375 ns |  1.00 |    0.01 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 197.32 ns | 1.164 ns | 0.972 ns |  3.49 |    0.03 | 0.0415 |     696 B |          NA |
| Bcl_Decode              | Decode     |  20.26 ns | 0.039 ns | 0.037 ns |  0.36 |    0.00 |      - |         - |          NA |
|                         |            |           |          |          |       |         |        |           |             |
| CSharpEssentials_Encode | Encode     |  47.72 ns | 0.132 ns | 0.123 ns |  1.00 |    0.00 | 0.0043 |      72 B |        1.00 |
| ShortGuid_Encode        | Encode     |  57.23 ns | 0.776 ns | 0.726 ns |  1.20 |    0.02 | 0.0153 |     256 B |        3.56 |
| Bcl_Encode              | Encode     |  15.03 ns | 0.114 ns | 0.106 ns |  0.32 |    0.00 | 0.0043 |      72 B |        1.00 |
