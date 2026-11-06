```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Valid | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials** | **False** | **522.30 ns** | **1.328 ns** | **1.177 ns** |  **1.00** | **0.0572** |     **968 B** |        **1.00** |
| FluentValidation | False | 885.59 ns | 5.267 ns | 4.927 ns |  1.70 | 0.1135 |    1904 B |        1.97 |
|                  |       |           |          |          |       |        |           |             |
| **CSharpEssentials** | **True**  |  **79.34 ns** | **0.360 ns** | **0.336 ns** |  **1.00** | **0.0119** |     **200 B** |        **1.00** |
| FluentValidation | True  | 151.31 ns | 0.932 ns | 0.826 ns |  1.91 | 0.0358 |     600 B |        3.00 |
