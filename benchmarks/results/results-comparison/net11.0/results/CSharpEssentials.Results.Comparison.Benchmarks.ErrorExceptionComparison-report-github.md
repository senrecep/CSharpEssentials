```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                   | Thrown | Mean         | Error      | StdDev     | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |------- |-------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**         | **False**  |    **78.271 ns** |  **0.5745 ns** |  **0.5374 ns** |  **1.00** | **0.0129** |     **216 B** |        **1.00** |
| CSharpEssentials_Failure | False  |     5.189 ns |  0.0074 ns |  0.0070 ns |  0.07 |      - |         - |        0.00 |
| ErrorOr                  | False  |     5.199 ns |  0.0033 ns |  0.0026 ns |  0.07 |      - |         - |        0.00 |
| FluentResults            | False  |    24.944 ns |  0.3558 ns |  0.3328 ns |  0.32 | 0.0095 |     160 B |        0.74 |
|                          |        |              |            |            |       |        |           |             |
| **CSharpEssentials**         | **True**   | **2,603.245 ns** | **18.9547 ns** | **17.7302 ns** | **1.000** | **0.2747** |    **4720 B** |        **1.00** |
| CSharpEssentials_Failure | True   |     4.952 ns |  0.0385 ns |  0.0360 ns | 0.002 |      - |         - |        0.00 |
| ErrorOr                  | True   |     5.215 ns |  0.0027 ns |  0.0022 ns | 0.002 |      - |         - |        0.00 |
| FluentResults            | True   |    25.518 ns |  0.2969 ns |  0.2777 ns | 0.010 | 0.0095 |     160 B |        0.03 |
