```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method      | Input    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |--------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten** | **Pass**     |   **4.635 ns** | **0.0068 ns** | **0.0053 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseIf       | Pass     | 152.083 ns | 0.0708 ns | 0.0553 ns | 32.81 |    0.04 | 0.0024 |      40 B |          NA |
|             |          |            |           |           |       |         |        |           |             |
| **HandWritten** | **FailLast** |  **16.835 ns** | **0.0589 ns** | **0.0492 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| CseIf       | FailLast | 166.029 ns | 0.2334 ns | 0.2183 ns |  9.86 |    0.03 | 0.0057 |      96 B |        1.71 |
