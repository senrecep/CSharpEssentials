```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 4.189 μs | 0.0206 μs | 0.0182 μs |  1.00 | 0.1602 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 3.256 μs | 0.0121 μs | 0.0107 μs |  0.78 | 0.1373 |   2.26 KB |        0.83 |
| MicrosoftResilience_Handler  | 4.145 μs | 0.0153 μs | 0.0143 μs |  0.99 | 0.1678 |   2.79 KB |        1.02 |
| MicrosoftResilience_Standard | 5.172 μs | 0.0372 μs | 0.0348 μs |  1.23 | 0.1678 |   2.83 KB |        1.04 |
| Factory_NoResilience         | 2.421 μs | 0.0110 μs | 0.0103 μs |  0.58 | 0.1450 |   2.41 KB |        0.89 |
