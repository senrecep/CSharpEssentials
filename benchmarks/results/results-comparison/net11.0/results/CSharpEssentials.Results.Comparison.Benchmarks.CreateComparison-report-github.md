```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **9.9265 ns** | **0.1208 ns** | **0.1130 ns** | **1.000** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  0.0000 ns | 0.0000 ns | 0.0000 ns | 0.000 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 16.9649 ns | 0.1845 ns | 0.1726 ns | 1.709 |    0.03 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 90.7117 ns | 0.7417 ns | 0.6938 ns | 9.139 |    0.12 | 0.0205 |     344 B |        6.14 |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.2526 ns** | **0.0041 ns** | **0.0036 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  0.0276 ns | 0.0013 ns | 0.0012 ns |  0.11 |    0.00 |      - |         - |          NA |
| ErrorOr                    | True    |  0.2907 ns | 0.0029 ns | 0.0024 ns |  1.15 |    0.02 |      - |         - |          NA |
| FluentResults              | True    | 22.4665 ns | 0.2236 ns | 0.2092 ns | 88.97 |    1.45 | 0.0067 |     112 B |          NA |
