```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                            | Count | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1    | Allocated | Alloc Ratio |
|---------------------------------- |------ |----------:|----------:|----------:|------:|--------:|----------:|--------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |  **5.803 ms** | **0.0944 ms** | **0.1496 ms** |  **1.00** |    **0.03** |  **125.0000** | **62.5000** |   **2.94 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  6.181 ms | 0.0876 ms | 0.1256 ms |  1.07 |    0.03 |  187.5000 | 62.5000 |   3.07 MB |        1.05 |
|                                   |       |           |           |           |       |         |           |         |           |             |
| **HandWritten_DictionaryDiff**        | **10000** | **69.880 ms** | **1.2074 ms** | **1.8798 ms** |  **1.00** |    **0.04** | **1000.0000** |       **-** |  **28.49 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 72.614 ms | 1.3253 ms | 1.1067 ms |  1.04 |    0.03 | 1000.0000 |       - |  29.89 MB |        1.05 |
