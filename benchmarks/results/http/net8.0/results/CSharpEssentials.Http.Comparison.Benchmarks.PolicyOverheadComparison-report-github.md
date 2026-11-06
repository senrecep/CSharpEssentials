```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Direct_Await                  | Empty      |    17.28 ns |  0.172 ns |  0.161 ns |  0.18 |    0.00 | 0.0043 |      72 B |        0.90 |
| Polly_Empty                   | Empty      |    82.33 ns |  0.208 ns |  0.194 ns |  0.83 |    0.00 |      - |         - |        0.00 |
| CSharpEssentials_Empty        | Empty      |    98.71 ns |  0.149 ns |  0.139 ns |  1.00 |    0.00 | 0.0048 |      80 B |        1.00 |
|                               |            |             |           |           |       |         |        |           |             |
| CSharpEssentials_Retry        | Retry      |   345.90 ns |  0.321 ns |  0.268 ns |  1.00 |    0.00 | 0.0062 |     104 B |        1.00 |
| Polly_Retry                   | Retry      |   326.63 ns |  0.230 ns |  0.203 ns |  0.94 |    0.00 | 0.0014 |      24 B |        0.23 |
| CSharpEssentials_RetryPerCall | Retry      | 4,967.01 ns |  9.176 ns |  7.663 ns | 14.36 |    0.02 | 0.4807 |    8136 B |       78.23 |
| Polly_RetryPerCall            | Retry      | 4,888.51 ns | 12.080 ns | 11.300 ns | 14.13 |    0.03 | 0.4807 |    8056 B |       77.46 |
