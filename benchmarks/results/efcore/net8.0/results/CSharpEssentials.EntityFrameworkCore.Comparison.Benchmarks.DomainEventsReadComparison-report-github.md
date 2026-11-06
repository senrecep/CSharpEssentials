```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                   | Categories | Events | Mean           | Error         | StdDev        | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |----------- |------- |---------------:|--------------:|--------------:|------:|--------:|--------:|-------:|----------:|------------:|
| **HandWritten_Collect**                      | **Collect1k**  | **0**      |  **10,516.232 ns** |     **7.2784 ns** |     **6.8083 ns** |  **1.00** |    **0.00** |       **-** |      **-** |      **32 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 0      |   8,535.421 ns |     7.6823 ns |     6.8102 ns |  0.81 |    0.00 |       - |      - |      32 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **1**      |  **45,092.678 ns** |   **160.7432 ns** |   **134.2279 ns** |  **1.00** |    **0.00** |  **2.8687** | **0.0610** |   **48600 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 1      |  55,502.522 ns |   228.3181 ns |   213.5689 ns |  1.23 |    0.01 |  2.8687 | 0.0610 |   48600 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **10**     | **123,528.515 ns** |   **471.9037 ns** |   **441.4190 ns** |  **1.00** |    **0.00** | **15.8691** | **3.9063** |  **268056 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 10     | 137,731.792 ns | 2,379.0043 ns | 2,225.3221 ns |  1.11 |    0.02 | 15.8691 | 3.9063 |  268056 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **0**      |       **2.304 ns** |     **0.0176 ns** |     **0.0156 ns** |  **1.00** |    **0.01** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 0      |       2.111 ns |     0.0297 ns |     0.0278 ns |  0.92 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **1**      |      **11.441 ns** |     **0.0631 ns** |     **0.0590 ns** |  **1.00** |    **0.01** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 1      |      12.430 ns |     0.0803 ns |     0.0711 ns |  1.09 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **10**     |      **63.929 ns** |     **0.3884 ns** |     **0.3633 ns** |  **1.00** |    **0.01** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 10     |      64.758 ns |     0.1666 ns |     0.1300 ns |  1.01 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **0**      |       **4.054 ns** |     **0.0153 ns** |     **0.0135 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| HandWritten_AsReadOnlyView               | Read       | 0      |       2.349 ns |     0.0357 ns |     0.0334 ns |  0.58 |    0.01 |       - |      - |         - |          NA |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 0      |       2.896 ns |     0.0189 ns |     0.0177 ns |  0.71 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **1**      |      **23.201 ns** |     **0.3043 ns** |     **0.2847 ns** |  **1.00** |    **0.02** |  **0.0019** |      **-** |      **32 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 1      |       7.827 ns |     0.1402 ns |     0.1312 ns |  0.34 |    0.01 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 1      |       7.581 ns |     0.0656 ns |     0.0614 ns |  0.33 |    0.00 |       - |      - |         - |        0.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **10**     |      **74.813 ns** |     **0.3673 ns** |     **0.3435 ns** |  **1.00** |    **0.01** |  **0.0062** |      **-** |     **104 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 10     |      41.108 ns |     0.5654 ns |     0.5288 ns |  0.55 |    0.01 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 10     |      41.616 ns |     0.5972 ns |     0.5586 ns |  0.56 |    0.01 |       - |      - |         - |        0.00 |
