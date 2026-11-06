```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_FlagsRead  | FlagsRead  | 173.0 ns | 0.33 ns | 0.29 ns |  1.00 |      - |         - |          NA |
| Bcl_FlagsRead               | FlagsRead  | 112.7 ns | 0.32 ns | 0.28 ns |  0.65 | 0.0029 |      48 B |          NA |
| Macross_FlagsRead           | FlagsRead  | 106.2 ns | 0.07 ns | 0.06 ns |  0.61 | 0.0029 |      48 B |          NA |
|                             |            |          |         |         |       |        |           |             |
| CSharpEssentials_FlagsWrite | FlagsWrite | 157.9 ns | 0.13 ns | 0.11 ns |  1.00 | 0.0033 |      56 B |        1.00 |
| Bcl_FlagsWrite              | FlagsWrite | 112.2 ns | 0.13 ns | 0.12 ns |  0.71 | 0.0029 |      48 B |        0.86 |
| Macross_FlagsWrite          | FlagsWrite | 123.4 ns | 0.19 ns | 0.17 ns |  0.78 | 0.0029 |      48 B |        0.86 |
