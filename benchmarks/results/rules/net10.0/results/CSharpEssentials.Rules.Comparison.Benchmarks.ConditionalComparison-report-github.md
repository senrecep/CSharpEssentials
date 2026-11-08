```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **4.404 ns** | **0.0189 ns** | **0.0158 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 125.255 ns | 0.4977 ns | 0.4656 ns | 28.44 |    0.14 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **15.686 ns** | **0.1234 ns** | **0.1154 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 138.210 ns | 0.4957 ns | 0.4637 ns |  8.81 |    0.07 | 0.0057 |      96 B |        1.71 |
