```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                            | Count | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1    | Allocated | Alloc Ratio |
|---------------------------------- |------ |----------:|----------:|----------:|------:|--------:|----------:|--------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |  **6.109 ms** | **0.0975 ms** | **0.0912 ms** |  **1.00** |    **0.02** |  **125.0000** | **62.5000** |   **2.94 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  6.208 ms | 0.0804 ms | 0.0627 ms |  1.02 |    0.02 |  187.5000 | 62.5000 |   3.06 MB |        1.04 |
|                                   |       |           |           |           |       |         |           |         |           |             |
| **HandWritten_DictionaryDiff**        | **10000** | **70.924 ms** | **1.1299 ms** | **1.9491 ms** |  **1.00** |    **0.04** | **1000.0000** |       **-** |  **28.53 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 73.432 ms | 0.9104 ms | 0.7107 ms |  1.04 |    0.03 | 1000.0000 |       - |  29.93 MB |        1.05 |
