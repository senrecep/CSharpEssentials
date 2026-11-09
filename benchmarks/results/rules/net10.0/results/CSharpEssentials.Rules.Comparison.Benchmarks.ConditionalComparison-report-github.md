```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method      | Input    | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |  **4.354 ns** | **0.0102 ns** | **0.0095 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 80.601 ns | 0.2208 ns | 0.1958 ns | 18.51 |    0.06 | 0.0024 |      40 B |          NA |
|             |          |           |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** | **15.857 ns** | **0.0740 ns** | **0.0656 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 84.393 ns | 0.3520 ns | 0.3292 ns |  5.32 |    0.03 | 0.0057 |      96 B |        1.71 |
