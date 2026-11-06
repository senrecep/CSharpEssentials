```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_FlagsRead  | FlagsRead  | 149.19 ns | 0.360 ns | 0.319 ns |  1.00 |      - |         - |          NA |
| Bcl_FlagsRead               | FlagsRead  |  82.61 ns | 0.053 ns | 0.050 ns |  0.55 |      - |         - |          NA |
| Macross_FlagsRead           | FlagsRead  |  86.60 ns | 0.161 ns | 0.143 ns |  0.58 | 0.0029 |      48 B |          NA |
|                             |            |           |          |          |       |        |           |             |
| CSharpEssentials_FlagsWrite | FlagsWrite | 122.73 ns | 0.276 ns | 0.244 ns |  1.00 | 0.0033 |      56 B |        1.00 |
| Bcl_FlagsWrite              | FlagsWrite |  77.50 ns | 0.109 ns | 0.097 ns |  0.63 | 0.0029 |      48 B |        0.86 |
| Macross_FlagsWrite          | FlagsWrite |  84.21 ns | 0.088 ns | 0.078 ns |  0.69 | 0.0029 |      48 B |        0.86 |
