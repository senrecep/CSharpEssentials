```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                   | Categories | Events | Mean           | Error         | StdDev        | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |----------- |------- |---------------:|--------------:|--------------:|------:|--------:|--------:|-------:|----------:|------------:|
| **HandWritten_Collect**                      | **Collect1k**  | **0**      |   **3,273.520 ns** |    **33.9330 ns** |    **31.7409 ns** |  **1.00** |    **0.01** |       **-** |      **-** |      **32 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 0      |   4,830.394 ns |    11.6660 ns |    10.9124 ns |  1.48 |    0.01 |       - |      - |      32 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **1**      |  **45,216.923 ns** |   **775.3602 ns** |   **725.2724 ns** |  **1.00** |    **0.02** |  **2.8687** | **0.0610** |   **48600 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 1      |  39,632.000 ns |   495.6368 ns |   463.6190 ns |  0.88 |    0.02 |  2.8687 | 0.0610 |   48600 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **10**     | **114,461.980 ns** | **2,213.8710 ns** | **2,368.8171 ns** |  **1.00** |    **0.03** | **15.9912** | **3.9063** |  **268056 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 10     | 114,706.463 ns | 1,558.0249 ns | 1,381.1485 ns |  1.00 |    0.02 | 15.9912 | 3.9063 |  268056 B |        1.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **0**      |       **2.259 ns** |     **0.0127 ns** |     **0.0112 ns** |  **1.00** |    **0.01** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 0      |       2.655 ns |     0.0324 ns |     0.0287 ns |  1.18 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **1**      |      **11.293 ns** |     **0.0355 ns** |     **0.0332 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 1      |       9.920 ns |     0.0160 ns |     0.0142 ns |  0.88 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **10**     |      **73.591 ns** |     **0.0993 ns** |     **0.0775 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 10     |      63.274 ns |     0.0643 ns |     0.0502 ns |  0.86 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **0**      |       **1.689 ns** |     **0.0019 ns** |     **0.0016 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| HandWritten_AsReadOnlyView               | Read       | 0      |       1.519 ns |     0.0040 ns |     0.0035 ns |  0.90 |    0.00 |       - |      - |         - |          NA |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 0      |       2.256 ns |     0.0041 ns |     0.0036 ns |  1.34 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **1**      |      **14.270 ns** |     **0.0533 ns** |     **0.0445 ns** |  **1.00** |    **0.00** |  **0.0019** |      **-** |      **32 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 1      |       3.585 ns |     0.0107 ns |     0.0100 ns |  0.25 |    0.00 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 1      |       4.163 ns |     0.0059 ns |     0.0046 ns |  0.29 |    0.00 |       - |      - |         - |        0.00 |
|                                          |            |        |                |               |               |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **10**     |      **26.181 ns** |     **0.1881 ns** |     **0.1759 ns** |  **1.00** |    **0.01** |  **0.0062** |      **-** |     **104 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 10     |      20.517 ns |     0.0754 ns |     0.0629 ns |  0.78 |    0.01 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 10     |      21.540 ns |     0.0466 ns |     0.0389 ns |  0.82 |    0.01 |       - |      - |         - |        0.00 |
