```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                    | Categories | Branch | Mean       | Error     | StdDev    | Gen0   | Allocated |
|-------------------------- |----------- |------- |-----------:|----------:|----------:|-------:|----------:|
| **CSharpEssentials_ToString** | **Cse**        | **Int**    |  **1.9939 ns** | **0.0156 ns** | **0.0146 ns** |      **-** |         **-** |
| **CSharpEssentials_ToString** | **Cse**        | **Text**   |  **0.5387 ns** | **0.0067 ns** | **0.0056 ns** |      **-** |         **-** |
|                           |            |        |            |           |           |        |           |
| **Dunet_ToString**            | **Dunet**      | **Int**    | **60.3165 ns** | **1.2468 ns** | **1.1053 ns** | **0.0162** |     **272 B** |
| **Dunet_ToString**            | **Dunet**      | **Text**   | **59.0311 ns** | **0.8682 ns** | **0.7696 ns** | **0.0172** |     **288 B** |
|                           |            |        |            |           |           |        |           |
| **OneOf_ToString**            | **OneOf**      | **Int**    | **13.2647 ns** | **0.1992 ns** | **0.1766 ns** | **0.0033** |      **56 B** |
| **OneOf_ToString**            | **OneOf**      | **Text**   | **14.3306 ns** | **0.1599 ns** | **0.1336 ns** | **0.0043** |      **72 B** |
