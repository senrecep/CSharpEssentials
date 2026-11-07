```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 3.406 μs | 0.0103 μs | 0.0097 μs |  1.00 | 0.1640 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 2.779 μs | 0.0123 μs | 0.0115 μs |  0.82 | 0.1373 |   2.26 KB |        0.83 |
| MicrosoftResilience_Handler  | 3.784 μs | 0.0128 μs | 0.0114 μs |  1.11 | 0.1755 |    2.9 KB |        1.06 |
| MicrosoftResilience_Standard | 4.756 μs | 0.0260 μs | 0.0243 μs |  1.40 | 0.1755 |   2.94 KB |        1.08 |
| Factory_NoResilience         | 2.377 μs | 0.0145 μs | 0.0135 μs |  0.70 | 0.1526 |   2.52 KB |        0.93 |
