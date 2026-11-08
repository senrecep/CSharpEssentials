```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 2.914 μs | 0.0216 μs | 0.0192 μs |  1.00 | 0.1526 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 2.451 μs | 0.0116 μs | 0.0108 μs |  0.84 | 0.1373 |   2.27 KB |        0.83 |
| MicrosoftResilience_Handler  | 3.246 μs | 0.0101 μs | 0.0094 μs |  1.11 | 0.1755 |   2.91 KB |        1.07 |
| MicrosoftResilience_Standard | 4.052 μs | 0.0204 μs | 0.0191 μs |  1.39 | 0.1755 |   2.95 KB |        1.08 |
| Factory_NoResilience         | 2.050 μs | 0.0070 μs | 0.0062 μs |  0.70 | 0.1526 |   2.54 KB |        0.93 |
