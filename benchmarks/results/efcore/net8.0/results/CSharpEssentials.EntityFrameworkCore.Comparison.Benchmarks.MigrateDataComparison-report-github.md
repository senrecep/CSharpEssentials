```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                            | Count | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|---------------------------------- |------ |----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |  **7.033 ms** | **0.0366 ms** | **0.0306 ms** |  **1.00** |    **0.01** |  **187.5000** |   **62.5000** |        **-** |   **3.04 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  7.302 ms | 0.0501 ms | 0.0469 ms |  1.04 |    0.01 |  187.5000 |   62.5000 |        - |   3.16 MB |        1.04 |
|                                   |       |           |           |           |       |         |           |           |          |           |             |
| **HandWritten_DictionaryDiff**        | **10000** | **87.142 ms** | **1.7286 ms** | **2.3076 ms** |  **1.00** |    **0.04** | **1500.0000** | **1000.0000** | **500.0000** |  **29.45 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 88.815 ms | 1.6834 ms | 1.4923 ms |  1.02 |    0.03 | 1500.0000 | 1000.0000 | 500.0000 |   30.9 MB |        1.05 |
