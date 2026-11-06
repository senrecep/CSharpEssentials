```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method         | Keys      | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------- |---------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **AsyncKeyedLock** | **OneKey**    | **786.49 ns** | **3.327 ns** | **2.949 ns** |  **1.00** | **0.0134** |     **222 B** |        **1.00** |
| Cse            | OneKey    | 798.83 ns | 4.644 ns | 4.344 ns |  1.02 | 0.0172 |     305 B |        1.37 |
| HandWritten    | OneKey    | 621.28 ns | 3.763 ns | 3.520 ns |  0.79 | 0.0048 |      89 B |        0.40 |
|                |           |           |          |          |       |        |           |             |
| **AsyncKeyedLock** | **PerWorker** | **417.10 ns** | **3.905 ns** | **3.652 ns** |  **1.00** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | PerWorker | 366.75 ns | 1.596 ns | 1.493 ns |  0.88 | 0.0100 |     170 B |        3.40 |
| HandWritten    | PerWorker |  14.69 ns | 0.261 ns | 0.245 ns |  0.04 | 0.0001 |       2 B |        0.04 |
|                |           |           |          |          |       |        |           |             |
| **AsyncKeyedLock** | **Unique**    | **421.98 ns** | **1.904 ns** | **1.688 ns** |  **1.00** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | Unique    | 344.41 ns | 1.538 ns | 1.363 ns |  0.82 | 0.0100 |     170 B |        3.40 |
| HandWritten    | Unique    |  16.60 ns | 0.176 ns | 0.156 ns |  0.04 | 0.0001 |       2 B |        0.04 |
