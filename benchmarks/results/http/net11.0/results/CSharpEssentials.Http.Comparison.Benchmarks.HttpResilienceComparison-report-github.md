```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 2.901 μs | 0.0143 μs | 0.0127 μs |  1.00 | 0.1526 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 2.378 μs | 0.0083 μs | 0.0074 μs |  0.82 | 0.1221 |   2.28 KB |        0.83 |
| MicrosoftResilience_Handler  | 3.172 μs | 0.0088 μs | 0.0074 μs |  1.09 | 0.1526 |   2.92 KB |        1.07 |
| MicrosoftResilience_Standard | 3.897 μs | 0.0150 μs | 0.0125 μs |  1.34 | 0.1221 |   2.96 KB |        1.08 |
| Factory_NoResilience         | 1.990 μs | 0.0080 μs | 0.0071 μs |  0.69 | 0.1526 |   2.55 KB |        0.93 |
