```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                            | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|---------------------------------- |------ |-----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| **HandWritten_DictionaryDiff**        | **1000**  |   **6.095 ms** | **0.0540 ms** | **0.0841 ms** |  **1.00** |    **0.02** |  **125.0000** |   **62.5000** |        **-** |   **2.94 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 1000  |  13.748 ms | 0.2508 ms | 0.2223 ms |  2.26 |    0.05 |  142.8571 |         - |        - |    3.1 MB |        1.06 |
|                                   |       |            |           |           |       |         |           |           |          |           |             |
| **HandWritten_DictionaryDiff**        | **10000** |  **74.779 ms** | **0.4862 ms** | **0.4548 ms** |  **1.00** |    **0.01** | **2000.0000** | **1285.7143** | **571.4286** |  **28.39 MB** |        **1.00** |
| CSharpEssentials_MigrateDataAsync | 10000 | 809.988 ms | 3.1580 ms | 2.7994 ms | 10.83 |    0.07 | 1000.0000 |         - |        - |  30.21 MB |        1.06 |
