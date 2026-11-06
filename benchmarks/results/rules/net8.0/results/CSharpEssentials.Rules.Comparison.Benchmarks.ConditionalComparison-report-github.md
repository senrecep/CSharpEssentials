```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **4.484 ns** | **0.0040 ns** | **0.0035 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 177.246 ns | 0.2138 ns | 0.1895 ns | 39.53 |    0.05 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **17.267 ns** | **0.3107 ns** | **0.2906 ns** |  **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 191.622 ns | 0.3830 ns | 0.3583 ns | 11.10 |    0.18 | 0.0057 |      96 B |        1.71 |
