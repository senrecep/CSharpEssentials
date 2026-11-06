```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method         | Keys      | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------- |---------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **AsyncKeyedLock** | **OneKey**    | **823.92 ns** | **3.964 ns** | **3.708 ns** |  **1.00** | **0.0134** |     **225 B** |        **1.00** |
| Cse            | OneKey    | 825.16 ns | 6.379 ns | 5.967 ns |  1.00 | 0.0172 |     305 B |        1.36 |
| HandWritten    | OneKey    | 599.10 ns | 2.585 ns | 2.418 ns |  0.73 | 0.0048 |      89 B |        0.40 |
|                |           |           |          |          |       |        |           |             |
| **AsyncKeyedLock** | **PerWorker** | **362.83 ns** | **4.038 ns** | **3.777 ns** |  **1.00** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | PerWorker | 370.77 ns | 2.398 ns | 2.243 ns |  1.02 | 0.0100 |     170 B |        3.40 |
| HandWritten    | PerWorker |  19.03 ns | 0.265 ns | 0.221 ns |  0.05 | 0.0001 |       2 B |        0.04 |
|                |           |           |          |          |       |        |           |             |
| **AsyncKeyedLock** | **Unique**    | **349.52 ns** | **2.569 ns** | **2.403 ns** |  **1.00** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | Unique    | 352.11 ns | 1.312 ns | 1.163 ns |  1.01 | 0.0100 |     170 B |        3.40 |
| HandWritten    | Unique    |  17.00 ns | 0.332 ns | 0.311 ns |  0.05 | 0.0001 |       2 B |        0.04 |
