```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                            | Count | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|---------------------------------- |------ |----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |  **6.586 ms** | **0.0621 ms** | **0.0551 ms** |  **1.00** |    **0.01** |  **187.5000** |   **93.7500** |        **-** |      **3 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  6.832 ms | 0.0957 ms | 0.0848 ms |  1.04 |    0.01 |  187.5000 |   62.5000 |        - |   3.12 MB |        1.04 |
|                                   |       |           |           |           |       |         |           |           |          |           |             |
| **HandWritten_DictionaryDiff**        | **10000** | **78.564 ms** | **1.5199 ms** | **2.2278 ms** |  **1.00** |    **0.04** | **1500.0000** | **1000.0000** | **500.0000** |  **29.06 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 82.540 ms | 1.3688 ms | 1.5214 ms |  1.05 |    0.03 | 1500.0000 | 1000.0000 | 500.0000 |  30.48 MB |        1.05 |
