```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method      | Input    | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |  **2.242 ns** | **0.0047 ns** | **0.0037 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 73.909 ns | 0.2825 ns | 0.2505 ns | 32.97 |    0.12 | 0.0024 |      40 B |          NA |
|             |          |           |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** | **11.300 ns** | **0.2502 ns** | **0.3340 ns** |  **1.00** |    **0.04** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 81.990 ns | 0.4243 ns | 0.3969 ns |  7.26 |    0.22 | 0.0057 |      96 B |        1.71 |
