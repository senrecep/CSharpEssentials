```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 4.117 μs | 0.0081 μs | 0.0072 μs |  1.00 | 0.1602 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 3.260 μs | 0.0096 μs | 0.0090 μs |  0.79 | 0.1373 |   2.26 KB |        0.83 |
| MicrosoftResilience_Handler  | 4.135 μs | 0.0056 μs | 0.0044 μs |  1.00 | 0.1678 |   2.79 KB |        1.02 |
| MicrosoftResilience_Standard | 5.137 μs | 0.0112 μs | 0.0100 μs |  1.25 | 0.1678 |   2.83 KB |        1.04 |
| Factory_NoResilience         | 2.381 μs | 0.0051 μs | 0.0048 μs |  0.58 | 0.1450 |   2.41 KB |        0.89 |
