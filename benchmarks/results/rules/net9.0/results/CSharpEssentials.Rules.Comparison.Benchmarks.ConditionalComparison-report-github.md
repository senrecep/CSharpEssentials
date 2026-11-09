```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **4.649 ns** | **0.0096 ns** | **0.0089 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 100.531 ns | 0.2659 ns | 0.2487 ns | 21.62 |    0.07 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **16.321 ns** | **0.1255 ns** | **0.1174 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 114.675 ns | 0.5706 ns | 0.5337 ns |  7.03 |    0.06 | 0.0057 |      96 B |        1.71 |
