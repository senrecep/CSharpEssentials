```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Direct_Await                  | Empty      |    16.36 ns |  0.131 ns |  0.116 ns |  0.19 |    0.00 | 0.0043 |      72 B |        0.90 |
| Polly_Empty                   | Empty      |    71.79 ns |  0.386 ns |  0.343 ns |  0.81 |    0.00 |      - |         - |        0.00 |
| CSharpEssentials_Empty        | Empty      |    88.17 ns |  0.194 ns |  0.151 ns |  1.00 |    0.00 | 0.0048 |      80 B |        1.00 |
|                               |            |             |           |           |       |         |        |           |             |
| CSharpEssentials_Retry        | Retry      |   287.35 ns |  0.384 ns |  0.340 ns |  1.00 |    0.00 | 0.0062 |     104 B |        1.00 |
| Polly_Retry                   | Retry      |   240.13 ns |  0.357 ns |  0.334 ns |  0.84 |    0.00 | 0.0014 |      24 B |        0.23 |
| CSharpEssentials_RetryPerCall | Retry      | 4,332.14 ns | 18.398 ns | 17.209 ns | 15.08 |    0.06 | 0.4272 |    7184 B |       69.08 |
| Polly_RetryPerCall            | Retry      | 4,324.80 ns | 16.384 ns | 14.524 ns | 15.05 |    0.05 | 0.4196 |    7104 B |       68.31 |
