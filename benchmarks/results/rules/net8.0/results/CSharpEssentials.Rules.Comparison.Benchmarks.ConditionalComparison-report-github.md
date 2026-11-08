```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **4.501 ns** | **0.0110 ns** | **0.0097 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 117.543 ns | 0.4671 ns | 0.4369 ns | 26.11 |    0.11 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **18.248 ns** | **0.3283 ns** | **0.2910 ns** |  **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 132.820 ns | 0.7609 ns | 0.7117 ns |  7.28 |    0.12 | 0.0057 |      96 B |        1.71 |
