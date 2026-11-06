```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_FlagsRead  | FlagsRead  | 150.69 ns | 0.227 ns | 0.213 ns |  1.00 |      - |         - |          NA |
| Bcl_FlagsRead               | FlagsRead  |  81.97 ns | 0.074 ns | 0.065 ns |  0.54 |      - |         - |          NA |
| Macross_FlagsRead           | FlagsRead  |  88.04 ns | 0.118 ns | 0.104 ns |  0.58 | 0.0029 |      48 B |          NA |
|                             |            |           |          |          |       |        |           |             |
| CSharpEssentials_FlagsWrite | FlagsWrite | 138.46 ns | 0.404 ns | 0.337 ns |  1.00 | 0.0033 |      56 B |        1.00 |
| Bcl_FlagsWrite              | FlagsWrite |  88.02 ns | 0.226 ns | 0.188 ns |  0.64 | 0.0029 |      48 B |        0.86 |
| Macross_FlagsWrite          | FlagsWrite | 103.99 ns | 1.531 ns | 1.432 ns |  0.75 | 0.0029 |      48 B |        0.86 |
