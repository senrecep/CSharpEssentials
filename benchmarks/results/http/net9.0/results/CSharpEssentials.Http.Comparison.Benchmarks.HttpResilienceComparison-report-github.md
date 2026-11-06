```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 3.413 μs | 0.0533 μs | 0.0498 μs |  1.00 |    0.02 | 0.1640 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 2.861 μs | 0.0083 μs | 0.0074 μs |  0.84 |    0.01 | 0.1373 |   2.26 KB |        0.83 |
| MicrosoftResilience_Handler  | 3.880 μs | 0.0339 μs | 0.0317 μs |  1.14 |    0.02 | 0.1755 |    2.9 KB |        1.06 |
| MicrosoftResilience_Standard | 4.778 μs | 0.0206 μs | 0.0172 μs |  1.40 |    0.02 | 0.1221 |   2.94 KB |        1.08 |
| Factory_NoResilience         | 2.408 μs | 0.0157 μs | 0.0147 μs |  0.71 |    0.01 | 0.1526 |   2.52 KB |        0.93 |
