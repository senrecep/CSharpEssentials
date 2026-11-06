```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                   | Categories | Events | Mean           | Error         | StdDev        | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |----------- |------- |---------------:|--------------:|--------------:|------:|--------:|--------:|-------:|----------:|------------:|
| **HandWritten_Collect**                      | **Collect1k**  | **0**      |   **9,879.891 ns** |     **7.7267 ns** |     **6.4521 ns** |  **1.00** |    **0.00** |       **-** |      **-** |      **32 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 0      |   7,243.147 ns |     2.3229 ns |     1.9397 ns |  0.73 |    0.00 |       - |      - |      32 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **1**      |  **45,567.657 ns** |    **73.8868 ns** |    **65.4987 ns** |  **1.00** |    **0.00** |  **2.8687** | **0.0610** |   **48600 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 1      |  54,012.410 ns | 1,014.7967 ns |   847.4012 ns |  1.19 |    0.02 |  2.8687 | 0.0610 |   48600 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **10**     | **121,972.929 ns** | **2,079.1098 ns** | **1,944.8006 ns** |  **1.00** |    **0.02** | **15.8691** | **3.9063** |  **268056 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 10     | 125,176.648 ns |   559.9120 ns |   523.7420 ns |  1.03 |    0.02 | 15.8691 | 3.9063 |  268056 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **0**      |       **2.299 ns** |     **0.0238 ns** |     **0.0211 ns** |  **1.00** |    **0.01** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 0      |       2.040 ns |     0.0147 ns |     0.0130 ns |  0.89 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **1**      |      **11.612 ns** |     **0.0231 ns** |     **0.0216 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 1      |      12.355 ns |     0.0758 ns |     0.0672 ns |  1.06 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **10**     |      **67.319 ns** |     **0.0421 ns** |     **0.0351 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 10     |      67.266 ns |     0.0367 ns |     0.0307 ns |  1.00 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **0**      |       **3.375 ns** |     **0.0016 ns** |     **0.0014 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| HandWritten_AsReadOnlyView               | Read       | 0      |       2.310 ns |     0.0009 ns |     0.0008 ns |  0.68 |    0.00 |       - |      - |         - |          NA |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 0      |       2.889 ns |     0.0081 ns |     0.0072 ns |  0.86 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **1**      |      **24.048 ns** |     **0.1033 ns** |     **0.0966 ns** |  **1.00** |    **0.01** |  **0.0019** |      **-** |      **32 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 1      |       6.927 ns |     0.0465 ns |     0.0412 ns |  0.29 |    0.00 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 1      |       7.073 ns |     0.0546 ns |     0.0456 ns |  0.29 |    0.00 |       - |      - |         - |        0.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **10**     |      **79.716 ns** |     **0.7383 ns** |     **0.6906 ns** |  **1.00** |    **0.01** |  **0.0062** |      **-** |     **104 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 10     |      40.591 ns |     0.4454 ns |     0.4166 ns |  0.51 |    0.01 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 10     |      41.033 ns |     0.3311 ns |     0.3097 ns |  0.51 |    0.01 |       - |      - |         - |        0.00 |
