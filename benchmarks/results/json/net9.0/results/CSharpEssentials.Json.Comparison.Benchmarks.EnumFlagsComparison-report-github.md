```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentials_FlagsRead  | FlagsRead  | 174.4 ns | 1.16 ns | 1.09 ns |  1.00 |      - |         - |          NA |
| Bcl_FlagsRead               | FlagsRead  | 106.4 ns | 0.73 ns | 0.69 ns |  0.61 |      - |         - |          NA |
| Macross_FlagsRead           | FlagsRead  | 111.3 ns | 0.58 ns | 0.55 ns |  0.64 | 0.0029 |      48 B |          NA |
|                             |            |          |         |         |       |        |           |             |
| CSharpEssentials_FlagsWrite | FlagsWrite | 139.3 ns | 1.42 ns | 1.33 ns |  1.00 | 0.0033 |      56 B |        1.00 |
| Bcl_FlagsWrite              | FlagsWrite | 105.6 ns | 0.39 ns | 0.34 ns |  0.76 | 0.0029 |      48 B |        0.86 |
| Macross_FlagsWrite          | FlagsWrite | 119.2 ns | 0.20 ns | 0.17 ns |  0.86 | 0.0029 |      48 B |        0.86 |
