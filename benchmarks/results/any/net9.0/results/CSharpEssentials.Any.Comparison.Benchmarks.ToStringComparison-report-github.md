```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                    | Categories | Branch | Mean       | Error     | StdDev    | Gen0   | Allocated |
|-------------------------- |----------- |------- |-----------:|----------:|----------:|-------:|----------:|
| **CSharpEssentials_ToString** | **Cse**        | **Int**    |  **2.2845 ns** | **0.0138 ns** | **0.0122 ns** |      **-** |         **-** |
| **CSharpEssentials_ToString** | **Cse**        | **Text**   |  **0.6073 ns** | **0.0279 ns** | **0.0261 ns** |      **-** |         **-** |
|                           |            |        |            |           |           |        |           |
| **Dunet_ToString**            | **Dunet**      | **Int**    | **61.7918 ns** | **0.4300 ns** | **0.3357 ns** | **0.0162** |     **272 B** |
| **Dunet_ToString**            | **Dunet**      | **Text**   | **63.5648 ns** | **1.2553 ns** | **1.1128 ns** | **0.0172** |     **288 B** |
|                           |            |        |            |           |           |        |           |
| **OneOf_ToString**            | **OneOf**      | **Int**    | **38.7656 ns** | **0.3002 ns** | **0.2661 ns** | **0.0033** |      **56 B** |
| **OneOf_ToString**            | **OneOf**      | **Text**   | **38.9631 ns** | **0.4935 ns** | **0.4121 ns** | **0.0043** |      **72 B** |
