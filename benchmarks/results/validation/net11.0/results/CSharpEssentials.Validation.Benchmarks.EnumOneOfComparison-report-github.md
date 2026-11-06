```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Valid | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** | **323.09 ns** | **2.793 ns** | **2.476 ns** |  **1.00** |    **0.01** | **0.0467** |     **784 B** |        **1.00** |
| FluentValidation | False | 732.71 ns | 3.720 ns | 3.480 ns |  2.27 |    0.02 | 0.1087 |    1832 B |        2.34 |
|                  |       |           |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |  **47.89 ns** | **0.158 ns** | **0.140 ns** |  **1.00** |    **0.00** | **0.0081** |     **136 B** |        **1.00** |
| FluentValidation | True  | 112.01 ns | 1.841 ns | 1.722 ns |  2.34 |    0.04 | 0.0358 |     600 B |        4.41 |
