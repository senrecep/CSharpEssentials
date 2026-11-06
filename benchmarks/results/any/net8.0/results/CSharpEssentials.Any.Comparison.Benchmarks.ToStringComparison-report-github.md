```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                    | Categories | Branch | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|-------------------------- |----------- |------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
| **CSharpEssentials_ToString** | **Cse**        | **Int**    |  **2.8552 ns** | **0.0066 ns** | **0.0058 ns** |  **2.8525 ns** |      **-** |         **-** |
| **CSharpEssentials_ToString** | **Cse**        | **Text**   |  **0.6192 ns** | **0.0075 ns** | **0.0066 ns** |  **0.6170 ns** |      **-** |         **-** |
|                           |            |        |            |           |           |            |        |           |
| **Dunet_ToString**            | **Dunet**      | **Int**    | **65.5996 ns** | **1.2981 ns** | **3.3969 ns** | **64.3853 ns** | **0.0162** |     **272 B** |
| **Dunet_ToString**            | **Dunet**      | **Text**   | **66.0724 ns** | **1.2948 ns** | **1.0812 ns** | **65.6242 ns** | **0.0172** |     **288 B** |
|                           |            |        |            |           |           |            |        |           |
| **OneOf_ToString**            | **OneOf**      | **Int**    | **43.1015 ns** | **0.7784 ns** | **0.7281 ns** | **42.8884 ns** | **0.0033** |      **56 B** |
| **OneOf_ToString**            | **OneOf**      | **Text**   | **42.7468 ns** | **0.3699 ns** | **0.3089 ns** | **42.6997 ns** | **0.0043** |      **72 B** |
