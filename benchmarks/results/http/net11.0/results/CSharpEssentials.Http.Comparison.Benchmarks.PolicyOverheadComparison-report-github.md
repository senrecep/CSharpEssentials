```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Direct_Await                  | Empty      |    13.63 ns |  0.091 ns |  0.085 ns |  0.16 |    0.00 | 0.0043 |      72 B |        0.90 |
| Polly_Empty                   | Empty      |    64.50 ns |  0.117 ns |  0.098 ns |  0.75 |    0.00 |      - |         - |        0.00 |
| CSharpEssentials_Empty        | Empty      |    86.21 ns |  0.210 ns |  0.175 ns |  1.00 |    0.00 | 0.0048 |      80 B |        1.00 |
|                               |            |             |           |           |       |         |        |           |             |
| CSharpEssentials_Retry        | Retry      |   241.65 ns |  0.234 ns |  0.219 ns |  1.00 |    0.00 | 0.0062 |     104 B |        1.00 |
| Polly_Retry                   | Retry      |   215.61 ns |  0.257 ns |  0.240 ns |  0.89 |    0.00 | 0.0014 |      24 B |        0.23 |
| CSharpEssentials_RetryPerCall | Retry      | 3,533.64 ns | 12.881 ns | 12.049 ns | 14.62 |    0.05 | 0.4082 |    6848 B |       65.85 |
| Polly_RetryPerCall            | Retry      | 4,351.67 ns | 11.389 ns | 10.653 ns | 18.01 |    0.05 | 0.4044 |    6768 B |       65.08 |
