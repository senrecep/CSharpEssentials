```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method         | Keys      | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------- |---------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **AsyncKeyedLock** | **OneKey**    | **392.85 ns** | **6.445 ns** | **5.713 ns** |  **1.00** |    **0.02** | **0.0114** |     **220 B** |        **1.00** |
| Cse            | OneKey    | 412.46 ns | 7.762 ns | 9.241 ns |  1.05 |    0.03 | 0.0153 |     303 B |        1.38 |
| HandWritten    | OneKey    | 231.70 ns | 4.545 ns | 4.251 ns |  0.59 |    0.01 | 0.0038 |      87 B |        0.40 |
|                |           |           |          |          |       |         |        |           |             |
| **AsyncKeyedLock** | **PerWorker** | **412.77 ns** | **2.363 ns** | **2.210 ns** |  **1.00** |    **0.01** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | PerWorker | 383.80 ns | 1.645 ns | 1.458 ns |  0.93 |    0.01 | 0.0095 |     170 B |        3.40 |
| HandWritten    | PerWorker |  18.96 ns | 0.285 ns | 0.266 ns |  0.05 |    0.00 | 0.0001 |       2 B |        0.04 |
|                |           |           |          |          |       |         |        |           |             |
| **AsyncKeyedLock** | **Unique**    | **421.21 ns** | **2.334 ns** | **2.183 ns** |  **1.00** |    **0.01** | **0.0029** |      **50 B** |        **1.00** |
| Cse            | Unique    | 371.93 ns | 1.959 ns | 1.833 ns |  0.88 |    0.01 | 0.0095 |     170 B |        3.40 |
| HandWritten    | Unique    |  23.68 ns | 0.170 ns | 0.159 ns |  0.06 |    0.00 | 0.0001 |       2 B |        0.04 |
