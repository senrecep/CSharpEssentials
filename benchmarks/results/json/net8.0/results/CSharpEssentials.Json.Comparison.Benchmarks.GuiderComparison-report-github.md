```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                  | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Decode | Decode     | 101.99 ns | 0.139 ns | 0.130 ns |  1.00 |      - |         - |          NA |
| ShortGuid_Decode        | Decode     | 268.40 ns | 1.107 ns | 0.981 ns |  2.63 | 0.0415 |     696 B |          NA |
|                         |            |           |          |          |       |        |           |             |
| CSharpEssentials_Encode | Encode     |  56.79 ns | 0.197 ns | 0.185 ns |  1.00 | 0.0067 |     112 B |        1.00 |
| ShortGuid_Encode        | Encode     |  72.78 ns | 0.600 ns | 0.531 ns |  1.28 | 0.0153 |     256 B |        2.29 |
