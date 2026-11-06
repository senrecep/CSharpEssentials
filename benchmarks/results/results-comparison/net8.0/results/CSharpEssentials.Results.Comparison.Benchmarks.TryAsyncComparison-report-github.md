```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** | **19,264.78 ns** |  **18.383 ns** |  **15.351 ns** |  **1.00** | **0.0610** |    **1432 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False | 19,323.20 ns |  23.309 ns |  21.803 ns |  1.00 | 0.0610 |    1376 B |        0.96 |
| ErrorOr                    | False   | False | 19,367.02 ns |  16.633 ns |  13.889 ns |  1.01 | 0.0610 |    1464 B |        1.02 |
| FluentResults              | False   | False | 20,778.00 ns |  24.253 ns |  20.252 ns |  1.08 | 0.1221 |    2160 B |        1.51 |
|                            |         |       |              |            |            |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **23,723.78 ns** | **202.880 ns** | **189.774 ns** |  **1.00** | **0.0610** |    **1581 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 23,711.71 ns | 137.466 ns | 128.586 ns |  1.00 | 0.0610 |    1517 B |        0.96 |
| ErrorOr                    | False   | True  | 23,906.34 ns | 119.953 ns | 100.166 ns |  1.01 | 0.0916 |    1589 B |        1.01 |
| FluentResults              | False   | True  | 25,508.84 ns | 197.085 ns | 164.575 ns |  1.08 | 0.1221 |    2295 B |        1.45 |
|                            |         |       |              |            |            |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |     **40.99 ns** |   **0.248 ns** |   **0.232 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |     44.10 ns |   0.305 ns |   0.285 ns |  1.08 | 0.0091 |     152 B |        1.00 |
| ErrorOr                    | True    | False |     38.32 ns |   0.231 ns |   0.205 ns |  0.94 | 0.0091 |     152 B |        1.00 |
| FluentResults              | True    | False |     80.12 ns |   0.390 ns |   0.346 ns |  1.95 | 0.0157 |     264 B |        1.74 |
|                            |         |       |              |            |            |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  |    **849.60 ns** |   **4.151 ns** |   **3.883 ns** |  **1.00** | **0.0143** |     **240 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  |    875.56 ns |   5.654 ns |   5.289 ns |  1.03 | 0.0134 |     232 B |        0.97 |
| ErrorOr                    | True    | True  |    861.31 ns |   3.861 ns |   3.612 ns |  1.01 | 0.0124 |     216 B |        0.90 |
| FluentResults              | True    | True  |    908.55 ns |   9.271 ns |   8.673 ns |  1.07 | 0.0191 |     336 B |        1.40 |
