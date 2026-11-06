```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Policy      | 2.882 μs | 0.0104 μs | 0.0092 μs |  1.00 | 0.1526 |   2.73 KB |        1.00 |
| Polly_Pipeline               | 2.452 μs | 0.0186 μs | 0.0174 μs |  0.85 | 0.1373 |   2.27 KB |        0.83 |
| MicrosoftResilience_Handler  | 3.420 μs | 0.0066 μs | 0.0061 μs |  1.19 | 0.1755 |   2.91 KB |        1.07 |
| MicrosoftResilience_Standard | 4.220 μs | 0.0226 μs | 0.0200 μs |  1.46 | 0.1221 |   2.95 KB |        1.08 |
| Factory_NoResilience         | 2.122 μs | 0.0052 μs | 0.0048 μs |  0.74 | 0.1526 |   2.54 KB |        0.93 |
