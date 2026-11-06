```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Direct_Await                  | Empty      |    13.94 ns |  0.158 ns |  0.148 ns |  0.17 |    0.00 | 0.0043 |      72 B |        0.90 |
| Polly_Empty                   | Empty      |    58.40 ns |  0.068 ns |  0.060 ns |  0.69 |    0.00 |      - |         - |        0.00 |
| CSharpEssentials_Empty        | Empty      |    84.48 ns |  0.202 ns |  0.169 ns |  1.00 |    0.00 | 0.0048 |      80 B |        1.00 |
|                               |            |             |           |           |       |         |        |           |             |
| CSharpEssentials_Retry        | Retry      |   275.96 ns |  0.336 ns |  0.314 ns |  1.00 |    0.00 | 0.0062 |     104 B |        1.00 |
| Polly_Retry                   | Retry      |   260.21 ns |  0.156 ns |  0.130 ns |  0.94 |    0.00 | 0.0014 |      24 B |        0.23 |
| CSharpEssentials_RetryPerCall | Retry      | 3,466.70 ns | 25.956 ns | 24.280 ns | 12.56 |    0.09 | 0.4158 |    6984 B |       67.15 |
| Polly_RetryPerCall            | Retry      | 3,398.24 ns | 13.949 ns | 13.048 ns | 12.31 |    0.05 | 0.4120 |    6904 B |       66.38 |
