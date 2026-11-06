```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                            | Count | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|---------------------------------- |------ |-------------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |     **7.310 ms** | **0.1059 ms** | **0.0990 ms** |  **1.00** |    **0.02** |  **187.5000** |   **62.5000** |        **-** |   **3.04 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |    25.221 ms | 0.2017 ms | 0.1788 ms |  3.45 |    0.05 |  142.8571 |         - |        - |   3.28 MB |        1.08 |
|                                   |       |              |           |           |       |         |           |           |          |           |             |
| **HandWritten_DictionaryDiff**        | **10000** |    **87.743 ms** | **1.2040 ms** | **1.1262 ms** |  **1.00** |    **0.02** | **2166.6667** | **1666.6667** | **666.6667** |  **29.45 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 1,852.582 ms | 8.1294 ms | 7.6043 ms | 21.12 |    0.27 | 1000.0000 |         - |        - |  32.21 MB |        1.09 |
