```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **2.256 ns** | **0.0011 ns** | **0.0010 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 113.496 ns | 0.1915 ns | 0.1599 ns | 50.31 |    0.07 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **11.396 ns** | **0.1336 ns** | **0.1250 ns** |  **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 125.511 ns | 0.4028 ns | 0.3768 ns | 11.01 |    0.12 | 0.0057 |      96 B |        1.71 |
