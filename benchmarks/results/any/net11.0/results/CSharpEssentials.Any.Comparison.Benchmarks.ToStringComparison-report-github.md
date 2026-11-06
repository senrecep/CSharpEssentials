```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                    | Categories | Branch | Mean       | Error     | StdDev    | Gen0   | Allocated |
|-------------------------- |----------- |------- |-----------:|----------:|----------:|-------:|----------:|
| **CSharpEssentials_ToString** | **Cse**        | **Int**    |  **1.1423 ns** | **0.0247 ns** | **0.0231 ns** |      **-** |         **-** |
| **CSharpEssentials_ToString** | **Cse**        | **Text**   |  **0.6349 ns** | **0.0240 ns** | **0.0225 ns** |      **-** |         **-** |
|                           |            |        |            |           |           |        |           |
| **Dunet_ToString**            | **Dunet**      | **Int**    | **65.1344 ns** | **1.3495 ns** | **2.0199 ns** | **0.0162** |     **272 B** |
| **Dunet_ToString**            | **Dunet**      | **Text**   | **60.5160 ns** | **1.2319 ns** | **1.8439 ns** | **0.0172** |     **288 B** |
|                           |            |        |            |           |           |        |           |
| **OneOf_ToString**            | **OneOf**      | **Int**    | **13.0074 ns** | **0.3044 ns** | **0.2990 ns** | **0.0033** |      **56 B** |
| **OneOf_ToString**            | **OneOf**      | **Text**   | **14.3547 ns** | **0.3273 ns** | **0.3061 ns** | **0.0043** |      **72 B** |
