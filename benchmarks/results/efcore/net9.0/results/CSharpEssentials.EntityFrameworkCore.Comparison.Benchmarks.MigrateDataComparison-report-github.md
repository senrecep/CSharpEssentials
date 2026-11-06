```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                            | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|---------------------------------- |------ |-----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |   **6.932 ms** | **0.1319 ms** | **0.1101 ms** |  **1.00** |    **0.02** |  **187.5000** |   **62.5000** |        **-** |      **3 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  15.327 ms | 0.3038 ms | 0.3842 ms |  2.21 |    0.06 |  166.6667 |         - |        - |   3.16 MB |        1.06 |
|                                   |       |            |           |           |       |         |           |           |          |           |             |
| **HandWritten_DictionaryDiff**        | **10000** |  **81.884 ms** | **0.5402 ms** | **0.4511 ms** |  **1.00** |    **0.01** | **2166.6667** | **1333.3333** | **666.6667** |  **29.06 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 956.951 ms | 5.9745 ms | 5.5886 ms | 11.69 |    0.09 | 1000.0000 |         - |        - |  31.03 MB |        1.07 |
