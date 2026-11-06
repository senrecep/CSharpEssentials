```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Valid | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** | **479.02 ns** | **3.054 ns** | **2.856 ns** |  **1.00** |    **0.01** | **0.0525** |     **888 B** |        **1.00** |
| FluentValidation | False | 826.98 ns | 6.271 ns | 5.866 ns |  1.73 |    0.02 | 0.1135 |    1904 B |        2.14 |
|                  |       |           |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |  **76.98 ns** | **0.694 ns** | **0.649 ns** |  **1.00** |    **0.01** | **0.0119** |     **200 B** |        **1.00** |
| FluentValidation | True  | 146.83 ns | 1.101 ns | 0.976 ns |  1.91 |    0.02 | 0.0358 |     600 B |        3.00 |
