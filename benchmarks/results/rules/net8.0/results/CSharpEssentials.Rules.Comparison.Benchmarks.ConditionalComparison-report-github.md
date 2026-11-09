```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **4.511 ns** | **0.0190 ns** | **0.0178 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 119.601 ns | 0.3041 ns | 0.2696 ns | 26.51 |    0.12 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **18.887 ns** | **0.1329 ns** | **0.1243 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 129.958 ns | 0.6405 ns | 0.5677 ns |  6.88 |    0.05 | 0.0057 |      96 B |        1.71 |
