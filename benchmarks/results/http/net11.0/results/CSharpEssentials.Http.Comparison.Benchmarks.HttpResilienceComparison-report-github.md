```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 2.764 μs | 0.0217 μs | 0.0203 μs |  1.00 | 0.1526 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 2.362 μs | 0.0191 μs | 0.0169 μs |  0.85 | 0.1221 |   2.28 KB |        0.83 |
| MicrosoftResilience_Handler  | 3.146 μs | 0.0285 μs | 0.0267 μs |  1.14 | 0.1526 |   2.92 KB |        1.07 |
| MicrosoftResilience_Standard | 3.931 μs | 0.0277 μs | 0.0246 μs |  1.42 | 0.1221 |   2.96 KB |        1.08 |
| Factory_NoResilience         | 1.919 μs | 0.0140 μs | 0.0131 μs |  0.69 | 0.1526 |   2.55 KB |        0.93 |
