```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |    80.49 ns | 0.028 ns | 0.022 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    88.53 ns | 0.165 ns | 0.154 ns |  1.10 | 0.0019 |      32 B |          NA |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    | 5,348.06 ns | 9.399 ns | 8.332 ns |  1.00 | 0.0687 |    1208 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 5,668.48 ns | 6.160 ns | 5.762 ns |  1.06 | 0.0916 |    1648 B |        1.36 |
| Macross_InvalidThrow             | InvalidThrow    | 5,267.60 ns | 4.571 ns | 3.569 ns |  0.98 | 0.0839 |    1432 B |        1.19 |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_Read            | Read            | 1,143.03 ns | 2.446 ns | 2.168 ns |  1.00 | 0.0172 |     304 B |        1.00 |
| Bcl_Read                         | Read            | 1,112.77 ns | 2.659 ns | 2.487 ns |  0.97 | 0.0172 |     304 B |        1.00 |
| Macross_Read                     | Read            |   910.53 ns | 1.527 ns | 1.354 ns |  0.80 | 0.0572 |     968 B |        3.18 |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |    60.20 ns | 0.043 ns | 0.038 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |    51.49 ns | 0.048 ns | 0.045 ns |  0.86 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |    64.33 ns | 0.065 ns | 0.054 ns |  1.07 | 0.0014 |      24 B |          NA |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_Write           | Write           |   394.33 ns | 0.756 ns | 0.707 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |   322.93 ns | 1.254 ns | 1.173 ns |  0.82 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |   572.93 ns | 0.797 ns | 0.746 ns |  1.45 | 0.0229 |     384 B |        1.00 |
