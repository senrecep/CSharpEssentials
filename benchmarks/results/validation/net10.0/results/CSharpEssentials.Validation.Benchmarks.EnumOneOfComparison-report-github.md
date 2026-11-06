```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Valid | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** | **399.77 ns** | **0.891 ns** | **0.834 ns** |  **1.00** |    **0.00** | **0.0491** |     **824 B** |        **1.00** |
| FluentValidation | False | 692.93 ns | 3.466 ns | 3.242 ns |  1.73 |    0.01 | 0.1097 |    1840 B |        2.23 |
|                  |       |           |          |          |       |         |        |           |             |
| **CSharpEssentials** | **True**  |  **48.13 ns** | **0.226 ns** | **0.200 ns** |  **1.00** |    **0.01** | **0.0081** |     **136 B** |        **1.00** |
| FluentValidation | True  | 114.06 ns | 1.600 ns | 1.418 ns |  2.37 |    0.03 | 0.0358 |     600 B |        4.41 |
