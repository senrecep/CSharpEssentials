```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                            | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|---------------------------------- |------ |-----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |   **6.044 ms** | **0.0630 ms** | **0.0863 ms** |  **1.00** |    **0.02** |  **125.0000** |   **62.5000** |        **-** |   **2.94 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  14.028 ms | 0.1854 ms | 0.1644 ms |  2.32 |    0.04 |  166.6667 |         - |        - |   3.11 MB |        1.06 |
|                                   |       |            |           |           |       |         |           |           |          |           |             |
| **HandWritten_DictionaryDiff**        | **10000** |  **76.195 ms** | **0.4272 ms** | **0.3787 ms** |  **1.00** |    **0.01** | **2000.0000** | **1285.7143** | **571.4286** |  **28.35 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 835.763 ms | 9.4885 ms | 8.4113 ms | 10.97 |    0.12 | 1000.0000 |         - |        - |  30.49 MB |        1.08 |
