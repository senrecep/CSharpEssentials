```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **2.242 ns** | **0.0034 ns** | **0.0030 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 115.564 ns | 0.5695 ns | 0.5327 ns | 51.54 |    0.24 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **10.626 ns** | **0.0922 ns** | **0.0862 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 125.684 ns | 0.5932 ns | 0.4953 ns | 11.83 |    0.10 | 0.0057 |      96 B |        1.71 |
