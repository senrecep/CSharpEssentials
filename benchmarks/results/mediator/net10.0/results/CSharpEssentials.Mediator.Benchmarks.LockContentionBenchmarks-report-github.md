```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method         | Keys      | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------- |---------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **AsyncKeyedLock** | **OneKey**    | **772.06 ns** | **4.217 ns** | **3.738 ns** |  **1.00** | **0.0134** |     **226 B** |        **1.00** |
| Cse            | OneKey    | 779.29 ns | 3.475 ns | 3.250 ns |  1.01 | 0.0172 |     305 B |        1.35 |
| HandWritten    | OneKey    | 576.49 ns | 2.065 ns | 1.932 ns |  0.75 | 0.0048 |      89 B |        0.39 |
|                |           |           |          |          |       |        |           |             |
| **AsyncKeyedLock** | **PerWorker** | **409.75 ns** | **2.975 ns** | **2.782 ns** |  **1.00** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | PerWorker | 340.94 ns | 1.614 ns | 1.510 ns |  0.83 | 0.0100 |     170 B |        3.40 |
| HandWritten    | PerWorker |  14.26 ns | 0.256 ns | 0.239 ns |  0.03 | 0.0001 |       2 B |        0.04 |
|                |           |           |          |          |       |        |           |             |
| **AsyncKeyedLock** | **Unique**    | **391.88 ns** | **2.563 ns** | **2.140 ns** |  **1.00** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | Unique    | 338.65 ns | 4.566 ns | 3.565 ns |  0.86 | 0.0100 |     170 B |        3.40 |
| HandWritten    | Unique    |  16.41 ns | 0.314 ns | 0.294 ns |  0.04 | 0.0001 |       2 B |        0.04 |
