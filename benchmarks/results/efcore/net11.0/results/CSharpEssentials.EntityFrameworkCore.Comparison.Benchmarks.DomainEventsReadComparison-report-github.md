```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                   | Categories | Events | Mean           | Error       | StdDev      | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |----------- |------- |---------------:|------------:|------------:|------:|--------:|--------:|-------:|----------:|------------:|
| **HandWritten_Collect**                      | **Collect1k**  | **0**      |   **3,482.026 ns** |   **1.9128 ns** |   **1.6956 ns** |  **1.00** |    **0.00** |       **-** |      **-** |      **32 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 0      |   4,785.733 ns |  86.1543 ns |  80.5888 ns |  1.37 |    0.02 |       - |      - |      32 B |        1.00 |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **1**      |  **40,135.234 ns** | **327.6983 ns** | **306.5292 ns** |  **1.00** |    **0.01** |  **2.8687** | **0.0610** |   **48600 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 1      |  35,256.158 ns | 122.7465 ns | 114.8171 ns |  0.88 |    0.01 |  2.8687 | 0.0610 |   48600 B |        1.00 |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_Collect**                      | **Collect1k**  | **10**     | **101,777.675 ns** | **301.5155 ns** | **282.0378 ns** |  **1.00** |    **0.00** | **15.9912** | **3.9063** |  **268056 B** |        **1.00** |
| CSharpEssentials_Collect                 | Collect1k  | 10     | 100,576.696 ns | 587.1306 ns | 520.4760 ns |  0.99 |    0.01 | 15.9912 | 3.9063 |  268056 B |        1.00 |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **0**      |       **2.223 ns** |   **0.0087 ns** |   **0.0077 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 0      |       2.611 ns |   0.0124 ns |   0.0116 ns |  1.17 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **1**      |       **9.493 ns** |   **0.0412 ns** |   **0.0385 ns** |  **1.00** |    **0.01** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 1      |       9.359 ns |   0.0377 ns |   0.0353 ns |  0.99 |    0.01 |       - |      - |         - |          NA |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_List**                         | **RaiseClear** | **10**     |      **66.329 ns** |   **0.0384 ns** |   **0.0341 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| CSharpEssentials_EntityBase_RaiseClear   | RaiseClear | 10     |      67.114 ns |   0.1752 ns |   0.1639 ns |  1.01 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **0**      |       **1.687 ns** |   **0.0020 ns** |   **0.0019 ns** |  **1.00** |    **0.00** |       **-** |      **-** |         **-** |          **NA** |
| HandWritten_AsReadOnlyView               | Read       | 0      |       1.517 ns |   0.0038 ns |   0.0035 ns |  0.90 |    0.00 |       - |      - |         - |          NA |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 0      |       2.255 ns |   0.0024 ns |   0.0022 ns |  1.34 |    0.00 |       - |      - |         - |          NA |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **1**      |      **12.899 ns** |   **0.0530 ns** |   **0.0442 ns** |  **1.00** |    **0.00** |  **0.0019** |      **-** |      **32 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 1      |       3.661 ns |   0.0037 ns |   0.0033 ns |  0.28 |    0.00 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 1      |       4.090 ns |   0.0068 ns |   0.0063 ns |  0.32 |    0.00 |       - |      - |         - |        0.00 |
|                                          |            |        |                |             |             |       |         |         |        |           |             |
| **HandWritten_ListToArray**                  | **Read**       | **10**     |      **26.952 ns** |   **0.5516 ns** |   **0.5902 ns** |  **1.00** |    **0.03** |  **0.0062** |      **-** |     **104 B** |        **1.00** |
| HandWritten_AsReadOnlyView               | Read       | 10     |      20.529 ns |   0.0991 ns |   0.0878 ns |  0.76 |    0.02 |       - |      - |         - |        0.00 |
| CSharpEssentials_EntityBase_DomainEvents | Read       | 10     |      20.926 ns |   0.4474 ns |   0.3966 ns |  0.78 |    0.02 |       - |      - |         - |        0.00 |
