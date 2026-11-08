```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     |  99.13 ns | 0.168 ns | 0.149 ns |  1.00 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 264.13 ns | 0.665 ns | 0.519 ns |  2.66 | 0.0415 |     696 B |          NA |
|                         |            |           |          |          |       |        |           |             |
| CSharpEssentials_Encode | Encode     |  56.72 ns | 0.290 ns | 0.257 ns |  1.00 | 0.0067 |     112 B |        1.00 |
| ShortGuid_Encode        | Encode     |  67.82 ns | 0.435 ns | 0.407 ns |  1.20 | 0.0153 |     256 B |        2.29 |
