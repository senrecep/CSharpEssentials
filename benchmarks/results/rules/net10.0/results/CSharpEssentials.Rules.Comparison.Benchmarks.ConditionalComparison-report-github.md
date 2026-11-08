```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method      | Input    | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |  **5.174 ns** | **0.0171 ns** | **0.0143 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 72.622 ns | 0.2544 ns | 0.2380 ns | 14.04 |    0.06 | 0.0024 |      40 B |          NA |
|             |          |           |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** | **16.010 ns** | **0.0731 ns** | **0.0684 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 84.361 ns | 1.3255 ns | 1.2399 ns |  5.27 |    0.08 | 0.0057 |      96 B |        1.71 |
